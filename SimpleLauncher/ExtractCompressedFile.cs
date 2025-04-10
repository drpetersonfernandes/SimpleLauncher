using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

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
        // Existing validation code remains the same
        if (string.IsNullOrEmpty(archivePath))
        {
            // Notify developer
            const string contextMessage = "Archive path cannot be null or empty";
            var ex = new ArgumentNullException(nameof(archivePath));
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        // Check if the file exists
        if (!File.Exists(archivePath))
        {
            // Notify developer
            var contextMessage = $"Archive file not found: {archivePath}";
            var ex = new FileNotFoundException("Archive file not found", archivePath);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        // Check file extension
        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return null;
        }

        // Choose the correct 7z executable
        var sevenZipPath = Get7ZipExecutablePath();
        if (string.IsNullOrEmpty(sevenZipPath) || !File.Exists(sevenZipPath))
        {
            // Notify developer
            const string contextMessage = "7-Zip executable not found or is inaccessible.";
            var ex = new FileNotFoundException(contextMessage, sevenZipPath);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        // Create temp folder with random name - IMPROVED IMPLEMENTATION
        string tempDirectory = null;
        PleaseWaitExtractionWindow pleaseWaitExtraction = null;

        try
        {
            // Ensure _tempFolder is safe and doesn't contain path traversal attempts
            var safeTempFolder = Path.GetFullPath(_tempFolder);
        
            // Validate that _tempFolder is still within the system's temp path
            var systemTempPath = Path.GetFullPath(Path.GetTempPath());
            if (!safeTempFolder.StartsWith(systemTempPath, StringComparison.OrdinalIgnoreCase))
            {
                // The _tempFolder has been manipulated - use default temp path instead
                safeTempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
            
                // Log this as a potential security issue
                var contextMessage = $"Potential path manipulation detected. Reverting to default temp path.";
                var ex = new SecurityException(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        
            // Create a random directory name
            var randomName = Path.GetRandomFileName();
        
            // Ensure the random name doesn't contain path traversal characters
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                // Create a safer random name
                randomName = Guid.NewGuid().ToString("N");
            }
        
            tempDirectory = Path.Combine(safeTempFolder, randomName);
            Directory.CreateDirectory(tempDirectory);

            // Show the wait window (on UI thread)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                pleaseWaitExtraction = new PleaseWaitExtractionWindow();
                pleaseWaitExtraction.Show();
            });

            // Construct the process with improved argument sanitization
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                // Proper quoting of paths to avoid command injection
                Arguments = $"x \"{EscapeCommandLineArgument(archivePath)}\" -o\"{EscapeCommandLineArgument(tempDirectory)}\" -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.EnableRaisingEvents = true;

            // Rest of the method remains the same
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    outputBuilder.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    errorBuilder.AppendLine(args.Data);
            };

            // Run the extraction process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit
            await process.WaitForExitAsync();

            // Check if extraction was successful
            if (process.ExitCode == 0)
            {
                // Add additional security check - scan the extracted files for zip slip attempts
                if (!VerifyNoPathTraversalInExtractedFiles(tempDirectory, tempDirectory))
                {
                    throw new SecurityException("Potential path traversal detected in archive contents");
                }
            
                return tempDirectory;
            }

            // If we get here, extraction failed
            var errorMessage = $"Extraction of the compressed file failed.\n\n" +
                               $"Exit code: {process.ExitCode}\n" +
                               $"Output: {outputBuilder}\n" +
                               $"Error: {errorBuilder}";

            throw new Exception(errorMessage);
        }
        catch (Exception ex)
        {
            // Clean up temp directory on failure
            CleanupTempDirectory(tempDirectory);

            // Notify developer
            const string contextMessage = $"Extraction of the compressed file failed.\n" +
                                          $"The file may be corrupted.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }
        finally
        {
            // Close the wait window on UI thread
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
        // Existing validation code remains the same
        if (string.IsNullOrEmpty(archivePath))
        {
            // Notify developer
            const string contextMessage = "Archive path cannot be null or empty";
            var ex = new ArgumentNullException(nameof(archivePath));
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        // Check if the file exists
        if (!File.Exists(archivePath))
        {
            // Notify developer
            var contextMessage = $"The specified archive file does not exist: {archivePath}";
            var ex = new FileNotFoundException("Archive file not found", archivePath);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        // Check file extension
        var extension = Path.GetExtension(archivePath).ToLowerInvariant();
        if (extension != ".zip")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return null;
        }

        // Create temp folder with random name - IMPROVED IMPLEMENTATION
        string tempDirectory = null;
        PleaseWaitExtractionWindow pleaseWaitExtraction = null;

        try
        {
            // Ensure _tempFolder is safe and doesn't contain path traversal attempts
            var safeTempFolder = Path.GetFullPath(_tempFolder);
        
            // Validate that _tempFolder is still within the system's temp path
            var systemTempPath = Path.GetFullPath(Path.GetTempPath());
            if (!safeTempFolder.StartsWith(systemTempPath, StringComparison.OrdinalIgnoreCase))
            {
                // The _tempFolder has been manipulated - use default temp path instead
                safeTempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
            
                // Log this as a potential security issue
                var contextMessage = $"Potential path manipulation detected. Reverting to default temp path.";
                var ex = new SecurityException(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        
            // Create a random directory name
            var randomName = Path.GetRandomFileName();
        
            // Ensure the random name doesn't contain path traversal characters
            if (randomName.Contains("..") || randomName.Contains('/') || randomName.Contains('\\'))
            {
                // Create a safer random name
                randomName = Guid.NewGuid().ToString("N");
            }
        
            tempDirectory = Path.Combine(safeTempFolder, randomName);
            Directory.CreateDirectory(tempDirectory);

            // Show the wait window (on UI thread)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                pleaseWaitExtraction = new PleaseWaitExtractionWindow();
                pleaseWaitExtraction.Show();
            });

            // Extract the zip file using built-in .NET method with improved security
            await Task.Run(() =>
            {
                try
                {
                    // Instead of using ZipFile.ExtractToDirectory directly, which doesn't
                    // protect against zip slip, extract files individually with validation
                    using var archive = ZipFile.OpenRead(archivePath);
                
                    // Check for empty archive
                    if (archive.Entries.Count == 0)
                    {
                        throw new InvalidDataException("The ZIP file contains no entries.");
                    }
                
                    // Extract files with path traversal protection
                    foreach (var entry in archive.Entries)
                    {
                        var entryDestinationPath = Path.Combine(tempDirectory, entry.FullName);
                        var fullDestPath = Path.GetFullPath(entryDestinationPath);
                        var fullTempDir = Path.GetFullPath(tempDirectory);
                    
                        // Prevent zip slip by validating the extraction path
                        if (!fullDestPath.StartsWith(fullTempDir, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new SecurityException($"Potentially dangerous zip entry path: {entry.FullName}");
                        }
                    
                        // Create directory for the entry if needed
                        var entryDirectoryPath = Path.GetDirectoryName(entryDestinationPath);
                        if (!string.IsNullOrEmpty(entryDirectoryPath) && !Directory.Exists(entryDirectoryPath))
                        {
                            Directory.CreateDirectory(entryDirectoryPath);
                        }
                    
                        // Skip directories (folders are already created above)
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }
                    
                        // Extract the file
                        entry.ExtractToFile(entryDestinationPath, true);
                    }
                }
                catch (Exception ex)
                {
                    // Wrap and rethrow to preserve stack trace
                    throw new InvalidDataException($"Failed to extract zip contents: {ex.Message}", ex);
                }
            });

            return tempDirectory;
        }
        catch (Exception ex)
        {
            // Clean up temp directory on failure
            CleanupTempDirectory(tempDirectory);

            // Notify developer
            var contextMessage = $"Extraction of the compressed file failed.\n" +
                                 $"The file may be corrupted.\n" +
                                 $"File: {archivePath}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }
        finally
        {
            // Close the wait window on UI thread
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
        // Parameter validation
        if (string.IsNullOrEmpty(filePath))
        {
            // Notify developer
            const string contextMessage = "File path cannot be null or empty";
            var ex = new ArgumentNullException(nameof(filePath));
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.DownloadedFileIsMissingMessageBox();

            return false;
        }

        if (string.IsNullOrEmpty(destinationFolder))
        {
            // Notify developer
            const string contextMessage = "Destination folder cannot be null or empty";
            var ex = new ArgumentNullException(nameof(destinationFolder));
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }

        // Check if the downloaded file exists and has content
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            // Notify developer
            var contextMessage = $"The filepath is invalid or file is empty.\n" +
                                 $"Filepath: {filePath}";
            var ex = new FileNotFoundException(contextMessage, filePath);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.DownloadedFileIsMissingMessageBox();

            return false;
        }

        // Validate destination path safety
        try
        {
            // Convert to the full path and check for potential path traversal issues
            var fullDestPath = Path.GetFullPath(destinationFolder);
            var appBasePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);

            if (!fullDestPath.StartsWith(appBasePath, StringComparison.OrdinalIgnoreCase))
            {
                // Notify developer
                var contextMessage = $"Destination folder must be within the application directory.\n" +
                                     $"Requested path: {fullDestPath}";
                var ex = new UnauthorizedAccessException(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return false;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Invalid destination path: {destinationFolder}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }

        // Check if the file is locked
        if (IsFileLocked(filePath))
        {
            // Notify developer
            var contextMessage = $"The downloaded file appears to be locked: {filePath}";
            var ex = new IOException(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FileIsLockedMessageBox();

            return false;
        }

        // Check file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Verify this is a supported format
        if (extension != ".zip")
        {
            // Notify developer
            var contextMessage = $"Only ZIP files are supported by this extraction method.\n" +
                                 $"File type: {extension}";
            var ex = new NotSupportedException(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return false;
        }

        var extractionSuccessful = false;

        try
        {
            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationFolder);

            // Create a tracking directory to detect partial extraction
            var extractionTrackingFile = Path.Combine(destinationFolder, ".extraction_in_progress");
            await File.WriteAllTextAsync(extractionTrackingFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // Extract the file using native .NET libraries
            await Task.Run(() =>
            {
                try
                {
                    using var archive = ZipFile.OpenRead(filePath);

                    // Perform basic validation of zip file contents
                    if (archive.Entries.Count == 0)
                    {
                        throw new InvalidDataException("The ZIP file contains no entries.");
                    }

                    // Verify there's enough disk space for extraction
                    var estimatedSize = EstimateExtractedSize(archive);
                    var driveName = Path.GetPathRoot(destinationFolder);

                    // Validate drive name before creating DriveInfo object
                    if (!string.IsNullOrEmpty(driveName))
                    {
                        try
                        {
                            // Format the drive name properly if needed
                            if (driveName.Length == 1)
                            {
                                driveName = $"{driveName}:\\"; // Convert "C" to "C:\"
                            }
                            else if (driveName.Length == 2 && driveName[1] == ':')
                            {
                                driveName = $"{driveName}\\"; // Convert "C:" to "C:\"
                            }

                            // Network paths and other formats won't work with DriveInfo
                            // so only proceed
                            // if it looks like a valid Windows drive path
                            if (driveName.Length >= 3 && driveName[1] == ':' && driveName[2] == '\\')
                            {
                                var drive = new DriveInfo(driveName);

                                if (drive.IsReady && drive.AvailableFreeSpace < estimatedSize)
                                {
                                    throw new IOException($"Not enough disk space for extraction. Required: {estimatedSize / (1024 * 1024)} MB, Available: {drive.AvailableFreeSpace / (1024 * 1024)} MB");
                                }
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            // Log the issue but continue with extraction
                            // We won't block extraction
                            // just because we can't verify disk space
                            Debug.WriteLine($"Unable to check disk space for path {destinationFolder}: {ex.Message}");
                        }
                    }

                    // Extract files one by one
                    foreach (var entry in archive.Entries)
                    {
                        var entryDestinationPath = Path.Combine(destinationFolder, entry.FullName);

                        // Make sure the destination path is still within the target directory
                        // (protection against zip slip vulnerability)
                        if (!Path.GetFullPath(entryDestinationPath).StartsWith(Path.GetFullPath(destinationFolder), StringComparison.OrdinalIgnoreCase))
                        {
                            throw new SecurityException($"Potentially dangerous zip entry path: {entry.FullName}");
                        }

                        // Create directory for the entry if needed
                        var entryDirectoryPath = Path.GetDirectoryName(entryDestinationPath);
                        if (!string.IsNullOrEmpty(entryDirectoryPath) && !Directory.Exists(entryDirectoryPath))
                        {
                            Directory.CreateDirectory(entryDirectoryPath);
                        }

                        // Skip directories (folders are already created above)
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }

                        // Extract the file (always overwrite existing files)
                        entry.ExtractToFile(entryDestinationPath, true);
                    }
                }
                catch (Exception)
                {
                    // Re-throw to be caught by the outer try/catch
                    throw;
                }
            });

            // Remove the tracking file on successful extraction
            if (File.Exists(extractionTrackingFile))
            {
                File.Delete(extractionTrackingFile);
            }

            extractionSuccessful = true;
            return true;
        }
        catch (Exception ex)
        {
            // Get more specific exception details
            var exceptionDetails = GetDetailedExceptionInfo(ex);

            // Notify developer
            var contextMessage = $"Error extracting the file: {filePath}\n" +
                                 $"{exceptionDetails}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }
        finally
        {
            // If extraction failed, clean up partial files
            if (!extractionSuccessful)
            {
                try
                {
                    // Check if the tracking file exists
                    var extractionTrackingFile = Path.Combine(destinationFolder, ".extraction_in_progress");
                    if (File.Exists(extractionTrackingFile))
                    {
                        // Remove all content from this folder as it's a partial extraction
                        CleanupPartialExtraction(destinationFolder);
                    }
                }
                catch (Exception cleanupEx)
                {
                    // Notify developer
                    var contextMessage = $"Failed to clean up partial extraction in: {destinationFolder}";
                    _ = LogErrors.LogErrorAsync(cleanupEx, contextMessage);
                }
            }
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

    // Check if the file is locked by antivirus software
    private static bool IsFileLocked(string filePath)
    {
        if (!File.Exists(filePath))
            return false;
        try
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }

    // Determine the 7z executable based on user environment
    private static string Get7ZipExecutablePath()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                return Path.Combine(baseDirectory, "7z.exe");
            case Architecture.X86:
                return Path.Combine(baseDirectory, "7z_x86.exe");
            default:
                throw new PlatformNotSupportedException("Unsupported architecture for 7z extraction.");
        }
    }

    /// <summary>
    /// Safely cleans up a temporary directory and its contents
    /// </summary>
    /// <param name="directoryPath">Path to the directory to clean up</param>
    private static void CleanupTempDirectory(string directoryPath)
    {
        if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - this is cleanup code
                // Notify developer
                var contextMessage = $"Failed to clean up temporary directory: {directoryPath}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }
    }

    /// <summary>
    /// Cleans up partially extracted files from a failed extraction
    /// </summary>
    /// <param name="directoryPath">Directory containing partial extraction</param>
    private static void CleanupPartialExtraction(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            return;
        }

        try
        {
            // Delete the tracking file first
            var trackingFile = Path.Combine(directoryPath, ".extraction_in_progress");
            if (File.Exists(trackingFile))
            {
                File.Delete(trackingFile);
            }

            // Delete all files in the directory
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                File.Delete(file);
            }

            // Recursively delete subdirectories
            foreach (var subDir in Directory.GetDirectories(directoryPath))
            {
                Directory.Delete(subDir, true);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - this is cleanup code
            // Notify developer
            var contextMessage = $"Error cleaning up partial extraction: {directoryPath}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }
    
    private string EscapeCommandLineArgument(string arg)
    {
        // Replace any embedded quotes with escaped quotes
        return arg.Replace("\"", "\\\"");
    }
    
    // Helper function to verify no path traversal in extracted files
    private bool VerifyNoPathTraversalInExtractedFiles(string basePath, string currentPath)
    {
        // Get the full path of both directories
        var fullBasePath = Path.GetFullPath(basePath);
        var fullCurrentPath = Path.GetFullPath(currentPath);
    
        // First check if the current directory is within the base path
        if (!fullCurrentPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
    
        // Check each file in the current directory
        foreach (var file in Directory.GetFiles(currentPath))
        {
            var fullFilePath = Path.GetFullPath(file);
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