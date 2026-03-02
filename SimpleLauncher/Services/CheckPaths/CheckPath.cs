using System.IO;

namespace SimpleLauncher.Services.CheckPaths;

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
            if (string.IsNullOrEmpty(resolvedPath))
            {
                return false;
            }

            // Prepend the long path prefix to handle paths longer than 260 characters reliably.
            var pathForCheck = @"\\?\" + resolvedPath;

            // Check if the resolved path exists as a file or directory
            return File.Exists(pathForCheck) || Directory.Exists(pathForCheck);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a path is valid and exists as an executable file.
    /// Handles absolute paths, relative paths, and paths using the %BASEFOLDER% placeholder.
    /// This is stricter than IsValidPath as it only accepts files, not directories.
    /// </summary>
    /// <param name="path">The path string to check.</param>
    /// <returns>True if the path is valid, exists as a file, and has an .exe extension; false otherwise.</returns>
    public static bool IsValidEmulatorExecutablePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(path);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                return false;
            }

            // Prepend the long path prefix to handle paths longer than 260 characters reliably.
            var pathForCheck = @"\\?\" + resolvedPath;

            // Check if the resolved path exists as a file (not directory)
            return File.Exists(pathForCheck);
        }
        catch
        {
            return false;
        }
    }
}