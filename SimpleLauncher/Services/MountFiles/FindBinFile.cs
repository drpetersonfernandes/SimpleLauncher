using System;
using System.IO;

namespace SimpleLauncher.Services.MountFiles;

public static class FindBinFile
{
    public static string Find(string rootPath)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var binFiles = Directory.GetFiles(rootPath, "*.bin");
            if (binFiles.Length > 0)
            {
                return binFiles[0];
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error finding bin file: {ex.Message}");
            return null;
        }
    }
}