using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;

namespace SimpleLauncher.Core.Services.CheckForFileLock;

public class FileLockService : IFileLockService
{
    public bool IsFileLocked(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(filePath);
        if (string.IsNullOrEmpty(resolvedPath))
            return false;

        var longPath = PathHelper.GetLongPath(resolvedPath);

        if (!File.Exists(longPath))
            return false;

        try
        {
            using FileStream stream = new(longPath, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }
}
