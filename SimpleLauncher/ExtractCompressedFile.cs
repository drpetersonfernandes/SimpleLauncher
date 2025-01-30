using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace SimpleLauncher;

internal class ExtractCompressedFile
{
    
    private static readonly Lazy<ExtractCompressedFile> Instance = new(() => new ExtractCompressedFile());
    public static ExtractCompressedFile Instance2 => Instance.Value;

    private readonly List<string> _tempDirectories = new();
    
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private ExtractCompressedFile() { } // Private constructor to enforce a singleton pattern

    public async Task<string> ExtractArchiveToTempAsync(string archivePath)
    {
        string extension = Path.GetExtension(archivePath)?.ToLower();

        if (extension != ".7z" && extension != ".zip" && extension != ".rar")
        {
            // Notify user
            FileNeedToBeCompressedMessageBox();
            
            return null;
        }
            
        // Choose the correct 7z executable path based on user environment (x64 or x86)
        string sevenZipPath = Get7ZipExecutablePath();
            
        // Show the Please Wait Window
        var pleaseWaitExtraction = new PleaseWaitExtraction();
        pleaseWaitExtraction.Show();

        // Create temp folders
        string tempDirectory = Path.Combine(_tempFolder, Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDirectory);
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"'Simple Launcher' could not create the temporary folder needed for extraction.\n\n" +
                                  $"Temp Location: {tempDirectory}\n" +
                                  $"Method: ExtractArchiveToTempAsync";
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            // Notify user
            ExtractionFailedMessageBox();
        }
        
        if (string.IsNullOrEmpty(tempDirectory) || !Directory.Exists(tempDirectory))
        {
            // Notify developer
            string errorMessage = $"'Simple Launcher' could not create the temporary folder needed for extraction.\n\n" +
                                  $"Temp Location: {tempDirectory}\n" +
                                  $"Method: ExtractArchiveToTempAsync";
            Exception ex = new(errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            // Notify user
            ExtractionFailedMessageBox();

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
                    // Notify developer
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
            // Notify developer
            string errorMessage = $"Extraction of the compressed file failed.\n\n" +
                                  $"The file '{archivePath}' may be corrupted.\n" +
                                  $"Method: ExtractArchiveToTempAsync";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            ExtractionFailedMessageBox();

            return null;
        }
        finally
        {
            pleaseWaitExtraction.Close();
        }
    }

