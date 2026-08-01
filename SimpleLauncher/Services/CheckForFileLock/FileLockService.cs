using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckPaths;

namespace SimpleLauncher.Services.CheckForFileLock;

/// <summary>
/// Provides a service to check whether a file is currently locked by another process.
/// </summary>
public class FileLockService : IFileLockService
{
    /// <summary>
    /// Determines whether the file at the given path is locked by another process.
    /// </summary>
    /// <param name="filePath">The path of the file to check.</param>
    /// <returns>True if the file is locked, false otherwise.</returns>
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
