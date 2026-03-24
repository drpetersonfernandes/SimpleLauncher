using System;
using System.IO;

namespace SimpleLauncher.Services.MountFiles;

public static class FindDefaultXex
{
    public static string Find(string rootPath)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var defaultXexPath = Path.Combine(rootPath, "default.xex");
            if (File.Exists(defaultXexPath))
            {
                return defaultXexPath;
            }

            // If not found in root, check for common subfolders like "game" if necessary,
            // but the prompt says it is in the root of the mounted drive.
            return null;
        }
        catch (Exception ex)
        {
            // Handle or log exception as needed
            Console.WriteLine($@"Error finding default.xex: {ex.Message}");
            return null;
        }
    }
}