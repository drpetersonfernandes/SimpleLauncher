using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CheckPaths
{
    // Check paths in SystemFolder, SystemImageFolder and EmulatorLocation. Allow relative paths.
    public static bool IsValidPath(string path)
    {
        // Check if the path is not null or whitespace
        if (string.IsNullOrWhiteSpace(path)) return false;

        // Check if the path is an absolute path and exists
        if (Directory.Exists(path) || File.Exists(path)) return true;

        // Assume the path might be relative and combine it with the base directory
        // Allow relative paths
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var fullPath = Path.Combine(basePath, path);

        // Check if the combined path exists
        return Directory.Exists(fullPath) || File.Exists(fullPath);
    }
}