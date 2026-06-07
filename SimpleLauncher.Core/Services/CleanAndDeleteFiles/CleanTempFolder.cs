namespace SimpleLauncher.Core.Services.CleanAndDeleteFiles;

public static class CleanTempFolder
{
    /// <summary>
    /// Cleans up the specified temporary directory by removing all its contents and the directory itself.
    /// </summary>
    /// <param name="directoryPath">The path to the temporary directory to be cleaned up.</param>
    public static async Task CleanupTempDirectoryAsync(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath)) return;

        try
        {
            await Task.Run(() => Directory.Delete(directoryPath, true));
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
    public static async Task CleanupPartialExtractionAsync(string directoryPath)
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
                await DeleteFiles.TryDeleteFileAsync(trackingFile);
            }

            // Delete all files in the directory
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                await DeleteFiles.TryDeleteFileAsync(file);
            }

            // Recursively delete subdirectories
            foreach (var subDir in Directory.GetDirectories(directoryPath))
            {
                await Task.Run(() => Directory.Delete(subDir, true));
            }
        }
        catch (Exception)
        {
            // Ignore - this is cleanup code
        }
    }
}