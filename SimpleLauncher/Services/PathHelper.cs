using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class PathHelper
{
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

        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the application's base directory.
    /// If the path is already absolute, it is canonicalized.
    /// Returns a canonical absolute path.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The canonical absolute path (relative to the application base directory if the input was relative, otherwise the canonicalized original absolute path).</returns>
    public static string ResolveRelativeToAppDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        // If the path is already rooted (absolute), canonicalize it.
        // Path.GetFullPath will handle this and also resolve any '.' or '..' segments.
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        // If the path is relative, combine it with the application's base directory.
        // Path.Combine correctly handles separators.
        var combinedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

        // Path.GetFullPath will then resolve '.\', '..\' segments from the combined path
        // and ensure it's a fully qualified, canonical path.
        return Path.GetFullPath(combinedPath);
    }

    /// <summary>
    /// Combines two path segments and resolves the result to a canonical absolute path
    /// relative to the application's base directory if the combined path is not rooted.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>The canonical absolute path resulting from combining path1 and path2, resolved relative to the application base directory if needed.</returns>
    public static string CombineAndResolveRelativeToAppDirectory(string path1, string path2)
    {
         // Path.Combine handles null/empty inputs reasonably well.
         // If path1 is absolute, path2 is appended to it (unless path2 is also absolute).
         // If path1 is relative, path2 is appended to it.
         var combinedPath = Path.Combine(path1, path2);

         // Now resolve the combined path relative to the app directory
         // ResolveRelativeToAppDirectory will also canonicalize it.
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
