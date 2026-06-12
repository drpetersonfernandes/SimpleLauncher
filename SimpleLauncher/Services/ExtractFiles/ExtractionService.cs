#nullable enable

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using SharpCompress.Archives;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using FileLock = SimpleLauncher.Services.CheckForFileLock.CheckForFileLock;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.ExtractFiles;

/// <summary>
/// Extracts compressed game archives (7z, ZIP, RAR) to temporary or permanent locations,
/// with support for path traversal protection, disk space checks, and 7za fallback.
/// </summary>
public class ExtractionService : IExtractionService
{
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly IDebugLogger _debugLogger;

    /// <summary>
    /// Initializes a new instance of <see cref="ExtractionService"/>.
    /// </summary>
    /// <param name="logErrors">Error logging service.</param>
    /// <param name="messageBoxLibrary">Service for displaying user-facing message boxes.</param>
    /// <param name="debugLogger">Debug logging service.</param>
    public ExtractionService(ILogErrors logErrors, IMessageBoxLibraryService messageBoxLibrary, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _messageBoxLibrary = messageBoxLibrary;
        _debugLogger = debugLogger;
    }

    public async Task<(string? gameFilePath, string? tempDirectoryPath)> ExtractToTempAndGetLaunchFileAsync(string archivePath, List<string> fileFormatsToLaunch)
    {
        var pathToExtractionDirectory = await ExtractToTempAsync(archivePath);

        if (string.IsNullOrEmpty(pathToExtractionDirectory) || !Directory.Exists(pathToExtractionDirectory))
        {
            _debugLogger.Log($"[ExtractionService] Extraction failed for {archivePath}. No temp directory created or invalid path returned.");
            return (null, null);
        }

        var extractedFileToLaunch = await ValidateAndFindGameFileAsync(pathToExtractionDirectory, fileFormatsToLaunch);
        if (!string.IsNullOrEmpty(extractedFileToLaunch))
        {
            return (extractedFileToLaunch, pathToExtractionDirectory);
        }
        else
        {
            _debugLogger.Log($"[ExtractionService] No suitable game file found in extracted directory {pathToExtractionDirectory}.");
            return (null, pathToExtractionDirectory);
        }
    }

