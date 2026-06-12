namespace SimpleLauncher.Services.GameLauncher.MountFiles;

using Interfaces;

/// <summary>
/// Locates the default.xex file (Xbox 360 executable) in a given directory.
/// </summary>
public static class FindDefaultXex
{
    /// <summary>
    /// Searches for default.xex in the specified root directory.
    /// </summary>
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
