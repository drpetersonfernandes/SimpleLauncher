using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher;

/// <summary>
/// Provides methods to clean up temporary files and folders used by the Simple Launcher application.
/// </summary>
public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string TempFolder = Path.Combine(AppDirectory, "temp");
    private static readonly string TempFolder2 = Path.Combine(AppDirectory, "temp2");
    private static readonly string TempFolder3 = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
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
            
            // Delete TempFolder3
            if (Directory.Exists(TempFolder3))
            {
                Directory.Delete(TempFolder3, true);
            }

            // Delete UpdateFile
            if (File.Exists(UpdateFile))
            {
                File.Delete(UpdateFile);
            }
        }
        catch (Exception ex)
        {
            string contextMessage = $"Error occurred while cleaning the 'Simple Launcher' temp folders and files.\n" +
                                    $"Method: CleanupTrash\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}\n" +
                                    $"Stack trace: {ex.StackTrace}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
}