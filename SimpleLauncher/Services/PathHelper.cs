using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class PathHelper
{
    public static string GetFullPath(string path)
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(appDir, path));
            return fullPath;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to get full path.");
            return string.Empty;
        }
    }
}