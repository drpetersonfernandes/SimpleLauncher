using System;
using System.IO;

namespace SimpleLauncher;

public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    // Arrays of directories and files to clean up
    private static readonly string[] DirectoriesToClean =
    [
        Path.Combine(AppDirectory, "temp"),
        Path.Combine(AppDirectory, "temp2"),
        Path.Combine(Path.GetTempPath(), "SimpleLauncher")
    ];

    private static readonly string[] FilesToClean =
    [
        Path.Combine(AppDirectory, "update.zip"),
        Path.Combine(AppDirectory, "mame.xml")
    ];

    public static void CleanupTrash()
    {
        // Clean directories
        foreach (var directory in DirectoriesToClean)
        {
            DeleteDirectorySafely(directory);
        }
    }

    private static void DeleteDirectorySafely(string path)
    {
        if (!Directory.Exists(path)) return;

        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to delete directory.");
        }
    }

    private static void DeleteFileSafely(string path)
    {
        if (!File.Exists(path)) return;

        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to delete files.");
        }
    }
}