using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CheckForFileLock
{
    public static bool IsFileLocked(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        // Prepend long path prefix if not already present, assuming absolute path
        var longPath = filePath.StartsWith(@"\\?\", StringComparison.Ordinal) ? filePath : @"\\?\" + filePath;

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