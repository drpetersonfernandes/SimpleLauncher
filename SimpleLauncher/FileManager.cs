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
                    return new List<string>(); // Return an empty list
                }

                var foundFiles = new List<string>();

                foreach (var ext in fileExtensions)
                {
                    try
                    {
                        foundFiles.AddRange(Directory.GetFiles(directoryPath, ext));
                    }
                    catch (Exception innerEx)
                    {
                        // Log the specific extension that caused the problem
                        var errorMessage = $"Error processing extension '{ext}' in directory '{directoryPath}'.\n" +
                                           $"Exception type: {innerEx.GetType().Name}\n" +
                                           $"Exception details: {innerEx.Message}";
                        await LogErrors.LogErrorAsync(innerEx, errorMessage);
                        // Continue with the next extension rather than failing the entire operation
                    }
                }

                return foundFiles;
            }
            catch (Exception ex)
            {
                // Notify developer
                var errorMessage = $"There was an error using the method GetFilesAsync.\n\n" +
                                   $"Exception type: {ex.GetType().Name}\n" +
                                   $"Exception details: {ex.Message}\n" +
                                   $"Directory path: {directoryPath}";
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.ErrorFindingGameFilesMessageBox(LogPath);

                return new List<string>(); // Return an empty list
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
                return files.Where(file => !string.IsNullOrEmpty(file) &&
                                           file.Length > 0 &&
                                           char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }

            return files.Where(file => !string.IsNullOrEmpty(file) &&
                                       Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
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
            var totalCount = 0;
            foreach (var extension in fileExtensions)
            {
                try
                {
                    var searchPattern = $"*.{extension}";
                    totalCount += Directory.EnumerateFiles(folderPath, searchPattern).Count();
                }
                catch (Exception innerEx)
                {
                    // Log the specific extension that caused the problem but continue counting
                    var contextMessage = $"Error counting files with extension '{extension}' in '{folderPath}'.\n" +
                                         $"Exception type: {innerEx.GetType().Name}\n" +
                                         $"Exception details: {innerEx.Message}";
                    LogErrors.LogErrorAsync(innerEx, contextMessage).Wait(TimeSpan.FromSeconds(2));
                }
            }

            return totalCount;
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = "An error occurred while counting files.\n\n" +
                                 $"Folder path: {folderPath}\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorWhileCountingFilesMessageBox(LogPath);

            return 0; // return 0 if an error occurs
        }
    }
}