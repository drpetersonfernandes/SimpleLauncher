using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private static readonly string[] DirectoriesToClean =
    [
        Path.Combine(AppDirectory, "temp"),
        Path.Combine(AppDirectory, "temp2"),
        Path.Combine(Path.GetTempPath(), "SimpleLauncher")
    ];

    private static readonly string[] FilesToClean =
    [
        Path.Combine(AppDirectory, "update.zip"),
        Path.Combine(AppDirectory, "mame.xml"),
        Path.Combine(AppDirectory, "Updater.deps.json"),
        Path.Combine(AppDirectory, "Updater.dll"),
        Path.Combine(AppDirectory, "Updater.pdb"),
        Path.Combine(AppDirectory, "Updater.runtimeconfig.json"),
        Path.Combine(AppDirectory, "SimpleLauncher.pdb")
    ];

    public static void CleanupTrash()
    {
        // Clean directories
        foreach (var directory in DirectoriesToClean)
        {
            DeleteDirectorySafely(directory);
        }

        // Clean files
        foreach (var file in FilesToClean)
        {
            DeleteFileSafely(file);
        }
    }

    private static void DeleteDirectorySafely(string path)
    {
        if (!Directory.Exists(path)) return;

        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    private static void DeleteFileSafely(string path)
    {
        if (!File.Exists(path)) return;

        try
        {
            DeleteFiles.TryDeleteFile(path);
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}