using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher.Services;

public static class CountFiles
{
    private static readonly string LogPath = GetLogPath.Path();

    public static async Task<int> CountFilesAsync(string folderPath, List<string> fileExtensions)
    {
        // Show the PleaseWaitWindow
        var processingpleasewait = (string)Application.Current.TryFindResource("Processingpleasewait") ?? "Processing, please wait...";
        var pleaseWaitWindow = new PleaseWaitWindow(processingpleasewait);

        try
        {
            // Show the window on the UI thread
            await Application.Current.Dispatcher.InvokeAsync(() => pleaseWaitWindow.Show());

            return await Task.Run(() =>
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
                            totalCount += Directory.EnumerateFiles(folderPath, searchPattern, SearchOption.TopDirectoryOnly).Count();
                        }
                        catch (DirectoryNotFoundException)
                        {
                            // Directory was deleted or inaccessible during enumeration
                            // Log the specific extension that caused the problem but continue counting
                            var contextMessage = $"Directory not found while counting files with extension '{extension}' in '{folderPath}'.";

                            // Notify developer
                            _ = LogErrors.LogErrorAsync(null, contextMessage);

                            // Continue with the next extension
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // No permission to access the directory
                            var contextMessage = $"Access denied while counting files with extension '{extension}' in '{folderPath}'.";

                            // Notify developer
                            _ = LogErrors.LogErrorAsync(null, contextMessage);

                            // Continue with the next extension
                        }
                        catch (Exception ex)
                        {
                            // Other exceptions during file enumeration
                            var contextMessage = $"Error counting files with extension '{extension}' in '{folderPath}'.";

                            // Notify developer
                            _ = LogErrors.LogErrorAsync(ex, contextMessage);

                            // Continue with the next extension
                        }
                    }

                    return totalCount;
                }
                catch (Exception ex)
                {
                    // Notify developer
                    var contextMessage = "An error occurred while counting files.\n" +
                                         $"Folder path: {folderPath}";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ErrorWhileCountingFilesMessageBox(LogPath);

                    return 0; // return 0 if an error occurs
                }
            });
        }
        finally
        {
            // Close the window on the UI thread
            Application.Current.Dispatcher.Invoke(() => pleaseWaitWindow.Close());
        }
    }
}