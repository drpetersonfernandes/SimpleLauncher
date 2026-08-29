using SimpleLauncher.Core.Services.CheckPaths;

namespace SimpleLauncher.Core.Services.CheckForFileLock;

/// <summary>
///     Provides methods to check whether a file is currently locked by another process.
/// </summary>
public static class CheckForFileLockService
{
    /// <summary>
    ///     Determines whether the file at the given path is locked by another process.
    /// </summary>
    /// <param name="filePath">The path of the file to check.</param>
    /// <returns>True if the file is locked, false otherwise.</returns>
    public static bool IsFileLocked(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        // Resolve the path to an absolute path, handling placeholders like %BASEFOLDER%
        var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(filePath);
        if (string.IsNullOrEmpty(resolvedPath))
            return false; // Path could not be resolved

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