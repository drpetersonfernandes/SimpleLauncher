using System.IO;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

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
            App.LogErrorAsync(ex, $"Error finding default.xbe in path: {rootPath}");
            return null;
        }
    }
}