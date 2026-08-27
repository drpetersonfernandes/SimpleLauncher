namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to retrieve lists of files from a directory based on extensions and search options.
/// </summary>
public interface IGetListOfFilesService
{
    /// <summary>
    /// Asynchronously retrieves files from the specified directory matching the given extensions.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to search.</param>
    /// <param name="fileExtensions">The list of file extensions to include (e.g., ".iso", ".bin").</param>
    /// <param name="disableRecursiveSearch">If true, only searches the top-level directory.</param>
    /// <param name="groupByFolder">If true, groups results by containing folder.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of matching file paths.</returns>
    Task<IList<string>> GetFilesAsync(string directoryPath, IList<string> fileExtensions, bool disableRecursiveSearch,
        bool groupByFolder, CancellationToken cancellationToken = default);
}