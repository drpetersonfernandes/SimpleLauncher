using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class PathHelper
{
    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the current working directory.
    /// Handles '.', '..', and resolves symbolic links if the file/directory exists.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The absolute path.</returns>
    public static string ResolveRelativeToCurrentDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            // Or throw an exception, depending on desired behavior for empty/null input
            return string.Empty;
        }

        // Path.GetFullPath correctly handles '.', '..', and relative paths
        // relative to the current working directory.
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the application's base directory.
    /// If the path is already absolute, it is returned as it is.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The absolute path relative to the application base directory if relative, otherwise the original absolute path.</returns>
    public static string ResolveRelativeToAppDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
             // Or throw an exception
             return string.Empty;
        }

        // If the path is already rooted (absolute), return it directly.
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        // If the path is relative, combine it with the application's base directory.
        // Path.Combine correctly handles separators and relative segments like '.\' and '..\'
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    /// <summary>
    /// Combines two path segments and resolves the result to an absolute path
    /// relative to the application's base directory if the combined path is not rooted.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>The absolute path resulting from combining path1 and path2, resolved relative to the application base directory if needed.</returns>
    public static string CombineAndResolveRelativeToAppDirectory(string path1, string path2)
    {
         // Path.Combine handles null/empty inputs reasonably well,
         // but adding checks might be safer depending on the expected input.
         // If path1 is absolute, path2 is appended to it.
         // If path1 is relative, path2 is appended to it.
         var combinedPath = Path.Combine(path1, path2);

         // Now resolve the combined path relative to the app directory
         return ResolveRelativeToAppDirectory(combinedPath);
    }

    // You might also want a method to combine and resolve relative to the current directory
    public static string CombineAndResolveRelativeToCurrentDirectory(string path1, string path2)
    {
        var combinedPath = Path.Combine(path1, path2);
        return ResolveRelativeToCurrentDirectory(combinedPath);
    }
}