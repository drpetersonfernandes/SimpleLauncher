using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class PathHelper
{
    private const string BaseFolderPlaceholder = "%BASEFOLDER%";

    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the current working directory.
    /// Handles '.', '..', and resolves symbolic links if the file/directory exists.
    /// Returns a canonical absolute path.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The canonical absolute path relative to the current working directory.</returns>
    public static string ResolveRelativeToCurrentDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        // This method is specifically for current directory, so no %BASEFOLDER% handling here.
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the application's base directory.
    /// If the path is already absolute, it is canonicalized.
    /// Handles the %BASEFOLDER% placeholder.
    /// Returns a canonical absolute path.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The canonical absolute path (relative to the application base directory if the input was relative or used %BASEFOLDER%, otherwise the canonicalized original absolute path).</returns>
    public static string ResolveRelativeToAppDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        string basePath;
        var remainingPath = path;

        // Check for the %BASEFOLDER% placeholder
        if (path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            basePath = AppDomain.CurrentDomain.BaseDirectory;
            // Remove the placeholder and any trailing separators
            remainingPath = path.Substring(BaseFolderPlaceholder.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        else if (Path.IsPathRooted(path))
        {
            // If the path is already rooted (absolute), use it directly as the base.
            // Path.GetFullPath below will canonicalize it.
            basePath = string.Empty; // Path.Combine handles this case correctly
        }
        else
        {
            // If the path is relative and doesn't use %BASEFOLDER%,
            // resolve it relative to the application's base directory.
            basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        try
        {
            // Path.Combine handles combining basePath and remainingPath correctly,
            // including cases where basePath is empty (for absolute paths).
            var combinedPath = Path.Combine(basePath, remainingPath);

            // Path.GetFullPath resolves '.\', '..\' segments and ensures it's canonical.
            return Path.GetFullPath(combinedPath);
        }
        catch (Exception ex)
        {
            // Log the error but return the original path or empty string
            // depending on how the caller handles invalid paths.
            // For now, let's return an empty string to indicate resolution failure.
            // The calling code (like validation or launcher) should handle the empty string.
            _ = LogErrors.LogErrorAsync(ex, $"Error resolving path '{path}' relative to app directory.");
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if a path is relative and does NOT start with the %BASEFOLDER% placeholder.
    /// </summary>
    /// <param name="path">The path string.</param>
    /// <returns>True if the path is relative and does not start with %BASEFOLDER%, false otherwise.</returns>
    public static bool IsRelativePathWithoutBaseFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // It's relative if it's not rooted AND doesn't start with the placeholder
        return !Path.IsPathRooted(path) && !path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Combines two path segments and resolves the result to a canonical absolute path
    /// relative to the application's base directory if the combined path is not rooted.
    /// Handles the %BASEFOLDER% placeholder in path1.
    /// </summary>
    /// <param name="path1">The first path segment (can contain %BASEFOLDER%).</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>The canonical absolute path resulting from combining path1 and path2, resolved relative to the application base directory if needed.</returns>
    public static string CombineAndResolveRelativeToAppDirectory(string path1, string path2)
    {
         // Resolve path1 first, which handles %BASEFOLDER% and relative-to-app resolution
         var resolvedPath1 = ResolveRelativeToAppDirectory(path1);

         // If path1 resolution failed, combining will also likely fail or be meaningless
         if (string.IsNullOrEmpty(resolvedPath1))
         {
             return string.Empty;
         }

         // Combine the resolved path1 with path2.
         // Path.Combine handles cases where path2 is absolute (it will ignore path1).
         var combinedPath = Path.Combine(resolvedPath1, path2);

         // Resolve the final combined path. This handles cases where path2 was absolute
         // or resolves any remaining '.' or '..' segments from the combination.
         return ResolveRelativeToAppDirectory(combinedPath);
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Sanitizes a path string intended for use as a token in string replacements.
    /// It removes any trailing directory separator characters.
    /// E.g., "C:\MyFolder\" becomes "C:\MyFolder".
    /// This helps prevent double separators when concatenating with a path segment like "\subfolder".
    /// </summary>
    /// <param name="pathTokenValue">The path string to sanitize.</param>
    /// <returns>The sanitized path string without a trailing separator.</returns>
    public static string SanitizePathToken(string pathTokenValue)
    {
        if (string.IsNullOrEmpty(pathTokenValue))
        {
            return string.Empty;
        }

        return pathTokenValue.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}

