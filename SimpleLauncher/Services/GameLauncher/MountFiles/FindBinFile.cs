using System.IO;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

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
            App.LogErrorAsync(ex, $"Error finding bin file in path: {rootPath}");
            return null;
        }
    }
}