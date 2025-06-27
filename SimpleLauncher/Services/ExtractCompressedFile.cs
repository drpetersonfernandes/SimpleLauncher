using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;

namespace SimpleLauncher.Services;

public class ExtractCompressedFile
{
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    /// <summary>
    /// Method to Extract 7z and Rar files from GameLauncher
    /// It extracts to temp folder
    /// Use 7z executable
    /// </summary>
    /// <param name="archivePath">Full path to the archive file</param>
    /// <returns>Path to the extraction directory or null if extraction failed</returns>
    public async Task<string> ExtractWith7ZToTempAsync(string archivePath)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
        {
            const string contextMessage = "Archive path cannot be null, empty, or file not found.";

            // Notify developer
            _ = LogErrors.LogErrorAsync(null, contextMessage);

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

        var sevenZipPath = Get7ZipPath.Get7ZipExecutablePath();
        if (string.IsNullOrEmpty(sevenZipPath) || !File.Exists(sevenZipPath))
        {
            // Notify developer
            const string contextMessage = "7-Zip executable not found or is inaccessible.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        string tempDirectory = null;
        PleaseWaitWindow pleaseWaitExtraction = null;

        try
        {
            if (string.IsNullOrEmpty(_tempFolder))
            {
                // Notify developer
                const string contextMessage = "Temp folder resolution failed.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return null;
            }

            var randomName = Path.GetRandomFileName();
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                randomName = Guid.NewGuid().ToString("N");
            }

            // Combine the resolved temp folder with the random name
            tempDirectory = Path.Combine(_tempFolder, randomName);
            try
            {
                Directory.CreateDirectory(tempDirectory);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Could not create the temp directory.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return null;
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var extractionMessage = Application.Current.TryFindResource("Extractingcompressed").ToString() ?? "Extracting compressed file...";
                pleaseWaitExtraction = new PleaseWaitWindow(extractionMessage);
                pleaseWaitExtraction.Show();
            });

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"x \"{EscapeCommandLineArgument(archivePath)}\" -o\"{EscapeCommandLineArgument(tempDirectory)}\" -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.EnableRaisingEvents = true;

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null) outputBuilder.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null) errorBuilder.AppendLine(args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                // VerifyNoPathTraversalInExtractedFiles uses PathHelper internally
                if (VerifyNoPathTraversalInExtractedFiles(tempDirectory, tempDirectory))
                {
                    return tempDirectory;
                }
                else
                {
                    // Notify developer
                    MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(archivePath);

                    throw new SecurityException("Potential path traversal detected in archive contents");
                }
            }

            var errorMessage = $"Extraction of the compressed file failed.\n\n" +
                               $"Exit code: {process.ExitCode}\n" +
                               $"Output: {outputBuilder}\n" +
                               $"Error: {errorBuilder}";
            throw new Exception(errorMessage);
        }
        catch (Exception ex)
        {
            CleanFolder.CleanupTempDirectory(tempDirectory);

            // Notify developer
            const string catchContextMessage = $"Extraction of the compressed file failed.\n" + // Renamed
                                               $"The file may be corrupted.";
            _ = LogErrors.LogErrorAsync(ex, catchContextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }
        finally
        {
            if (pleaseWaitExtraction != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    pleaseWaitExtraction.Close();
                    pleaseWaitExtraction = null;
                });
            }
        }
    }

    /// <summary>
    /// Use to extract Zip files from GameLauncher
    /// Use SharpZipLib library to extract files
    /// It extracts to temp folder
    /// </summary>
    /// <param name="archivePath">Full path to the zip file</param>
    /// <returns>Path to the extraction directory or null if extraction failed</returns>
    public async Task<string> ExtractWithNativeLibraryToTempAsync(string archivePath)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
        {
            // Notify developer
            const string contextMessage = "Archive path cannot be null, empty, or file not found.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".zip")
        {
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();
            return null;
        }

        string tempDirectory = null;
        PleaseWaitWindow pleaseWaitExtraction = null;
        var extractionSuccessful = false;

        try
        {
            if (string.IsNullOrEmpty(_tempFolder))
            {
                // Notify developer
                const string contextMessage = "Temp folder resolution failed.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return null;
            }

            var randomName = Path.GetRandomFileName();
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                randomName = Guid.NewGuid().ToString("N");
            }

            // Combine the resolved temp folder with the random name
            tempDirectory = Path.Combine(_tempFolder, randomName);
            try
            {
                Directory.CreateDirectory(tempDirectory);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Could not create the temp directory.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return null;
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var extractionMessage = Application.Current.TryFindResource("Extractingcompressed").ToString() ?? "Extracting compressed file...";
                pleaseWaitExtraction = new PleaseWaitWindow(extractionMessage);
                pleaseWaitExtraction.Show();
            });

            await Task.Run(() =>
            {
                try
                {
                    using (var zipFile = new ZipFile(archivePath))
                    {
                        if (zipFile.Count == 0)
                        {
                            throw new InvalidDataException("The ZIP file contains no entries.");
                        }

                        var estimatedSize = EstimateExtractedSize(archivePath);
                        var rootPath = Path.GetPathRoot(Path.GetFullPath(tempDirectory)); // Use tempDirectory for drive check

                        if (!string.IsNullOrEmpty(rootPath))
                        {
                            try
                            {
                                var drive = new DriveInfo(rootPath);
                                if (drive.IsReady && drive.AvailableFreeSpace < estimatedSize)
                                {
                                    // Notify developer
                                    var contextMessage = $"Not enough disk space for extraction. Required: {estimatedSize / (1024 * 1024)} MB, Available: {drive.AvailableFreeSpace / (1024 * 1024)} MB";
                                    _ = LogErrors.LogErrorAsync(null, contextMessage);

                                    // Notify user
                                    MessageBoxLibrary.DiskSpaceErrorMessageBox();

                                    return;
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                // Notify developer
                                _ = LogErrors.LogErrorAsync(ex, $"Unable to check disk space for path {tempDirectory}: {ex.Message}");

                                // Notify user
                                MessageBoxLibrary.CouldNotCheckForDiskSpaceMessageBox();

                                return;
                            }
                        }

                        // Path traversal check
                        var fullTempDir = PathHelper.ResolveRelativeToCurrentWorkingDirectory(tempDirectory);
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            var entryDestinationPath = Path.GetFullPath(Path.Combine(tempDirectory, zipEntry.Name));
                            var fullDestPath = PathHelper.ResolveRelativeToCurrentWorkingDirectory(entryDestinationPath);

                            if (fullDestPath.StartsWith(fullTempDir, StringComparison.OrdinalIgnoreCase)) continue;

                            MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(archivePath);
                            throw new SecurityException($"Potentially dangerous zip entry path: {zipEntry.Name}");
                        }
                    }

                    // Extract using FastZip
                    var fastZip = new FastZip();
                    fastZip.ExtractZip(archivePath, tempDirectory, FastZip.Overwrite.Always, null, null, null, true);
                }
                catch (Exception)
                {
                    throw;
                }
            });

            var extractionTrackingFile = Path.Combine(tempDirectory, ".extraction_in_progress");
            if (File.Exists(extractionTrackingFile))
            {
                DeleteFiles.TryDeleteFile(extractionTrackingFile);
            }

            extractionSuccessful = true; // Set to true on successful extraction
            return tempDirectory;
        }
        catch (Exception ex)
        {
            CleanFolder.CleanupTempDirectory(tempDirectory);

            // Notify developer
            var catchContextMessage = $"Extraction of the compressed file failed.\n" +
                                      $"The file may be corrupted.\n" +
                                      $"File: {archivePath}";
            _ = LogErrors.LogErrorAsync(ex, catchContextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }
        finally
        {
            // Use tempDirectory for cleanup
            if (!extractionSuccessful && tempDirectory != null) // Check extractionSuccessful and tempDirectory
            {
                try
                {
                    var extractionTrackingFile = Path.Combine(tempDirectory, ".extraction_in_progress");
                    if (File.Exists(extractionTrackingFile))
                    {
                        CleanFolder.CleanupPartialExtraction(tempDirectory);
                    }
                }
                catch (Exception cleanupEx)
                {
                    // Notify developer
                    var contextMessage = $"Failed to clean up partial extraction in: {tempDirectory}";
                    _ = LogErrors.LogErrorAsync(cleanupEx, contextMessage);
                }
            }

            if (pleaseWaitExtraction != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    pleaseWaitExtraction.Close();
                    pleaseWaitExtraction = null;
                });
            }
        }
    }

    /// <summary>
    /// Extracts downloaded compressed files to a specified destination folder.
    /// This method extracts files inside the 'Simple Launcher' folder
    /// and requires appropriate permissions.
    /// </summary>
    /// <param name="filePath">The full path to the compressed file</param>
    /// <param name="destinationFolder">The destination folder where files will be extracted</param>
    /// <returns>True if extraction was successful, false otherwise</returns>
    public static async Task<bool> ExtractDownloadFilesToBaseFolderAsync(string filePath, string destinationFolder)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            // Notify developer
            const string contextMessage = "File path is invalid.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

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
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }

        if (CheckForFileLock.IsFileLocked(filePath))
        {
            // Notify developer
            var contextMessage = $"The downloaded file appears to be locked: {filePath}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.FileIsLockedMessageBox();

            return false;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".zip")
        {
            // Notify developer
            var contextMessage = $"Only ZIP files are supported by this extraction method.\n" +
                                 $"File type: {extension}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

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
                _ = LogErrors.LogErrorAsync(ex, $"Failed to create directory: {resolvedDestinationFolder}");
            }

            // Create a tracking file in the resolved destination folder
            var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
            await File.WriteAllTextAsync(extractionTrackingFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            await Task.Run(() =>
            {
                try
                {
                    using (var zipFile = new ZipFile(filePath))
                    {
                        if (zipFile.Count == 0)
                        {
                            throw new InvalidDataException("The ZIP file contains no entries.");
                        }

                        var estimatedSize = EstimateExtractedSize(filePath);

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
                                    _ = LogErrors.LogErrorAsync(null, contextMessage);

                                    // Notify user
                                    MessageBoxLibrary.DiskSpaceErrorMessageBox();

                                    return;
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                // Notify developer
                                _ = LogErrors.LogErrorAsync(ex, $"Unable to check disk space for path {resolvedDestinationFolder}: {ex.Message}");

                                // Notify user
                                MessageBoxLibrary.CouldNotCheckForDiskSpaceMessageBox();

                                return;
                            }
                        }

                        // Path traversal check
                        var fullResolvedDestFolder = PathHelper.ResolveRelativeToAppDirectory(resolvedDestinationFolder);
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            var entryDestinationPath = Path.GetFullPath(Path.Combine(resolvedDestinationFolder, zipEntry.Name));
                            var fullDestPath = PathHelper.ResolveRelativeToAppDirectory(entryDestinationPath);

                            if (fullDestPath.StartsWith(fullResolvedDestFolder, StringComparison.OrdinalIgnoreCase))
                                continue;

                            MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(filePath);
                            throw new SecurityException($"Potentially dangerous zip entry path: {zipEntry.Name}");
                        }
                    }

                    // Extract using FastZip
                    var fastZip = new FastZip();
                    fastZip.ExtractZip(filePath, resolvedDestinationFolder, FastZip.Overwrite.Always, null, null, null, true);
                }
                catch (Exception)
                {
                    throw;
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
            if (!string.IsNullOrEmpty(resolvedDestinationFolder)) // Only attempt cleanup if resolution was successful
            {
                try
                {
                    var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
                    if (File.Exists(extractionTrackingFile))
                    {
                        CleanFolder.CleanupPartialExtraction(resolvedDestinationFolder);
                    }
                }
                catch (Exception cleanupEx)
                {
                    // Notify developer
                    var contextMessage = $"Failed to clean up partial extraction in: {resolvedDestinationFolder}";
                    _ = LogErrors.LogErrorAsync(cleanupEx, contextMessage);
                }
            }

            // Notify developer
            var exceptionDetails = GetDetailedExceptionInfo(ex);
            var catchContextMessage = $"Error extracting the file: {filePath}\n" +
                                      $"{exceptionDetails}";
            _ = LogErrors.LogErrorAsync(ex, catchContextMessage); // Use renamed variable

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }
    }

    /// <summary>
    /// Estimates the extracted size of a ZIP archive
    /// </summary>
    /// <param name="archivePath">The path to the ZIP archive to estimate</param>
    /// <returns>Estimated size in bytes</returns>
    private static long EstimateExtractedSize(string archivePath)
    {
        long totalSize;
        using (var zipFile = new ZipFile(archivePath))
        {
            totalSize = zipFile.Cast<ZipEntry>().Where(static entry => !entry.IsDirectory).Sum(entry => entry.Size);
        }

        // Add a safety margin of 20%
        return (long)(totalSize * 1.2);
    }

    /// <summary>
    /// Gets detailed exception information including inner exceptions
    /// </summary>
    /// <param name="ex">The exception to analyze</param>
    /// <returns>Detailed exception information</returns>
    private static string GetDetailedExceptionInfo(Exception ex)
    {
        var sb = new StringBuilder();

        var currentEx = ex;
        var level = 0;

        while (currentEx != null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"[Level {level}] {currentEx.GetType().Name}: {currentEx.Message}");
            level++;
            currentEx = currentEx.InnerException;
        }

        return sb.ToString();
    }

    private static string EscapeCommandLineArgument(string arg)
    {
        // Replace any embedded quotes with escaped quotes
        return arg.Replace("\"", "\\\"");
    }

    // Helper function to verify no path traversal in extracted files
    private static bool VerifyNoPathTraversalInExtractedFiles(string basePath, string currentPath)
    {
        // Get the full path of both directories
        var fullBasePath = PathHelper.ResolveRelativeToCurrentWorkingDirectory(basePath);
        var fullCurrentPath = PathHelper.ResolveRelativeToCurrentWorkingDirectory(currentPath);

        // First check if the current directory is within the base path
        if (!fullCurrentPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check each file in the current directory
        foreach (var file in Directory.GetFiles(currentPath))
        {
            var fullFilePath = PathHelper.ResolveRelativeToCurrentWorkingDirectory(file);
            if (!fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Recursively check all subdirectories
        foreach (var dir in Directory.GetDirectories(currentPath))
        {
            if (!VerifyNoPathTraversalInExtractedFiles(basePath, dir))
            {
                return false;
            }
        }

        return true;
    }
}