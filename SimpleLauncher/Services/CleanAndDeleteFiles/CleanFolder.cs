using System;
using System.IO;

namespace SimpleLauncher.Services.CleanAndDeleteFiles;

public static class CleanFolder
{
    /// <summary>
    /// Cleans up the specified temporary directory by removing all its contents and the directory itself.
    /// </summary>
    /// <param name="directoryPath">The path to the temporary directory to be cleaned up.</param>
    public static void CleanupTempDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath)) return;

        try
        {
            Directory.Delete(directoryPath, true);
        }
        catch (Exception)
        {
            // Ignore - this is cleanup code
        }
    }

    /// <summary>
    /// Cleans up partially extracted files from a failed extraction
    /// </summary>
    /// <param name="directoryPath">Directory containing partial extraction</param>
    public static void CleanupPartialExtraction(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            return;
        }

        try
        {
            // Delete the tracking file first
            var trackingFile = Path.Combine(directoryPath, ".extraction_in_progress");
            if (File.Exists(trackingFile))
            {
                DeleteFiles.TryDeleteFile(trackingFile);
            }

            // Delete all files in the directory
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                DeleteFiles.TryDeleteFile(file);
            }

            // Recursively delete subdirectories
            foreach (var subDir in Directory.GetDirectories(directoryPath))
            {
                Directory.Delete(subDir, true);
            }
        }
        catch (Exception)
        {
            // Ignore - this is cleanup code
        }
    }
}