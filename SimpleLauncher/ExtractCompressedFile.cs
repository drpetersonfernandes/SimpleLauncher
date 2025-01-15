using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

namespace SimpleLauncher;

internal class ExtractCompressedFile
{
    
    static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

    private static readonly Lazy<ExtractCompressedFile> Instance = new(() => new ExtractCompressedFile());
    public static ExtractCompressedFile Instance2 => Instance.Value;
    private readonly List<string> _tempDirectories = new();
        
    // Use the Windows default temp directory
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        
    private ExtractCompressedFile() { } // Private constructor to enforce a singleton pattern

    public async Task<string> ExtractArchiveToTempAsync(string archivePath)
    {
        string extension = Path.GetExtension(archivePath)?.ToLower();

        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            MessageBox.Show($"The selected file '{archivePath}' cannot be extracted.\n\n" +
                            $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                            $"Please go to Edit System - Expert Mode, and edit this system.",
                "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
            
        // Choose the correct 7z executable path based on user environment (x64, x86 or arm64)
        string sevenZipPath = Get7ZipExecutablePath();
            
        // Open the Please Wait Window
        var pleaseWaitExtraction = new PleaseWaitExtraction();
        pleaseWaitExtraction.Show();

        // Combine temp folder with generated temp folders
        string tempDirectory = Path.Combine(_tempFolder, Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        
        if (string.IsNullOrEmpty(tempDirectory) || !Directory.Exists(tempDirectory))
        {
            // Notify developer
            string errorMessage = $"'Simple Launcher' could not create the temporary folder needed for extraction.\n\n" +
                                  $"Temp Location: {tempDirectory}\n" +
                                  $"Method: ExtractArchiveToTempAsync";
            Exception ex = new(errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            // Notify user
            MessageBox.Show("Extraction failed!\n\n" +
                            "'Simple Launcher' could not create the temporary folder needed for extraction.\n" +
                            "This error is happening because 'Simple Launcher' does not have enough privileges.\n" +
                            "Try running it with administrative privileges.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            return null;
        }

        // Keep track of the temp directory
        _tempDirectories.Add(tempDirectory);

        // Start the process to extract the archive
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
            // Track the start time
            var startTime = DateTime.Now;
                
            // Run the extraction process in a background thread
            string result = await Task.Run(() =>
            {
                using Process process = new();
                process.StartInfo = processStartInfo;
                process.Start();

                // Read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string errorMessage = $"Extraction of the compressed file failed.\n\n" +
                                          $"Method: ExtractArchiveToTempAsync\n" +
                                          $"Exit code: {process.ExitCode}\n" +
                                          $"Output: {output}\n" +
                                          $"Error: {error}";
                    throw new Exception(errorMessage);
                }

                return tempDirectory;
            });

            // Ensure at least 2 seconds have passed since the start
            var elapsedTime = DateTime.Now - startTime;
            if (elapsedTime.TotalMilliseconds < 2000)
            {
                await Task.Delay(2000 - (int)elapsedTime.TotalMilliseconds);
            }
                
            return result;
        }
        catch (Exception ex)
        {
            // Log the error
            string errorMessage = $"Extraction of the compressed file failed.\n\n" +
                                  $"The file {archivePath} may be corrupted.\n" +
                                  $"Method: ExtractArchiveToTempAsync";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            var result = MessageBox.Show($"Extraction of the compressed file failed!\n\n" +
                                         $"The file {archivePath} may be corrupted.\n" +
                                         $"Or maybe Simple Launcher does not have enough privileges to run in your system.\n" +
                                         $"Try to run with administrative privileges.\n\n" +
                                         $"Do you want to open the file 'error_user.log' to debug the error?",
                "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = LogPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    MessageBox.Show("The file 'error_user.log' was not found!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            return null;
        }
        finally
        {
            // Close the Please Wait Window
            pleaseWaitExtraction.Close();
        }
    }
        
    private string Get7ZipExecutablePath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return Path.Combine(baseDirectory, "7z.exe"); // Default for 64-bit
        }
        else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
        {
            return Path.Combine(baseDirectory, "7z_x86.exe");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported architecture for 7z extraction.");
        }
    }

    public void Cleanup()
    {
        foreach (var dir in _tempDirectories)
        {
            // Delete generated temp folders
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
        // Clear the list after deleting
        _tempDirectories.Clear();
    }
        
    public async Task<bool> ExtractDownloadFilesAsync(string filePath, string destinationFolder)
    {
        
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        //  // Test: Lock the file before trying to extract it
        //  FileStream? testLockStream = null;
        //  try
        //  {
        //      // Open the file and lock it for testing purposes
        //      testLockStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        //      testLockStream.Lock(0, new FileInfo(filePath).Length); // Lock the entire file
        //      Console.WriteLine(@"File is locked for testing purposes. Attempting extraction...");
        //  }
        //  catch (Exception ex)
        //  {
        //      Console.WriteLine($@"Failed to lock the file: {ex.Message}");
        //  }
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            string formattedException = $"The downloaded file appears to be empty or corrupted.\n\n" +
                                        $"Method: ExtractDownloadFilesAsync";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);
                
            MessageBox.Show("The downloaded file appears to be empty or corrupted.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
            
        if (IsFileLocked(filePath))
        {
            string formattedException = $"The downloaded file appears to be locked.\n" +
                                        $"Method: ExtractDownloadFilesAsync";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);
                
            MessageBox.Show("The downloaded file appears to be locked.",
                "Downloaded File is Locked", MessageBoxButton.OK, MessageBoxImage.Warning);

            return false;
        }
            
        try
        {
            // Get the correct 7z executable path based on user environment (x64 or x86)
            string sevenZipPath = Get7ZipExecutablePath();

            if (!File.Exists(sevenZipPath))
            {
                string formattedException = $"The required 7z executable was not found in the application folder.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                    
                // Ask the user if they want to automatically reinstall Simple Launcher
                var messageBoxResult = MessageBox.Show(
                    "The appropriate version of '7z.exe' was not found in the application folder!\n\n" +
                    "'Simple Launcher' will not be able to extract compressed files.\n\n" +
                    "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?",
                    "Extraction Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' to fix the problem.",
                        "Warning", MessageBoxButton.OK,MessageBoxImage.Warning);
                }
                    
                return false;
            }
                
            // Create destination folder if it does not exist
            Directory.CreateDirectory(destinationFolder);
            
            // Delay for 1 second
            // Give time to file unlock and also create destinationFolder
            await Task.Delay(1000);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"x \"{filePath}\" -o\"{destinationFolder}\" -y",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string formattedException = $"Error extracting the file: {filePath}\n\n" +
                                            $"Error message: {error}\n\n" +
                                            $"Method: ExtractDownloadFilesAsync";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);

                MessageBox.Show($"Error extracting the file: {filePath}\n\n" +
                                $"The file might be corrupted or locked by some other process.\n\n" +
                                $"Some antivirus programs may lock, block extraction or scan newly downloaded files, causing access issues.\n" +
                                $"Try to temporarily disable real-time protection.\n\n" +
                                $"You can also try to run 'Simple Launcher' with administrative privileges.",
                    "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            string formattedException = $"Error extracting the file: {filePath}\n\n" +
                                        $"Method: ExtractDownloadFilesAsync\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show($"Error extracting the file: {filePath}\n\n" +
                            $"The file might be corrupted or locked by some other process.\n\n" +
                            $"Some antivirus programs may lock, block extraction or scan newly downloaded files, causing access issues.\n" +
                            $"Try to temporarily disable real-time protection.\n\n" +
                            $"You can also try to run 'Simple Launcher' with administrative privileges.",
                "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return false;
        }
        
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        //  finally
        //  {
        //      // Unlock the file after testing
        //      testLockStream?.Unlock(0, new FileInfo(filePath).Length);
        //      testLockStream?.Dispose();
        //      Console.WriteLine(@"File lock released after testing.");
        //  }
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
        // ///////////////////////////////////////////////////
    }
        
    private bool IsFileLocked(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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
}