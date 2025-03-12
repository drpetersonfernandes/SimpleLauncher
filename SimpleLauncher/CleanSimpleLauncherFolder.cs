using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleLauncher;

public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    // Arrays of directories and files to clean up
    private static readonly string[] DirectoriesToClean =
    {
        Path.Combine(AppDirectory, "temp"),
        Path.Combine(AppDirectory, "temp2"),
        Path.Combine(Path.GetTempPath(), "SimpleLauncher")
    };

    private static readonly string[] FilesToClean =
    {
        Path.Combine(AppDirectory, "update.zip"),
        Path.Combine(AppDirectory, "mame.xml")
    };

    // Files to be excluded from cleanup
    private static readonly HashSet<string> ExcludedFiles = new()
    {
        // Add any files you want to exclude from cleanup
        // Example: Path.Combine(AppDirectory, "important.txt")
    };

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
            if (!ExcludedFiles.Contains(file))
            {
                DeleteFileSafely(file);
            }
        }
    }

    private static void DeleteDirectorySafely(string path)
    {
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }

    private static void DeleteFileSafely(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}