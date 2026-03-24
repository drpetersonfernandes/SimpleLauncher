using System;
using System.IO;

namespace SimpleLauncher.Services.MountFiles;

public static class FindDefaultXbe
{
    public static string Find(string rootPath)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var defaultXbePath = Path.Combine(rootPath, "default.xbe");
            if (File.Exists(defaultXbePath))
            {
                return defaultXbePath;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error finding default.xbe: {ex.Message}");
            return null;
        }
    }
}