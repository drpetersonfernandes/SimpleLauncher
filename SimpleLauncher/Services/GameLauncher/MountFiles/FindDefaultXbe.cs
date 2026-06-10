using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public static class FindDefaultXbe
{
    public static string Find(string rootPath, ILogErrors logErrors)
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
            logErrors.LogAndForget(ex, $"Error finding default.xbe in path: {rootPath}");
            return null;
        }
    }
}