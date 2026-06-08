namespace SimpleLauncher.Core.Services.CleanAndDeleteFiles;

public interface ICleanTempFolderService
{
    Task CleanupTempDirectoryAsync(string directoryPath);
    Task CleanupPartialExtractionAsync(string directoryPath);
}
