using System.IO;

namespace SimpleLauncher.Services;

public static class CheckPath
{
    /// <summary>
    /// Checks if a path is valid and exists as a file or directory.
    /// Handles absolute paths, relative paths, and paths using the %BASEFOLDER% placeholder.
    /// </summary>
    /// <param name="path">The path string to check.</param>
    /// <returns>True if the path is valid and exists, false otherwise.</returns>
    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false; // Empty or whitespace paths are not considered valid existing paths
        }

        try
        {
            // Resolve the path relative to the app directory, which now handles %BASEFOLDER%
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(path);

            // Check if the resolved path exists as a file or directory
            return !string.IsNullOrEmpty(resolvedPath) && (File.Exists(resolvedPath) || Directory.Exists(resolvedPath));
        }
        catch
        {
            // Any exception during path resolution or checking means it's invalid
            return false;
        }
    }
}