    public async Task<string> ExtractArchiveToTempAsync2(string archivePath)
    {
        string extension = Path.GetExtension(archivePath)?.ToLower();

        if (extension != ".zip")
        {
            // Notify user
            FileNeedToBeCompressedMessageBox();

            return null;
        }

        // Show Please Wait Window
        var pleaseWaitExtraction = new PleaseWaitExtraction();
        pleaseWaitExtraction.Show();

        // Create temp folders
        string tempDirectory = Path.Combine(_tempFolder, Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDirectory);

            if (!Directory.Exists(tempDirectory))
            {
                // Notify developer
                string errorMessage = $"'Simple Launcher' could not create the temporary folder needed for extraction.\n\n" +
                                      $"Temp Location: {tempDirectory}\n" +
                                      "Method: ExtractArchiveToTempAsync2";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                ExtractionFailedMessageBox();

                return null;
            }

            // Keep track of the temp directory
            _tempDirectories.Add(tempDirectory);
        
            // Perform the extraction
            await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, tempDirectory));

            // Ensure at least 2 seconds have passed
            await Task.Delay(2000);

            return tempDirectory;
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Extraction of the compressed file failed.\n\n" +
                                  $"The file {archivePath} may be corrupted.\n" +
                                  "Method: ExtractArchiveToTempAsync";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            ExtractionFailedMessageBox();

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
            return Path.Combine(baseDirectory, "7z.exe");
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
        {
            return Path.Combine(baseDirectory, "7z_x86.exe");
        }

        throw new PlatformNotSupportedException("Unsupported architecture for 7z extraction.");
    }

    public void CleanupTempFolders()
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
            // Notify developer
            string formattedException = $"The downloaded file appears to be empty or corrupted.\n\n" +
                                        $"File: {filePath}\n" +
                                        $"Method: ExtractDownloadFilesAsync";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);
            
            // Notify user
            DownloadErrorMessageBox();
            
            return false;
        }
            
        if (IsFileLocked(filePath))
        {
            // Notify developer
            string formattedException = $"The downloaded file appears to be locked.\n" +
                                        $"File: {filePath}\n" +
                                        $"Method: ExtractDownloadFilesAsync";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);

            // Notify user
            DownloadErrorMessageBox();

            return false;
        }
            
        try
        {
            // Get the correct 7z executable path based on user environment (x64 or x86)
            string sevenZipPath = Get7ZipExecutablePath();

            if (!File.Exists(sevenZipPath))
            {
                // Notify developer
                string formattedException = "The required 7z executable was not found in the application folder.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                    
                // Notify user
                DecompressorAppIsNotAvailableMessageBox();

                return false;
            }
                
            // Create destination folder if it does not exist
            try
            {
                Directory.CreateDirectory(destinationFolder);
            }
            catch (Exception ex)
            {
                // Notify developer
                string formattedException = "'Simple Launcher' could not create the destination folder.\n" +
                                            "Method: ExtractDownloadFilesAsync";
                await LogErrors.LogErrorAsync(ex, formattedException);
 
                // Notify user
                ExtractionFailedMessageBox();
            }
            
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
                // Notify developer
                string formattedException = $"Error extracting the file: {filePath}\n\n" +
                                            $"Error message: {error}\n\n" +
                                            $"Method: ExtractDownloadFilesAsync";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);

                // Notify user
                ExtractionFailedMessageBox();

                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error extracting the file: {filePath}\n\n" +
                                        $"Method: ExtractDownloadFilesAsync\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            ExtractionFailedMessageBox();

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

    public async Task<bool> ExtractDownloadFilesAsync2(string filePath, string destinationFolder)
    {
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            // Notify developer
            string formattedException = $"The downloaded file appears to be empty or corrupted.\n\n" +
                                        $"Method: ExtractDownloadFilesAsync2";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);

            // Notify user
            DownloadErrorMessageBox();

            return false;
        }

        if (IsFileLocked(filePath))
        {
            // Notify developer
            string formattedException = $"The downloaded file appears to be locked.\n" +
                                        $"Method: ExtractDownloadFilesAsync2";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);

            // Notify user
            DownloadErrorMessageBox();

            return false;
        }

        try
        {
            Directory.CreateDirectory(destinationFolder);

            await Task.Run(() => ZipFile.ExtractToDirectory(filePath, destinationFolder, true));

            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error extracting the file: {filePath}\n\n" +
                                        $"Method: ExtractDownloadFilesAsync2\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            ExtractionFailedMessageBox();

            return false;
        }
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
    
    private static void ExtractionFailedMessageBox()
    {
        string extractionfailed2 = (string)Application.Current.TryFindResource("Extractionfailed") ?? "Extraction failed.";
        string ensurethefileisnotcorrupted2 = (string)Application.Current.TryFindResource("Ensurethefileisnotcorrupted") ?? "Ensure the file is not corrupted.";
        string grantadministrativeaccesstoSimple2 = (string)Application.Current.TryFindResource("GrantadministrativeaccesstoSimple") ?? "Grant administrative access to 'Simple Launcher'.";
        string ensureSimpleLauncherisinawritable2 = (string)Application.Current.TryFindResource("EnsureSimpleLauncherisinawritable") ?? "Ensure 'Simple Launcher' is in a writable folder.";
        string temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirussoftware") ?? "Temporarily disable your antivirus software and try again.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{extractionfailed2}\n\n" +
                        $"{ensurethefileisnotcorrupted2}\n" +
                        $"{grantadministrativeaccesstoSimple2}\n" +
                        $"{ensureSimpleLauncherisinawritable2}\n" +
                        $"{temporarilydisableyourantivirussoftware2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private static void FileNeedToBeCompressedMessageBox()
    {
        string theselectedfilecannotbe2 = (string)Application.Current.TryFindResource("Theselectedfilecannotbe") ?? "The selected file cannot be extracted.";
        string toextractafileitneedstobe2 = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        string pleasefixthatintheEditwindow2 = (string)Application.Current.TryFindResource("PleasefixthatintheEditwindow") ?? "Please fix that in the Edit window.";
        string invalidFile2 = (string)Application.Current.TryFindResource("InvalidFile") ?? "Invalid File";
        MessageBox.Show($"{theselectedfilecannotbe2}\n\n" +
                        $"{toextractafileitneedstobe2}\n\n" +
                        $"{pleasefixthatintheEditwindow2}",
            invalidFile2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    private static void DecompressorAppIsNotAvailableMessageBox()
    {
        string theappropriateversionof7Zexe2 = (string)Application.Current.TryFindResource("Theappropriateversionof7zexe") ?? "The appropriate version of '7z.exe' was not found in the application folder!";
        string simpleLauncherwillnotbeabletoextract2 = (string)Application.Current.TryFindResource("SimpleLauncherwillnotbeabletoextract") ?? "'Simple Launcher' will not be able to extract compressed files.";
        string doyouwanttoautomaticallyreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = MessageBox.Show($"{theappropriateversionof7Zexe2}\n\n" +
                                               $"{simpleLauncherwillnotbeabletoextract2}\n\n" +
                                               $"{doyouwanttoautomaticallyreinstall2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (messageBoxResult == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually.";
            string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually2,
                warning2, MessageBoxButton.OK,MessageBoxImage.Warning);
        }
    }
    
    private static void DownloadErrorMessageBox()
    {
        string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloaderror2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}