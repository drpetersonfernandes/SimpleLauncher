namespace SimpleLauncher.Services.CleanAndDeleteFiles;

public interface ICleanTempFolderService
{
    Task CleanupTempDirectoryAsync(string directoryPath);
    Task CleanupPartialExtractionAsync(string directoryPath);
}
