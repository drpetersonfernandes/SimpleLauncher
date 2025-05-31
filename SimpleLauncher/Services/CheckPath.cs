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
            return false;
        }

        try
        {
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(path);

            // Check if the resolved path exists as a file or directory
            return !string.IsNullOrEmpty(resolvedPath) && (File.Exists(resolvedPath) || Directory.Exists(resolvedPath));
        }
        catch
        {
            return false;
        }
    }
}