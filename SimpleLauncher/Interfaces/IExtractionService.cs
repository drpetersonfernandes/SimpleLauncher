#nullable enable

namespace SimpleLauncher.Interfaces;

public interface IExtractionService
{
    Task<(string? gameFilePath, string? tempDirectoryPath)> ExtractToTempAndGetLaunchFileAsync(string archivePath, List<string> fileFormatsToLaunch);
    Task<bool> ExtractToFolderAsync(string archivePath, string destinationFolder);
}
