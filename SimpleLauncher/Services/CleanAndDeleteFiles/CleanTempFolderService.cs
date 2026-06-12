namespace SimpleLauncher.Services.CleanAndDeleteFiles;

using Interfaces;

/// <summary>
/// Provides operations to clean up temporary directories used during file extraction.
/// </summary>
public class CleanTempFolderService : ICleanTempFolderService
{
    private readonly IDeleteFilesService _deleteFilesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanTempFolderService"/> class.
    /// </summary>
    /// <param name="deleteFilesService">The service used to delete individual files.</param>
    public CleanTempFolderService(IDeleteFilesService deleteFilesService)
    {
        _deleteFilesService = deleteFilesService;
    }

    /// <summary>
    /// Deletes the specified temporary directory and all its contents.
    /// </summary>
    /// <param name="directoryPath">The path of the temporary directory to delete.</param>
    public async Task CleanupTempDirectoryAsync(string directoryPath)
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
    /// Cleans up a partially extracted directory by removing the tracking file, all files, and subdirectories.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to clean up.</param>
    public async Task CleanupPartialExtractionAsync(string directoryPath)
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
                await _deleteFilesService.TryDeleteFileAsync(trackingFile);
            }

            // Delete all files in the directory
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                await _deleteFilesService.TryDeleteFileAsync(file);
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
