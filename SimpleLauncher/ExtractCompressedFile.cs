using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace SimpleLauncher;

public class ExtractCompressedFile
{
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    // Method to Extract 7z and Rar files from GameLauncher
    // It extracts to temp folder
    // Use 7z executable
    public async Task<string> ExtractGameToTempAsync(string archivePath)
    {
        // Check file extension
        var extension = Path.GetExtension(archivePath)?.ToLower();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return null;
        }

        // Choose the correct 7z executable
        var sevenZipPath = Get7ZipExecutablePath();

        var pleaseWaitExtraction = new PleaseWaitExtraction();
        pleaseWaitExtraction.Show();

        // Create temp folders
        var tempDirectory = Path.Combine(_tempFolder, Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDirectory);
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"'Simple Launcher' could not create the temporary folder needed for extraction.\n\n" +
                               $"Temp Location: {tempDirectory}\n";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        // Construct the call
        ProcessStartInfo processStartInfo = new()
        {
            FileName = sevenZipPath,
            Arguments = $"x \"{archivePath}\" -o\"{tempDirectory}\" -y",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            // Run on a background task
            var result = await Task.Run(() =>
            {
                using Process process = new();
                process.StartInfo = processStartInfo;
                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0) return tempDirectory;

                // Notify developer
                var errorMessage = $"Extraction of the compressed file failed.\n\n" +
                                   $"Exit code: {process.ExitCode}\n" +
                                   $"Output: {output}\n" +
                                   $"Error: {error}";
                throw new Exception(errorMessage);
            });
            return result;
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"Extraction of the compressed file failed.\n\n" +
                               $"The file may be corrupted.\n";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }
        finally
        {
            pleaseWaitExtraction.Close();
        }
    }

    // Use to extract Zip files from GameLauncher
    // Use native .net library to extract files
    // Do not use third party application (7z.exe)
    // It extracts to temp folder
    public async Task<string> ExtractGameToTempAsync2(string archivePath)
    {
        // Check file extension
        var extension = Path.GetExtension(archivePath)?.ToLower();
        if (extension != ".zip")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return null;
        }

        var pleaseWaitExtraction = new PleaseWaitExtraction();
        pleaseWaitExtraction.Show();

        // Create temp folders
        var tempDirectory = Path.Combine(_tempFolder, Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDirectory);
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"'Simple Launcher' could not create the temporary folder needed for extraction.\n\n" +
                               $"Temp folder: {tempDirectory}\n";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        try
        {
            // Run on a background task
            await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, tempDirectory));

            return tempDirectory;
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"Extraction of the compressed file failed.\n" +
                               $"The file may be corrupted.\n" +
                               $"File: {archivePath}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return null;
        }

        finally
        {
            pleaseWaitExtraction.Close();
        }
    }

    // Used to extract downloaded files
    // More prone to errors because it extracts files inside 'Simple Launcher' folder
    // User needs to be an admin
    // 'Simple Launcher' folder needs to be writable
    public async Task<bool> ExtractDownloadFilesAsync2(string filePath, string destinationFolder)
    {
        // Check if the downloaded file exists
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            // Notify developer
            var formattedException = $"The filepath is invalid.\n" +
                                     $"Filepath: {filePath}";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);

            // Notify user
            MessageBoxLibrary.DownloadedFileIsMissingMessageBox();

            return false;
        }

        // Check if the file is locked
        // Can be locked by antivirus software
        if (IsFileLocked(filePath))
        {
            // Notify developer
            var formattedException = "The downloaded file appears to be locked.";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);

            // Notify user
            MessageBoxLibrary.FileIsLockedMessageBox();

            return false;
        }

        // Check file extension
        // File needs to be a compressed file to be extracted
        var extension = Path.GetExtension(filePath).ToLower();
        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            MessageBoxLibrary.FileNeedToBeCompressedMessageBox();

            return false;
        }

        // Create folders inside 'Simple Launcher' folder
        // Prone to errors due to access issues
        try
        {
            Directory.CreateDirectory(destinationFolder);
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"'Simple Launcher' could not create the destination folder.\n\n" +
                               $"Destination folder: {destinationFolder}\n";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }

        // Extract the download file using native .net library
        try
        {
            // Run in a background task
            await Task.Run(() => ZipFile.ExtractToDirectory(filePath, destinationFolder, true));

            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"Error extracting the file: {filePath}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.ExtractionFailedMessageBox();

            return false;
        }
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
}