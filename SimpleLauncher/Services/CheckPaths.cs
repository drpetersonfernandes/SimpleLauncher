using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CheckPaths
{
    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        // Directly check if the path exists (for absolute paths)
        if (Directory.Exists(path) || File.Exists(path)) return true;

        // Allow relative paths
        // Combine with the base directory to check for relative paths
        try
        {
            // Ensure we correctly handle relative paths that go up from the base directory
            var fullPath = PathHelper.SinglePathReturnAbsolutePathInsideApplicationFolderIfNeeded(path);
            return Directory.Exists(fullPath) || File.Exists(fullPath);
        }
        catch (Exception)
        {
            // If there's any exception parsing the path, consider it invalid
            return false;
        }
    }
}