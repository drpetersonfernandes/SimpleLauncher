using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
    public async Task<string> ExtractGameToTempAsync(string archivePath)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
        {
            const string contextMessage = "Archive path cannot be null, empty, or file not found.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.ExtractionFailedMessageBox();
            return null;
        }

        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();
            return null;
        }

        var sevenZipPath = Get7ZipPath.Get7ZipExecutablePath();
        if (string.IsNullOrEmpty(sevenZipPath) || !File.Exists(sevenZipPath))
        {
            const string contextMessage = "7-Zip executable not found or is inaccessible.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.ExtractionFailedMessageBox();
            return null;
        }

        string tempDirectory = null;
        PleaseWaitWindow pleaseWaitExtraction = null;

        try
        {
            // Resolve the base temp folder path using PathHelper
            var resolvedTempFolder = PathHelper.ResolveRelativeToAppDirectory(_tempFolder);

            // Validate that the resolved temp folder is within the system's temp path
            var systemTempPath = Path.GetTempPath(); // Use standard GetTempPath for comparison

            if (string.IsNullOrEmpty(resolvedTempFolder) || !resolvedTempFolder.StartsWith(PathHelper.ResolveRelativeToAppDirectory(systemTempPath), StringComparison.OrdinalIgnoreCase)) // Resolve system temp path too
            {
                // The _tempFolder has been manipulated or resolution failed - use default temp path instead
                resolvedTempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

                const string contextMessage = "Potential path manipulation detected or temp folder resolution failed. Reverting to default temp path.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(archivePath);
            }

            var randomName = Path.GetRandomFileName();
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                randomName = Guid.NewGuid().ToString("N");
            }

            // Combine the resolved temp folder with the random name
            tempDirectory = Path.Combine(resolvedTempFolder, randomName);
            try
            {
                Directory.CreateDirectory(tempDirectory);
            }
            catch (Exception ex)
            {
                const string contextMessage = "Could not create the temp directory.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
            const string catchContextMessage = $"Extraction of the compressed file failed.\n" + // Renamed
                                          $"The file may be corrupted.";
            _ = LogErrors.LogErrorAsync(ex, catchContextMessage); // Use renamed variable
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
    /// Use native .net library to extract files
    /// It extracts to temp folder
    /// </summary>
    /// <param name="archivePath">Full path to the zip file</param>
    /// <returns>Path to the extraction directory or null if extraction failed</returns>
    public async Task<string> ExtractGameToTempAsync2(string archivePath)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
        {
            const string contextMessage = "Archive path cannot be null, empty, or file not found.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
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
        var extractionSuccessful = false; // Declared outside try

        try
        {
            // Resolve the base temp folder path using PathHelper
            var resolvedTempFolder = PathHelper.ResolveRelativeToAppDirectory(_tempFolder);

            // Validate that the resolved temp folder is within the system's temp path
            var systemTempPath = Path.GetTempPath(); // Use standard GetTempPath for comparison

            if (string.IsNullOrEmpty(resolvedTempFolder) || !resolvedTempFolder.StartsWith(PathHelper.ResolveRelativeToAppDirectory(systemTempPath), StringComparison.OrdinalIgnoreCase)) // Resolve system temp path too
            {
                // The _tempFolder has been manipulated or resolution failed - use default temp path instead
                resolvedTempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

                const string contextMessage = "Potential path manipulation detected or temp folder resolution failed. Reverting to default temp path.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(archivePath);
            }

            var randomName = Path.GetRandomFileName();
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                randomName = Guid.NewGuid().ToString("N");
            }

            // Combine the resolved temp folder with the random name
            tempDirectory = Path.Combine(resolvedTempFolder, randomName);
            try
            {
                Directory.CreateDirectory(tempDirectory);
            }
            catch (Exception ex)
            {
                const string contextMessage = "Could not create the temp directory.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
                    using var fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

                    if (archive.Entries.Count == 0)
                    {
                        throw new InvalidDataException("The ZIP file contains no entries.");
                    }

                    var estimatedSize = EstimateExtractedSize(archive);
                    var rootPath = Path.GetPathRoot(Path.GetFullPath(tempDirectory)); // Use tempDirectory for drive check

                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        try
                        {
                            var drive = new DriveInfo(rootPath);
                            if (drive.IsReady && drive.AvailableFreeSpace < estimatedSize)
                            {
                                var contextMessage =
                                    $"Not enough disk space for extraction. Required: {estimatedSize / (1024 * 1024)} MB, Available: {drive.AvailableFreeSpace / (1024 * 1024)} MB";
                                Exception ex = new IOException(contextMessage);
                                _ = LogErrors.LogErrorAsync(ex, "Not enough disk space for extraction.");
                                MessageBoxLibrary.DiskSpaceErrorMessageBox();
                                return; // Exit the Task.Run lambda
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            _ = LogErrors.LogErrorAsync(ex,
                                $"Unable to check disk space for path {tempDirectory}: {ex.Message}");
                            MessageBoxLibrary.CouldNotCheckForDiskSpaceMessageBox();
                            return; // Exit the Task.Run lambda
                        }
                    }

                    foreach (var entry in archive.Entries)
                    {
                        var entryDestinationPath = Path.Combine(tempDirectory, entry.FullName);

                        // Verify the destination path is within the intended temp directory
                        var fullDestPath = PathHelper.ResolveRelativeToCurrentWorkingDirectory(entryDestinationPath);
                        var fullTempDir = PathHelper.ResolveRelativeToCurrentWorkingDirectory(tempDirectory);

                        if (!fullDestPath.StartsWith(fullTempDir, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(archivePath);
                            throw new SecurityException($"Potentially dangerous zip entry path: {entry.FullName}");
                        }

                        var entryDirectoryPath = Path.GetDirectoryName(entryDestinationPath);
                        if (!string.IsNullOrEmpty(entryDirectoryPath) && !Directory.Exists(entryDirectoryPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(entryDirectoryPath);
                            }
                            catch (Exception ex)
                            {
                                _ = LogErrors.LogErrorAsync(ex, $"Failed to create directory: {entryDirectoryPath}");
                            }
                        }

                        if (string.IsNullOrEmpty(entry.Name)) continue;

                        entry.ExtractToFile(entryDestinationPath, true);
                    }
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
            var catchContextMessage = $"Extraction of the compressed file failed.\n" + // Renamed
                                     $"The file may be corrupted.\n" +
                                     $"File: {archivePath}";
            _ = LogErrors.LogErrorAsync(ex, catchContextMessage); // Use renamed variable
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
                    var contextMessage = $"Failed to clean up partial extraction in: {tempDirectory}"; // Use tempDirectory
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
    public static async Task<bool> ExtractDownloadFilesAsync(string filePath, string destinationFolder)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            const string contextMessage = "File path is invalid.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.DownloadedFileIsMissingMessageBox();
            return false;
        }

        // Resolve the destination folder using PathHelper
        var resolvedDestinationFolder = PathHelper.ResolveRelativeToAppDirectory(destinationFolder);

        if (string.IsNullOrEmpty(resolvedDestinationFolder)) // Check if resolution failed
        {
            const string contextMessage = "Destination folder path resolution failed.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.ExtractionFailedMessageBox();
            return false;
        }

        if (CheckForFileLock.IsFileLocked(filePath))
        {
            var contextMessage = $"The downloaded file appears to be locked: {filePath}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.FileIsLockedMessageBox();
            return false;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".zip")
        {
            var contextMessage = $"Only ZIP files are supported by this extraction method.\n" +
                                 $"File type: {extension}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();
            return false;
        }

        var extractionSuccessful = false; // Declared outside try

        try
        {
            try
            {
                // Use the resolved destination folder for creation
                Directory.CreateDirectory(resolvedDestinationFolder);
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, $"Failed to create directory: {resolvedDestinationFolder}");
            }

            // Create a tracking file in the resolved destination folder
            var extractionTrackingFile = Path.Combine(resolvedDestinationFolder, ".extraction_in_progress");
            await File.WriteAllTextAsync(extractionTrackingFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            await Task.Run(() =>
            {
                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

                    if (archive.Entries.Count == 0)
                    {
                        throw new InvalidDataException("The ZIP file contains no entries.");
                    }

                    var estimatedSize = EstimateExtractedSize(archive);

                    // Check disk space using the resolved destination folder
                    var rootPath = Path.GetPathRoot(resolvedDestinationFolder);
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        try
                        {
                            var drive = new DriveInfo(rootPath);
                            if (drive.IsReady && drive.AvailableFreeSpace < estimatedSize)
                            {
                                var contextMessage =
                                    $"Not enough disk space for extraction. Required: {estimatedSize / (1024 * 1024)} MB, Available: {drive.AvailableFreeSpace / (1024 * 1024)} MB";
                                Exception ex = new IOException(contextMessage);
                                _ = LogErrors.LogErrorAsync(ex, "Not enough disk space for extraction.");
                                MessageBoxLibrary.DiskSpaceErrorMessageBox();
                                return; // Exit the Task.Run lambda
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            _ = LogErrors.LogErrorAsync(ex,
                                $"Unable to check disk space for path {resolvedDestinationFolder}: {ex.Message}");
                            MessageBoxLibrary.CouldNotCheckForDiskSpaceMessageBox();
                            return; // Exit the Task.Run lambda
                        }
                    }

                    foreach (var entry in archive.Entries)
                    {
                        // Combine with the resolved destination folder
                        var entryDestinationPath = Path.Combine(resolvedDestinationFolder, entry.FullName);

                        // Verify the destination path is within the resolved destination folder
                        var fullDestPath = PathHelper.ResolveRelativeToAppDirectory(entryDestinationPath);
                        var fullResolvedDestFolder = PathHelper.ResolveRelativeToAppDirectory(resolvedDestinationFolder);

                        if (!fullDestPath.StartsWith(fullResolvedDestFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(filePath); // Use original filePath for message
                            throw new SecurityException($"Potentially dangerous zip entry path: {entry.FullName}");
                        }

                        var entryDirectoryPath = Path.GetDirectoryName(entryDestinationPath);
                        if (!string.IsNullOrEmpty(entryDirectoryPath) && !Directory.Exists(entryDirectoryPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(entryDirectoryPath);
                            }
                            catch (Exception ex)
                            {
                                _ = LogErrors.LogErrorAsync(ex, $"Failed to create directory: {entryDirectoryPath}");
                            }
                        }

                        if (string.IsNullOrEmpty(entry.Name)) continue;

                        entry.ExtractToFile(entryDestinationPath, true);
                    }
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

            extractionSuccessful = true; // Set to true on successful extraction
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
                        CleanFolder.CleanupPartialExtraction(resolvedDestinationFolder); // Use resolvedDestinationFolder
                    }
                }
                catch (Exception cleanupEx)
                {
                    var contextMessage = $"Failed to clean up partial extraction in: {resolvedDestinationFolder}"; // Use resolvedDestinationFolder
                    _ = LogErrors.LogErrorAsync(cleanupEx, contextMessage);
                }
            }

            var exceptionDetails = GetDetailedExceptionInfo(ex);
            var catchContextMessage = $"Error extracting the file: {filePath}\n" + // Renamed
                                     $"{exceptionDetails}";
            _ = LogErrors.LogErrorAsync(ex, catchContextMessage); // Use renamed variable
            MessageBoxLibrary.ExtractionFailedMessageBox();
            return false;
        }
        finally
        {
            // No pleaseWaitExtraction window in this method, so no cleanup needed here.
            // The cleanup logic for partial extraction is handled in the catch block above.
        }
    }


    /// <summary>
    /// Estimates the extracted size of a ZIP archive
    /// </summary>
    /// <param name="archive">The ZIP archive to estimate</param>
    /// <returns>Estimated size in bytes</returns>
    private static long EstimateExtractedSize(ZipArchive archive)
    {
        long totalSize = 0;
        foreach (var entry in archive.Entries)
        {
            totalSize += entry.Length;
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
