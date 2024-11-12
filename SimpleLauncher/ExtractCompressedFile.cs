using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SimpleLauncher
{
    internal class ExtractCompressedFile
    {
        private static readonly Lazy<ExtractCompressedFile> Instance = new(() => new ExtractCompressedFile());
        public static ExtractCompressedFile Instance2 => Instance.Value;
        private readonly List<string> _tempDirectories = new();
        
        // Use the application's directory for the temporary directory
        private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _tempFolder = Path.Combine(AppDirectory, "temp");
        
        private ExtractCompressedFile() { } // Private constructor to enforce a singleton pattern

        public async Task<string> ExtractArchiveToTempAsync(string archivePath)
        {
            string extension = Path.GetExtension(archivePath)?.ToLower();

            if (extension != ".7z" && extension != ".zip" && extension != ".rar")
            {
                MessageBox.Show($"The selected file '{archivePath}' cannot be extracted.\n\n" +
                                $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                                $"Please go to Edit System - Expert Mode, and edit this system.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        string errorMessage = $"Extraction of the compressed file failed.\n\nExit code: {process.ExitCode}\nOutput: {output}\nError: {error}";
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
                string errorMessage = $"Extraction of the compressed file failed.\n\nThe file {archivePath} may be corrupted.";
                await LogErrors.LogErrorAsync(ex, errorMessage);

                MessageBox.Show($"Extraction of the compressed file failed!\n" +
                                $"The file {archivePath} may be corrupted.\n" +
                                $"Or maybe Simple Launcher does not have enough privileges to run in your system.\n" +
                                $"Try to run with administrative privileges.\n\n" +
                                $"If you want to debug the error you can see the file 'error_user.log' inside Simple Launcher folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return Path.Combine(baseDirectory, "7z_arm64.exe");
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
                if (Directory.Exists(dir))
                {
                    try
                    {
                        // Delete generated temp folders
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        string contextMessage = $"Error occurred while cleaning up temp directories.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                    }
                }
            }
            // Clear the list after deleting
            _tempDirectories.Clear();
            
            try
            {
                // Delete temp folder
                if (Directory.Exists(_tempFolder))
                {
                    Directory.Delete(_tempFolder, true);
                }
            }
            catch (Exception ex)
            {
                string contextMessage = $"Error occurred while deleting the temp folder.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
            }
        }
        
        public async Task<bool> ExtractFileWith7ZipAsync(string filePath, string destinationFolder)
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                string formattedException = $"The downloaded file appears to be empty or corrupted.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                
                MessageBox.Show("The downloaded file appears to be empty or corrupted.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            if (IsFileLocked(filePath))
            {
                if (IsRunningAsAdministrator())
                {
                    await RunHandleExeAndLogProcess(filePath);
                }
                else
                {
                    var restartAsAdminResult = MessageBox.Show(
                        "The downloaded file appears to be locked by another process.\n\n" +
                        "'Simple Launcher' does not have administrative privileges to detect the cause of this lock.\n\n" +
                        "Would you like to restart 'Simple Launcher' with administrative privileges to diagnose the issue?",
                        "File Lock Detected",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (restartAsAdminResult == MessageBoxResult.Yes)
                    {
                        RestartAsAdministrator();
                        MessageBox.Show("Please retry the download and extraction after 'Simple Launcher' restarts with administrative privileges.", "Restart with Admin Privileges", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
            }
            
            try
            {
                // Get the correct 7z executable path based on user environment (x64, x86 or arm64)
                string sevenZipPath = Get7ZipExecutablePath();

                if (!File.Exists(sevenZipPath))
                {
                    string formattedException = $"The required 7z executable was not found in the application folder.";
                    Exception exception = new(formattedException);
                    await LogErrors.LogErrorAsync(exception, formattedException);
                    
                    // Ask the user if they want to automatically reinstall Simple Launcher
                    var messageBoxResult = MessageBox.Show(
                        "The appropriate version of 7z.exe was not found in the application folder!\n\n" +
                        "'Simple Launcher' will not be able to extract compressed files.\n\n" +
                        "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?",
                        "Extraction Error",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                    }
                    else
                    {
                        MessageBox.Show("Please reinstall 'Simple Launcher' to fix the problem.","Warning", MessageBoxButton.OK,MessageBoxImage.Warning);
                    }
                    
                    return false;
                }
                
                // Create destination folder if it does not exist
                Directory.CreateDirectory(destinationFolder);

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
                    string formattedException = $"Error extracting the file: {filePath}\n\nError message: {error}";
                    Exception ex = new(formattedException);
                    await LogErrors.LogErrorAsync(ex, formattedException);
            
                    MessageBox.Show($"Error extracting the file: {filePath}\n\n" +
                                    $"The file might be corrupted or locked by some other process.\n\n" +
                                    $"Some antivirus programs may lock, block extraction or scan newly downloaded files, causing access issues. Try to temporarily disable real-time protection.\n\n",
                        "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                string formattedException = $"Error extracting the file: {filePath}\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);

                MessageBox.Show($"Error extracting the file: {filePath}\n\n" +
                                $"The file might be corrupted or locked by some other process.\n\n" +
                                $"Some antivirus programs may lock, block extraction or scan newly downloaded files, causing access issues. Try to temporarily disable real-time protection.\n\n",
                    "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);

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
        
        private bool IsRunningAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Using Handle from Microsoft to figure out which program is locking the downloaded file
        private async Task RunHandleExeAndLogProcess(string filePath)
        {
            string handleExePath = GetHandleExecutablePath();
    
            if (!File.Exists(handleExePath))
            {
                string formattedException = $"{Path.GetFileName(handleExePath)} was not found in the application folder.";
                await LogErrors.LogErrorAsync(new FileNotFoundException(formattedException), formattedException);

                MessageBox.Show("'Simple Launcher' could not find the appropriate handle executable.\n\n" +
                                "The error has been reported to the developer, who will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = handleExePath,
                Arguments = filePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            string logMessageUser = string.IsNullOrEmpty(output) ? "No process found locking the file." : output;
            string logMessageDeveloper = string.IsNullOrEmpty(output) ? "No process found locking the file." : $"Handle output: {output}\nHandle error: {error}";
            await LogErrors.LogErrorAsync(new Exception("File lock detected"), logMessageDeveloper);

            MessageBox.Show($"The following process is locking the file:\n\n" +
                            $"{logMessageUser}\n\n" +
                            $"You may try to stop this process to proceed with the extraction.\n\n" +
                            $"A report has been sent to the developer, who will attempt to fix the issue and prevent it from happening again.", "File Lock Detected", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private string GetHandleExecutablePath()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (Environment.Is64BitOperatingSystem)
            {
                // Check if the system is ARM64
                if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "ARM64")
                {
                    return Path.Combine(appDirectory, "handle_arm64.exe");
                }
                else
                {
                    return Path.Combine(appDirectory, "handle.exe");
                }
            }
            else
            {
                // 32-bit system
                return Path.Combine(appDirectory, "handle_x86.exe");
            }
        }

        private void RestartAsAdministrator()
        {
            // Prepare the process start info
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = processModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                // Start new application instance
                Process.Start(startInfo);

                // Shutdown current application instance
                Application.Current.Shutdown();
            }
        }
    }
}