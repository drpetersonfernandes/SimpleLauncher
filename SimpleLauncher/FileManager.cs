using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

public abstract class FileManager
{
    public static async Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions)
    {
        return await Task.Run(async () =>
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return new List<string>();
                }
                var foundFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(directoryPath, ext)).ToList();
                return foundFiles;
            }
            catch (Exception ex)
            {
                string errorMessage = $"There was an error using the method GetFilesAsync in the Main window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);

                MessageBox.Show("There was an error finding the game files.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<string>();
            }
        });
    }

    public static async Task<List<string>> FilterFilesAsync(List<string> files, string startLetter)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(startLetter))
                return files; // If no startLetter is provided, no filtering is required

            if (startLetter == "#")
            {
                return files.Where(file => char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }

            return files.Where(file => Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
        });
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
            string contextMessage = $"An error occurred while counting files in the Main window.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("An error occurred while counting files.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return 0;
        }
    }
}