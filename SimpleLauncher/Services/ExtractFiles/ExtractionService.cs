#nullable enable
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using SharpCompress.Archives;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.ExtractFiles;

public class ExtractionService : IExtractionService
{
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private readonly ILogErrors _logErrors;

    public ExtractionService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public async Task<(string? gameFilePath, string? tempDirectoryPath)> ExtractToTempAndGetLaunchFileAsync(string archivePath, List<string> fileFormatsToLaunch)
    {
        var pathToExtractionDirectory = await ExtractToTempAsync(archivePath);

        if (string.IsNullOrEmpty(pathToExtractionDirectory) || !Directory.Exists(pathToExtractionDirectory))
        {
            DebugLogger.Log($"[ExtractionService] Extraction failed for {archivePath}. No temp directory created or invalid path returned.");
            return (null, null);
        }

        var extractedFileToLaunch = await ValidateAndFindGameFileAsync(pathToExtractionDirectory, fileFormatsToLaunch);
        if (!string.IsNullOrEmpty(extractedFileToLaunch))
        {
            return (extractedFileToLaunch, pathToExtractionDirectory);
        }
        else
        {
            DebugLogger.Log($"[ExtractionService] No suitable game file found in extracted directory {pathToExtractionDirectory}.");
            return (null, pathToExtractionDirectory);
        }
    }

