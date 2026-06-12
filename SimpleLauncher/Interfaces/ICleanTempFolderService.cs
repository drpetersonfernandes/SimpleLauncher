namespace SimpleLauncher.Interfaces;

public interface ICleanTempFolderService
{
    Task CleanupTempDirectoryAsync(string directoryPath);
    Task CleanupPartialExtractionAsync(string directoryPath);
}
