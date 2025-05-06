using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher.Services;

public class CountFiles
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

    public static async Task<int> CountFilesAsync(string folderPath, List<string> fileExtensions)
    {
        // Create and show the PleaseWaitWindow
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
                            totalCount += Directory.EnumerateFiles(folderPath, searchPattern).Count();
                        }
                        catch (Exception innerEx)
                        {
                            // Log the specific extension that caused the problem but continue counting
                            var contextMessage = $"Error counting files with extension '{extension}' in '{folderPath}'.";
                            _ = LogErrors.LogErrorAsync(innerEx, contextMessage);
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