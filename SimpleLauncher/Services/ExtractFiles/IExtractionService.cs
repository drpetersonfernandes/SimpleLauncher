#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleLauncher.Services.ExtractFiles;

public interface IExtractionService
{
    Task<(string? gameFilePath, string? tempDirectoryPath)> ExtractToTempAndGetLaunchFileAsync(string archivePath, List<string> fileFormatsToLaunch);
    Task<bool> ExtractToFolderAsync(string archivePath, string destinationFolder);
}