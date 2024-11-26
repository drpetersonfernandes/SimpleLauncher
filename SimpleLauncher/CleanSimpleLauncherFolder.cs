using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

/// <summary>
/// Provides methods to clean up temporary files and folders used by the Simple Launcher application.
/// </summary>
public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string TempFolder = Path.Combine(AppDirectory, "temp");
    private static readonly string TempFolder2 = Path.Combine(AppDirectory, "temp2");
    private static readonly string UpdateFile = Path.Combine(AppDirectory, "update.zip");

    /// <summary>
    /// Deletes temporary folders and files inside the Simple Launcher directory.
    /// </summary>
    public static void CleanupTrash()
    {
        try
        {
            // Delete TempFolder
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }

            // Delete TempFolder2
            if (Directory.Exists(TempFolder2))
            {
                Directory.Delete(TempFolder2, true);
            }

            // Delete UpdateFile
            if (File.Exists(UpdateFile))
            {
                File.Delete(UpdateFile);
            }
        }
        catch (Exception ex)
        {
            string contextMessage = $"Error occurred while cleaning the 'Simple Launcher' folder.\n\n" +
                                    $"Method: CleanupTrash\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}\n" +
                                    $"Stack trace: {ex.StackTrace}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show(
                "'Simple Launcher' could not clean up temporary folders and files inside its directory.\n\n" +
                "You will need to delete them manually.\n\n" +
                "This issue occurred because 'Simple Launcher' is running with insufficient privileges.\n" +
                "Try running it with administrative privileges.\n\n" +
                "To debug the error, you can see the file 'error_user.log' inside the 'Simple Launcher' folder.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}