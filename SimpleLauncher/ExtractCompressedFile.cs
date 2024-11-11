using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

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
            // Choose the correct 7z executable path based on user environment (x64, x32 or arm) 
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
    }
}