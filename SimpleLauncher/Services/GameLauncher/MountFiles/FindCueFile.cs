namespace SimpleLauncher.Services.GameLauncher.MountFiles;

using Interfaces;

/// <summary>
/// Locates the first .cue file in a given directory.
/// </summary>
public static class FindCueFile
{
    /// <summary>
    /// Searches for the first .cue file in the specified root directory.
    /// </summary>
    public static string Find(string rootPath, ILogErrors logErrors)
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
            logErrors.LogAndForget(ex, $"Error finding cue file in path: {rootPath}");
            return null;
        }
    }
}
