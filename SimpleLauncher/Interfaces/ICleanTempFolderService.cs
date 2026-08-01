namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to clean up temporary directories and partial extraction artifacts.
/// </summary>
public interface ICleanTempFolderService
{
    /// <summary>
    /// Cleans up the specified temporary directory by removing its contents.
    /// </summary>
    /// <param name="directoryPath">The path of the temporary directory to clean up.</param>
    Task CleanupTempDirectoryAsync(string directoryPath);

    /// <summary>
    /// Cleans up partial extraction artifacts left behind by an interrupted extraction process.
    /// </summary>
    /// <param name="directoryPath">The path of the directory containing partial extraction artifacts.</param>
    Task CleanupPartialExtractionAsync(string directoryPath);
}
