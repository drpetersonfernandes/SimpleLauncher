using System;
using System.IO;

namespace SimpleLauncher.Services.MountFiles;

public static class FindImageIso
{
    public static string Find(string rootPath)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var defaultImagePath = Path.Combine(rootPath, "image.iso");
            if (File.Exists(defaultImagePath))
            {
                return defaultImagePath;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error finding image.iso: {ex.Message}");
            return null;
        }
    }
}