namespace SimpleLauncher.Services.GameLauncher.MountFiles;

using Interfaces;

/// <summary>
/// Locates the default.xbe file (original Xbox executable) in a given directory.
/// </summary>
public static class FindDefaultXbe
{
    /// <summary>
    /// Searches for default.xbe in the specified root directory.
    /// </summary>
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
