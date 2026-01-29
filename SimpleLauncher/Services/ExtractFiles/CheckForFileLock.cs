using System;
using System.IO;

namespace SimpleLauncher.Services.ExtractFiles;

public static class CheckForFileLock
{
    public static bool IsFileLocked(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        // Resolve the path to an absolute path, handling placeholders like %BASEFOLDER%
        var resolvedPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(filePath);
        if (string.IsNullOrEmpty(resolvedPath))
            return false; // Path could not be resolved

        var longPath = resolvedPath.StartsWith(@"\\?\", StringComparison.Ordinal) ? resolvedPath : @"\\?\" + resolvedPath;

        if (!File.Exists(longPath))
            return false;

        try
        {
            using FileStream stream = new(longPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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