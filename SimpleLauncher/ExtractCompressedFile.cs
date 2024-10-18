using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher
{
    internal class ExtractCompressedFile
    {
        private static readonly Lazy<ExtractCompressedFile> Instance = new(() => new ExtractCompressedFile());
        public static ExtractCompressedFile Instance2 => Instance.Value;
        private readonly List<string> _tempDirectories = new();

        private ExtractCompressedFile() { } // Private constructor to enforce a singleton pattern

        public string ExtractArchiveToTemp(string archivePath)
        {
            // Use the application's directory for the temporary directory
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempDirectory = Path.Combine(appDirectory, "temp", Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            // Keep track of the temp directory
            _tempDirectories.Add(tempDirectory);

            // Path to the 7z.exe executable
            string sevenZipPath = Path.Combine(appDirectory, "7z.exe");

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

            using Process process = new();
            process.StartInfo = processStartInfo;
            process.Start();

            // Optionally, read the output and error streams
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string errorMessage = $"7z.exe process failed with exit code {process.ExitCode}\nOutput: {output}\nError: {error}";
                Exception exception = new(errorMessage);
                Task logTask = LogErrors.LogErrorAsync(exception, errorMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("Extraction of the compressed file failed.\n\nThe file may be corrupted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return tempDirectory;
        }

        public void Cleanup()
        {
            foreach (var dir in _tempDirectories)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception exception)
                    {
                        string contextMessage = $"Error occurred while cleaning up temp directories.\n\nException detail: {exception}";
                        Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                    }
                }
            }
            _tempDirectories.Clear();  // Clear the list after deleting
        }
    }
}
