using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using SevenZip;

namespace SimpleLauncher.Services;

public class ExtractCompressedFile
{
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    /// <summary>
    /// Method to Extract 7z, Zip, and Rar files from GameLauncher.
    /// It extracts to a temp folder.
    /// Use the SevenZipExtractor library.
    /// </summary>
    /// <param name="archivePath">Full path to the archive file</param>
    /// <returns>Path to the extraction directory or null if extraction failed</returns>
    public async Task<string> ExtractWithSevenZipSharpToTempAsync(string archivePath)
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
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return null;
        }

        string tempDirectory = null;

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

            tempDirectory = Path.Combine(_tempFolder, randomName);
            Directory.CreateDirectory(tempDirectory);

            await Task.Run(() =>
            {
                using var extractor = new SevenZipExtractor(archivePath);

                // First, validate for path traversal before extracting
                var fullTempDir = Path.GetFullPath(tempDirectory);

                foreach (var entry in extractor.ArchiveFileData)
                {
                    if (entry.IsDirectory) continue;

                    var fullDestPath = Path.GetFullPath(Path.Combine(fullTempDir, entry.FileName));
                    if (!fullDestPath.StartsWith(fullTempDir, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new SecurityException($"Potential path traversal detected in archive entry: {entry.FileName}");
                    }
                }

                // If validation passes, extract the archive.
                // The ExtractArchive method is synchronous, so it's correctly wrapped in Task.Run.
                extractor.ExtractArchive(tempDirectory);
            });

            return tempDirectory;
        }
        catch (Exception ex)
        {
            CleanFolder.CleanupTempDirectory(tempDirectory);

            // Notify developer
            const string contextMessage = "Extraction of the compressed file failed. The file may be corrupted or a security issue was detected.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
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

                            // Notify user
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
            totalSize = zipFile.Cast<ZipEntry>().Where(static entry => !entry.IsDirectory).Sum(static entry => entry.Size);
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
            // Corrected the ambiguous invocation by using FormattableString.Invariant
            sb.AppendLine(FormattableString.Invariant($"[Level {level}] {currentEx.GetType().Name}: {currentEx.Message}"));
            level++;
            currentEx = currentEx.InnerException;
        }

        return sb.ToString();
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
