namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to extract archive files and locate launchable game files within them.
/// </summary>
public interface IExtractionService
{
    /// <summary>
    /// Asynchronously extracts an archive to a temporary directory and returns the path to the first matching launch file.
    /// </summary>
    /// <param name="archivePath">The path to the archive file to extract.</param>
    /// <param name="fileFormatsToLaunch">A list of file extensions to search for within the archive.</param>
    /// <returns>A tuple containing the path to the game file and the temporary directory path, or null values if no matching file was found.</returns>
    Task<(string? gameFilePath, string? tempDirectoryPath)> ExtractToTempAndGetLaunchFileAsync(string archivePath, IList<string> fileFormatsToLaunch);

    /// <summary>
    /// Asynchronously extracts an archive to the specified destination folder.
    /// </summary>
    /// <param name="archivePath">The path to the archive file to extract.</param>
    /// <param name="destinationFolder">The path to the folder where files will be extracted.</param>
    /// <returns>True if extraction succeeded; otherwise, false.</returns>
    Task<bool> ExtractToFolderAsync(string archivePath, string destinationFolder);
}