    public async Task<bool> ExtractToFolderAsync(string archivePath, string destinationFolder)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath) || new FileInfo(archivePath).Length == 0)
        {
            // Notify developer
            const string contextMessage = "File path is invalid.";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            MessageBoxLibrary.DownloadedFileIsMissingMessageBox();

            return false;
        }

        // Resolve the destination folder using PathHelper
        var resolvedDestinationFolder = PathHelper.ResolveRelativeToAppDirectory(destinationFolder);

        if (string.IsNullOrEmpty(resolvedDestinationFolder))
        {
            // Notify developer
            const string contextMessage = "Destination folder path resolution failed.";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }

            // Add a retry loop to handle transient file locks (e.g., from antivirus)
            const int maxRetries = 10;
            const int retryDelayMs = 1000;
        for (var i = 0; i < maxRetries; i++)
        {
            if (!CheckForFileLock.CheckForFileLock.IsFileLocked(archivePath))
            {
                break; // File is not locked, proceed
            }

            if (i == maxRetries - 1)
            {
                // Last attempt failed
                // Notify developer
                var contextMessage = $"The downloaded file appears to be locked after {maxRetries} retries: {archivePath}";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user, passing the directory of the locked archive
                MessageBoxLibrary.FileIsLockedMessageBox(Path.GetDirectoryName(archivePath));

                return false;
            }

            await Task.Delay(retryDelayMs); // Wait before retrying
        }

        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify developer
            var contextMessage = $"Only 7z, ZIP, and RAR files are supported by this extraction method.\n" + $"File type: {extension}";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return false;
        }

        try
        {
            try
            {
                // Use the resolved destination folder for creation
                Directory.CreateDirectory(resolvedDestinationFolder);
            }
            catch (Exception ex)
            {
                // Notify developer
                _logErrors.LogAndForget(ex, $"Failed to create directory: {resolvedDestinationFolder}");
            }

            // Create a tracking file in the resolved destination folder
            var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
            await File.WriteAllTextAsync(extractionTrackingFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            await Task.Run(() =>
            {
                using var archive = ArchiveFactory.OpenArchive(archivePath);
                var entries = archive.Entries.ToList();

                if (entries.Count == 0)
                {
                    throw new InvalidDataException("The archive file contains no entries.");
                }

                var estimatedSize = EstimateExtractedSize(archivePath);

                // Check disk space using the resolved destination folder
                var rootPath = Path.GetPathRoot(resolvedDestinationFolder);
                if (!string.IsNullOrEmpty(rootPath))
                {
                    try
                    {
                        var drive = new DriveInfo(rootPath);
                        if (drive.IsReady && drive.AvailableFreeSpace < estimatedSize)
                        {
                            // Notify developer
                            var contextMessage = $"Not enough disk space for extraction. Required: {estimatedSize / (1024 * 1024)} MB, Available: {drive.AvailableFreeSpace / (1024 * 1024)} MB";
                            _logErrors.LogAndForget(null, contextMessage);

                            // Notify user
                            MessageBoxLibrary.DiskSpaceErrorMessageBox();

                            throw new IOException("Insufficient disk space.");
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        // Notify developer
                        _logErrors.LogAndForget(ex, $"Unable to check disk space for path {resolvedDestinationFolder}: {ex.Message}");

                        // Notify user
                        MessageBoxLibrary.CouldNotCheckForDiskSpaceMessageBox();

                        throw new IOException($"Unable to check disk space for path {resolvedDestinationFolder}", ex);
                    }
                }

                // Path traversal check
                var fullResolvedDestFolder = PathHelper.ResolveRelativeToAppDirectory(resolvedDestinationFolder);
                if (!fullResolvedDestFolder.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    fullResolvedDestFolder += Path.DirectorySeparatorChar;
                }

                foreach (var entry in entries)
                {
                    if (entry.Key != null)
                    {
                        var entryDestinationPath = Path.GetFullPath(Path.Combine(resolvedDestinationFolder, entry.Key));
                        var fullDestPath = PathHelper.ResolveRelativeToAppDirectory(entryDestinationPath);

                        if (!fullDestPath.StartsWith(fullResolvedDestFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            // Notify user
                            MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(archivePath);
                            throw new SecurityException($"Potentially dangerous zip entry path: {entry.Key}");
                        }
                    }
                }

                // Extract all entries
                foreach (var entry in entries)
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }

                    if (entry.Key != null)
                    {
                        var destinationPath = Path.Combine(resolvedDestinationFolder, entry.Key);
                        var directory = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        using (var entryStream = entry.OpenEntryStream())
                        using (var fileStream = File.Create(destinationPath))
                        {
                            entryStream.CopyTo(fileStream);
                        }

                        // Preserve file time if available
                        if (entry.LastModifiedTime.HasValue)
                        {
                            File.SetLastWriteTime(destinationPath, entry.LastModifiedTime.Value);
                        }
                    }
                }
            });

            if (File.Exists(extractionTrackingFile))
            {
                DeleteFiles.TryDeleteFile(extractionTrackingFile);
            }

            return true;
        }
        catch (Exception ex)
        {
            // For .7z files, try fallback extraction with 7za executable
            if (extension == ".7z" && !string.IsNullOrEmpty(resolvedDestinationFolder))
            {
                DebugLogger.Log($"[ExtractionService] SharpCompress failed for .7z file, trying 7za fallback: {archivePath}");
                var fallbackSuccess = await ExtractWith7ZipAsync(archivePath, resolvedDestinationFolder);
                if (fallbackSuccess)
                {
                    DebugLogger.Log($"[ExtractionService] 7za fallback extraction succeeded for: {archivePath}");
                    var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
                    if (File.Exists(extractionTrackingFile))
                    {
                        DeleteFiles.TryDeleteFile(extractionTrackingFile);
                    }

                    return true;
                }

                DebugLogger.Log($"[ExtractionService] 7za fallback extraction also failed for: {archivePath}");
            }

            if (!string.IsNullOrEmpty(resolvedDestinationFolder)) // Only attempt cleanup if resolution was successful
            {
                try
                {
                    var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
                    if (File.Exists(extractionTrackingFile))
                    {
                        await CleanTempFolder.CleanupPartialExtractionAsync(resolvedDestinationFolder);
                    }
                }
                catch (Exception cleanupEx)
                {
                    // Notify developer
                    var contextMessage = $"Failed to clean up partial extraction in: {resolvedDestinationFolder}";
                    _logErrors.LogAndForget(cleanupEx, contextMessage);
                }
            }

            // Notify developer
            var exceptionDetails = GetDetailedExceptionInfo(ex);
            _logErrors.LogAndForget(ex, $"Error extracting the file: {archivePath}\n{exceptionDetails}");

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }
    }

    private async Task<string?> ExtractToTempAsync(string archivePath)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
        {
            // Notify developer
            const string contextMessage = "Archive path cannot be null, empty, or file not found.";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return null;
        }

        string? tempDirectory = null;

        try
        {
            if (string.IsNullOrEmpty(_tempFolder))
            {
                // Notify developer
                const string contextMessage = "Temp folder resolution failed.";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return null;
            }

            var randomName = Path.GetRandomFileName();
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                randomName = Guid.NewGuid().ToString("N");
            }

            tempDirectory = Path.Combine(_tempFolder, randomName);
            Directory.CreateDirectory(tempDirectory);

            await Task.Run(() =>
            {
                using var archive = ArchiveFactory.OpenArchive(archivePath);

                // First, validate for path traversal before extracting
                var fullTempDir = Path.GetFullPath(tempDirectory);
                if (!fullTempDir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    fullTempDir += Path.DirectorySeparatorChar;
                }

                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory) continue;

                    if (entry.Key != null)
                    {
                        var fullDestPath = Path.GetFullPath(Path.Combine(fullTempDir, entry.Key));
                        if (!fullDestPath.StartsWith(fullTempDir, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new SecurityException($"Potential path traversal detected in archive entry: {entry.Key}");
                        }
                    }
                }

                // If validation passes, extract the archive.
                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory) continue;

                    if (entry.Key != null)
                    {
                        var destinationPath = Path.Combine(tempDirectory, entry.Key);
                        var directory = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        using (var entryStream = entry.OpenEntryStream())
                        using (var fileStream = File.Create(destinationPath))
                        {
                            entryStream.CopyTo(fileStream);
                        }

                        // Preserve file time if available
                        if (entry.LastModifiedTime.HasValue)
                        {
                            File.SetLastWriteTime(destinationPath, entry.LastModifiedTime.Value);
                        }
                    }
                }
            });

            return tempDirectory;
        }
        catch (Exception ex)
        {
            // For .7z files, try fallback extraction with 7za executable
            if (extension == ".7z" && !string.IsNullOrEmpty(tempDirectory))
            {
                DebugLogger.Log($"[ExtractionService] SharpCompress failed for .7z file, trying 7za fallback: {archivePath}");
                var fallbackSuccess = await ExtractWith7ZipAsync(archivePath, tempDirectory);
                if (fallbackSuccess)
                {
                    DebugLogger.Log($"[ExtractionService] 7za fallback extraction succeeded for: {archivePath}");
                    return tempDirectory;
                }

                DebugLogger.Log($"[ExtractionService] 7za fallback extraction also failed for: {archivePath}");
            }

            await CleanTempFolder.CleanupTempDirectoryAsync(tempDirectory);

            // Notify developer
            const string contextMessage = "Extraction of the compressed file failed. The file may be corrupted or a security issue was detected.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }
    }

    private async Task<bool> ExtractWith7ZipAsync(string archivePath, string destinationFolder)
    {
        var arch = RuntimeInformation.ProcessArchitecture;
        var exeName = arch == Architecture.Arm64 ? "7za_arm64.exe" : "7za.exe";
        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "SevenZip", exeName);

        if (!File.Exists(exePath))
        {
            DebugLogger.Log($"[ExtractionService] 7-Zip executable not found at: {exePath}");
            return false;
        }

        try
        {
            Directory.CreateDirectory(destinationFolder);

            var args = $"x -o\"{destinationFolder}\" -y \"{archivePath}\"";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            DebugLogger.Log($"[ExtractionService] Running 7za fallback for: {archivePath}");
            process.Start();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                DebugLogger.Log("[ExtractionService] 7za extraction timed out after 30 minutes.");
                try
                {
                    process.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return false;
            }

            if (process.ExitCode == 0)
            {
                DebugLogger.Log($"[ExtractionService] 7za extraction succeeded for: {archivePath}");
                return true;
            }

            DebugLogger.Log($"[ExtractionService] 7za extraction failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return false;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[ExtractionService] 7za extraction exception: {ex.Message}");
            _logErrors.LogAndForget(ex, $"Error extracting with 7za: {archivePath}");
            return false;
        }
    }

    private static long EstimateExtractedSize(string archivePath)
    {
        long totalSize;
        using (var archive = ArchiveFactory.OpenArchive(archivePath))
        {
            totalSize = archive.Entries.Where(static entry => !entry.IsDirectory).Sum(static entry => entry.Size);
        }

        // Add a safety margin of 20%
        return (long)(totalSize * 1.2);
    }

    private static string GetDetailedExceptionInfo(Exception ex)
    {
        var sb = new StringBuilder();

        var currentEx = ex;
        var level = 0;

        while (currentEx != null)
        {
            // Corrected the ambiguous invocation by using FormattableString.Invariant
            sb.AppendLine(FormattableString.Invariant($"[Level {level}] {currentEx.GetType().Name}: {currentEx.Message}"));
            level++;
            currentEx = currentEx.InnerException;
        }

        return sb.ToString();
    }

    private Task<string?> ValidateAndFindGameFileAsync(string tempExtractLocation, List<string> fileFormatsToLaunch)
    {
        DebugLogger.Log($"[ValidateAndFindGameFileAsync] Validating extracted path: {tempExtractLocation}");
        if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
        {
            // Notify developer
            var contextMessage = $"Extracted path is invalid: {tempExtractLocation}";
            _logErrors.LogAndForget(null, contextMessage);
            DebugLogger.Log($"[ValidateAndFindGameFileAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return Task.FromResult<string?>(null);
        }

        string? foundFile = null;

        // First, try to find a file matching the specified formats, if any are provided.
        if (fileFormatsToLaunch is { Count: > 0 })
        {
            DebugLogger.Log($"[ValidateAndFindGameFileAsync] Searching for formats: {string.Join(", ", fileFormatsToLaunch)} in {tempExtractLocation}");
            foreach (var formatToLaunch in fileFormatsToLaunch)
            {
                try
                {
                    var searchPattern = $"*{formatToLaunch}";
                    if (!formatToLaunch.StartsWith('.'))
                    {
                        searchPattern = $"*.{formatToLaunch}";
                    }

                    var files = Directory.GetFiles(tempExtractLocation, searchPattern, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        foundFile = files[0]; // Take the first match
                        DebugLogger.Log($"[ValidateAndFindGameFileAsync] Found file matching format '{formatToLaunch}': {foundFile}");
                        return Task.FromResult<string?>(foundFile);
                    }
                }
                catch (Exception ex)
                {
                    _logErrors.LogAndForget(ex, $"Error searching for file format '{formatToLaunch}' in '{tempExtractLocation}'.");
                    DebugLogger.Log($"[ValidateAndFindGameFileAsync] Exception searching for {formatToLaunch}: {ex.Message}");
                    // Continue to next format or fallback if this one fails
                }
            }
        }
        else
        {
            DebugLogger.Log($"[ValidateAndFindGameFileAsync] fileFormatsToLaunch is null or empty. Attempting to find any file in {tempExtractLocation}.");
        }

        // If no specific format was found, or no formats were specified, try to find any file.
        if (string.IsNullOrEmpty(foundFile))
        {
            try
            {
                var allFiles = Directory.EnumerateFiles(tempExtractLocation, "*", SearchOption.AllDirectories).OrderBy(static f => f).ToList();
                if (allFiles.Count > 0)
                {
                    foundFile = allFiles.First();
                    DebugLogger.Log($"[ValidateAndFindGameFileAsync] No specific format found/specified, picked first file: {foundFile}");
                    return Task.FromResult<string?>(foundFile);
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, $"Error enumerating all files in {tempExtractLocation} as a fallback.");
                DebugLogger.Log($"[ValidateAndFindGameFileAsync] Error enumerating all files: {ex.Message}");
            }
        }

        // If still no file found after all attempts
        const string notFoundContext = "Could not find a file with any of the specified extensions (or any file at all) after extraction.";
        _logErrors.LogAndForget(new FileNotFoundException(notFoundContext), notFoundContext);
        DebugLogger.Log($"[ValidateAndFindGameFileAsync] Error: {notFoundContext}");

        MessageBoxLibrary.CouldNotFindAFileMessageBox(); // This message is now more general.

        return Task.FromResult<string?>(null);
    }
}