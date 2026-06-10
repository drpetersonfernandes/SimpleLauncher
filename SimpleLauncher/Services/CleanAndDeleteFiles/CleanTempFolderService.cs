namespace SimpleLauncher.Services.CleanAndDeleteFiles;

public class CleanTempFolderService : ICleanTempFolderService
{
    private readonly IDeleteFilesService _deleteFilesService;

    public CleanTempFolderService(IDeleteFilesService deleteFilesService)
    {
        _deleteFilesService = deleteFilesService;
    }

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
