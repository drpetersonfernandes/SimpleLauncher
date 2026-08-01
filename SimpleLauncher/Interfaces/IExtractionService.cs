namespace SimpleLauncher.Interfaces;

public interface IExtractionService
{
    Task<(string? gameFilePath, string? tempDirectoryPath)> ExtractToTempAndGetLaunchFileAsync(string archivePath, IList<string> fileFormatsToLaunch);
    Task<bool> ExtractToFolderAsync(string archivePath, string destinationFolder);
}
