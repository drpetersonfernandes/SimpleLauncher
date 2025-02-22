using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleLauncher;

public abstract class FileManager
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

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
            catch (Exception ex)
            {
                // Notify developer
                var errorMessage = $"There was an error using the method GetFilesAsync.\n\n" +
                                   $"Exception type: {ex.GetType().Name}\n" +
                                   $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.ErrorFindingGameFilesMessageBox(LogPath);

                return []; // Return an empty list
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
            return fileExtensions.Select(extension => $"*.{extension}").Select(searchPattern => Directory.EnumerateFiles(folderPath, searchPattern).Count()).Sum();
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = "An error occurred while counting files.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorWhileCountingFilesMessageBox(LogPath);

            return 0; // return 0 if an error occurs
        }
    }
}