using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher
{
    public static class LoadFiles
    {
        public static async Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (!Directory.Exists(directoryPath))
                    {
                        return [];
                    }
                    var foundFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(directoryPath, ext)).ToList();
                    return foundFiles;
                }
                catch (Exception exception)
                {
                    string errorMessage = $"There was an error getting the list of files from folder.\n\nException details: {exception}";
                    await LogErrors.LogErrorAsync(exception, errorMessage);
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return [];
                }
            });
        }

        public static List<string> FilterFiles(List<string> files, string startLetter)
        {
            if (string.IsNullOrEmpty(startLetter))
                return files; // If no startLetter is provided, no filtering is required

            if (startLetter == "#")
            {
                return files.Where(file => char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }
            else
            {
                return files.Where(file => Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        public static int CountFiles(string folderPath, List<string> fileExtensions)
        {
            if (!Directory.Exists(folderPath))
            {
                return 0;
            }

            try
            {
                int fileCount = 0;

                foreach (string extension in fileExtensions)
                {
                    string searchPattern = $"*.{extension}";
                    fileCount += Directory.EnumerateFiles(folderPath, searchPattern).Count();
                }

                return fileCount;
            }
            catch (Exception ex)
            {
                string contextMessage = $"An error occurred while counting files.\n\nException details: {ex}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                MessageBox.Show($"An error occurred while counting files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logTask.Wait(TimeSpan.FromSeconds(2));
                return 0;
            }
        }

    }
}