using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public static class FindDefaultXex
{
    public static string Find(string rootPath, ILogErrors logErrors)
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

            return null;
        }
        catch (Exception ex)
        {
            logErrors.LogAndForget(ex, $"Error finding default.xex in path: {rootPath}");
            return null;
        }
    }
}