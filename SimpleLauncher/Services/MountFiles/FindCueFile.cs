using System;
using System.IO;

namespace SimpleLauncher.Services.MountFiles;

public class FindCueFile
{
    public static string Find(string rootPath)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var cueFiles = Directory.GetFiles(rootPath, "*.cue");
            if (cueFiles.Length > 0)
            {
                return cueFiles[0];
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error finding cue file: {ex.Message}");
            return null;
        }
    }
}