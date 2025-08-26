using System;
using System.IO;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class FindCoverImageForDeletion
{
    /// <summary>
    /// Attempts to find the cover image path for a given file and system.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The file name without its extension for which the cover image is being searched.</param>
    /// <param name="systemName">The name of the system associated with the file, used to determine the appropriate image directory.</param>
    /// <param name="systemManager">The system manager instance that provides settings like system image folder configurations.</param>
    /// <returns>
    /// A string representing the file path of the cover image if found.
    /// </returns>
    public static string FindCoverImagePath(string fileNameWithoutExtension, string systemName, SystemManager systemManager)
    {
        var applicationPath = AppDomain.CurrentDomain.BaseDirectory;
        var imageExtensions = GetImageExtensions.GetExtensions();

        string systemImageFolder;
        if (string.IsNullOrEmpty(systemManager.SystemImageFolder))
        {
            systemImageFolder = Path.Combine(applicationPath, "images", systemName ?? string.Empty);
        }
        else
        {
            systemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemManager.SystemImageFolder);
        }

        // Check if the resolved system image folder path is valid before proceeding
        if (!string.IsNullOrEmpty(systemImageFolder) && Directory.Exists(systemImageFolder))
        {
            // 1. Check for the exact match first within the resolved folder
            foreach (var ext in imageExtensions)
            {
                var imagePath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}{ext}");
                if (File.Exists(imagePath))
                    return imagePath; // Return the found path (which is already resolved)
            }
        }

        return null;
    }
}