    /// <summary>
    /// Extracts an archive to the specified destination folder with retry logic for file locks,
    /// disk space validation, and path traversal protection.
    /// </summary>
    /// <param name="archivePath">The full path to the archive file.</param>
    /// <param name="destinationFolder">The target folder for extraction.</param>
    /// <returns>True if extraction succeeded; otherwise false.</returns>
    public async Task<bool> ExtractToFolderAsync(string archivePath, string destinationFolder)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath) || new FileInfo(archivePath).Length == 0)
        {
            // Notify developer
            const string contextMessage = "File path is invalid.";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            await _messageBoxLibrary.DownloadedFileIsMissingMessageBoxAsync();

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
            await _messageBoxLibrary.ExtractionFailedMessageBoxAsync();

            return false;
        }

        // Add a retry loop to handle transient file locks (e.g., from antivirus)
        const int maxRetries = 10;
        const int retryDelayMs = 1000;
        for (var i = 0; i < maxRetries; i++)
        {
            if (!FileLock.IsFileLocked(archivePath))
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
                await _messageBoxLibrary.FileIsLockedMessageBoxAsync(Path.GetDirectoryName(archivePath));

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
            await _messageBoxLibrary.FileNeedToBeCompressedMessageBoxAsync();

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

            await Task.Run(async () =>
            {
                using var archive = ArchiveFactory.OpenArchive(archivePath);
                var entries = archive.Entries.ToList();

                if (entries.Count == 0)
                {
                    throw new InvalidDataException("The archive file contains no entries.");
                }

                var estimatedSize = (long)(entries.Where(static e => !e.IsDirectory).Sum(static e => e.Size) * 1.2);

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
                            await _messageBoxLibrary.DiskSpaceErrorMessageBoxAsync();

                            throw new IOException("Insufficient disk space.");
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        // Notify developer
                        _logErrors.LogAndForget(ex, $"Unable to check disk space for path {resolvedDestinationFolder}: {ex.Message}");

                        // Notify user
                        await _messageBoxLibrary.CouldNotCheckForDiskSpaceMessageBoxAsync();

                        throw new IOException($"Unable to check disk space for path {resolvedDestinationFolder}", ex);
                    }
                }

                // Path traversal check
                var fullResolvedDestFolder = PathHelper.ResolveRelativeToAppDirectory(resolvedDestinationFolder);
                if (fullResolvedDestFolder != null && !fullResolvedDestFolder.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    fullResolvedDestFolder += Path.DirectorySeparatorChar;
                }

                foreach (var entry in entries)
                {
                    if (entry.Key != null)
                    {
                        var entryDestinationPath = Path.GetFullPath(Path.Combine(resolvedDestinationFolder, entry.Key));
                        var fullDestPath = PathHelper.ResolveRelativeToAppDirectory(entryDestinationPath);

                        if (fullDestPath != null && fullResolvedDestFolder != null && !fullDestPath.StartsWith(fullResolvedDestFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            // Notify user
                            await _messageBoxLibrary.PotentialPathManipulationDetectedMessageBoxAsync(archivePath);
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

                        await using (var entryStream = await entry.OpenEntryStreamAsync())
                        await using (var fileStream = File.Create(destinationPath))
                        {
                            await entryStream.CopyToAsync(fileStream);
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
                await DeleteFiles.TryDeleteFileAsync(extractionTrackingFile);
            }

            return true;
        }
        catch (Exception ex)
        {
            // For .7z files, try fallback extraction with 7za executable
            if (extension == ".7z" && !string.IsNullOrEmpty(resolvedDestinationFolder))
            {
                _debugLogger.Log($"[ExtractionService] SharpCompress failed for .7z file, trying 7za fallback: {archivePath}");
                var fallbackSuccess = await ExtractWith7ZipAsync(archivePath, resolvedDestinationFolder);
                if (fallbackSuccess)
                {
                    _debugLogger.Log($"[ExtractionService] 7za fallback extraction succeeded for: {archivePath}");
                    var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
                    if (File.Exists(extractionTrackingFile))
                    {
                        await DeleteFiles.TryDeleteFileAsync(extractionTrackingFile);
                    }

                    return true;
                }

                _debugLogger.Log($"[ExtractionService] 7za fallback extraction also failed for: {archivePath}");
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
            await _messageBoxLibrary.ExtractionFailedMessageBoxAsync();

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
            await _messageBoxLibrary.ExtractionFailedMessageBoxAsync();

            return null;
        }

        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            await _messageBoxLibrary.FileNeedToBeCompressedMessageBoxAsync();

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
                await _messageBoxLibrary.ExtractionFailedMessageBoxAsync();

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
                _debugLogger.Log($"[ExtractionService] SharpCompress failed for .7z file, trying 7za fallback: {archivePath}");
                var fallbackSuccess = await ExtractWith7ZipAsync(archivePath, tempDirectory);
                if (fallbackSuccess)
                {
                    _debugLogger.Log($"[ExtractionService] 7za fallback extraction succeeded for: {archivePath}");
                    return tempDirectory;
                }

                _debugLogger.Log($"[ExtractionService] 7za fallback extraction also failed for: {archivePath}");
            }

            await CleanTempFolder.CleanupTempDirectoryAsync(tempDirectory);

            // Notify developer
            const string contextMessage = "Extraction of the compressed file failed. The file may be corrupted or a security issue was detected.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBoxLibrary.ExtractionFailedMessageBoxAsync();

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
            _debugLogger.Log($"[ExtractionService] 7-Zip executable not found at: {exePath}");
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

            _debugLogger.Log($"[ExtractionService] Running 7za fallback for: {archivePath}");
            process.Start();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _debugLogger.Log("[ExtractionService] 7za extraction timed out after 30 minutes.");
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ExtractionService] process.Kill() failed: {ex.Message}");
                }

                return false;
            }

            if (process.ExitCode == 0)
            {
                _debugLogger.Log($"[ExtractionService] 7za extraction succeeded for: {archivePath}");
                return true;
            }

            _debugLogger.Log($"[ExtractionService] 7za extraction failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return false;
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[ExtractionService] 7za extraction exception: {ex.Message}");
            _logErrors.LogAndForget(ex, $"Error extracting with 7za: {archivePath}");
            return false;
        }
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

    private async Task<string?> ValidateAndFindGameFileAsync(string tempExtractLocation, List<string> fileFormatsToLaunch)
    {
        _debugLogger.Log($"[ValidateAndFindGameFileAsync] Validating extracted path: {tempExtractLocation}");
        if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
        {
            // Notify developer
            var contextMessage = $"Extracted path is invalid: {tempExtractLocation}";
            _logErrors.LogAndForget(null, contextMessage);
            _debugLogger.Log($"[ValidateAndFindGameFileAsync] Error: {contextMessage}");

            // Notify user
            await _messageBoxLibrary.ExtractionFailedMessageBoxAsync();

            return null;
        }

        string? foundFile = null;

        // First, try to find a file matching the specified formats, if any are provided.
        if (fileFormatsToLaunch is { Count: > 0 })
        {
            _debugLogger.Log($"[ValidateAndFindGameFileAsync] Searching for formats: {string.Join(", ", fileFormatsToLaunch)} in {tempExtractLocation}");
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
                        _debugLogger.Log($"[ValidateAndFindGameFileAsync] Found file matching format '{formatToLaunch}': {foundFile}");
                        return foundFile;
                    }
                }
                catch (Exception ex)
                {
                    _logErrors.LogAndForget(ex, $"Error searching for file format '{formatToLaunch}' in '{tempExtractLocation}'.");
                    _debugLogger.Log($"[ValidateAndFindGameFileAsync] Exception searching for {formatToLaunch}: {ex.Message}");
                    // Continue to next format or fallback if this one fails
                }
            }
        }
        else
        {
            _debugLogger.Log($"[ValidateAndFindGameFileAsync] fileFormatsToLaunch is null or empty. Attempting to find any file in {tempExtractLocation}.");
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
                    _debugLogger.Log($"[ValidateAndFindGameFileAsync] No specific format found/specified, picked first file: {foundFile}");
                    return foundFile;
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, $"Error enumerating all files in {tempExtractLocation} as a fallback.");
                _debugLogger.Log($"[ValidateAndFindGameFileAsync] Error enumerating all files: {ex.Message}");
            }
        }

        // If still no file found after all attempts
        const string notFoundContext = "Could not find a file with any of the specified extensions (or any file at all) after extraction.";
        _logErrors.LogAndForget(new FileNotFoundException(notFoundContext), notFoundContext);
        _debugLogger.Log($"[ValidateAndFindGameFileAsync] Error: {notFoundContext}");

        await _messageBoxLibrary.CouldNotFindAFileMessageBoxAsync(); // This message is now more general.

        return null;
    }
}