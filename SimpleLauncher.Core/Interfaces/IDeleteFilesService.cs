namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Provides methods to safely attempt deletion of files and directories.
/// </summary>
public interface IDeleteFilesService
{
    /// <summary>
    ///     Attempts to delete the specified file, handling any errors gracefully.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    void TryDeleteFile(string filePath);

    /// <summary>
    ///     Asynchronously attempts to delete the specified file, handling any errors gracefully.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    Task TryDeleteFileAsync(string filePath);

    /// <summary>
    ///     Attempts to delete the specified directory, handling any errors gracefully.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to delete.</param>
    void TryDeleteDirectory(string directoryPath);
}