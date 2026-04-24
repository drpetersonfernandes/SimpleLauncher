using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public static class FindImageIso
{
    public static string Find(string rootPath)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var defaultImagePath = Path.Combine(rootPath, "image.iso");
            if (File.Exists(defaultImagePath))
            {
                return defaultImagePath;
            }

            return null;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider?.GetService<ILogErrors>()?.LogErrorAsync(ex, $"Error finding image.iso in path: {rootPath}");
            return null;
        }
    }
}