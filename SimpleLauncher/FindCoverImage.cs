using System;
using System.IO;

namespace SimpleLauncher;

public static class FindCoverImage
{
    private static readonly string GlobalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");
    
    public static string FindCoverImagePath(string fileNameWithoutExtension, string systemName, SystemConfig systemConfig)
    {
        var applicationPath = AppDomain.CurrentDomain.BaseDirectory;
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];
    
        string systemImagePath;
        if (string.IsNullOrEmpty(systemConfig?.SystemImageFolder))
        {
            systemImagePath = Path.Combine(applicationPath, "images", systemName);
        }
        else
        {
            systemImagePath = Path.IsPathRooted(systemConfig.SystemImageFolder)
                ? systemConfig.SystemImageFolder // If already absolute
                : Path.Combine(applicationPath, systemConfig.SystemImageFolder); // Make it absolute
        }
        foreach (var ext in imageExtensions)
        {
            var imagePath = Path.Combine(systemImagePath, $"{fileNameWithoutExtension}{ext}");
            if (File.Exists(imagePath))
                return imagePath;
        }

        var defaultSystemImagePath = Path.Combine(systemImagePath, "default.png");
        if (File.Exists(defaultSystemImagePath))
        {
            return defaultSystemImagePath;
        }
        else
        {
            return GlobalDefaultImagePath;
        }
    }
}