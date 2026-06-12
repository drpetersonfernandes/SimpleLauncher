namespace SimpleLauncher.Services.GameLauncher.MountFiles;

using Interfaces;

/// <summary>
/// Locates the first .bin file in a given directory.
/// </summary>
public static class FindBinFile
{
    /// <summary>
    /// Searches for the first .bin file in the specified root directory.
    /// </summary>
    public static string Find(string rootPath, ILogErrors logErrors)
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
            logErrors.LogAndForget(ex, $"Error finding bin file in path: {rootPath}");
            return null;
        }
    }
}
