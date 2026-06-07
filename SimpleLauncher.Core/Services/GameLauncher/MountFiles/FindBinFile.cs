using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Core.Services.GameLauncher.MountFiles;

public static class FindBinFile
{
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