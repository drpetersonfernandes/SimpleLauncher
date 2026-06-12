using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// No-op implementation of <see cref="IGetListOfFilesService"/> for unit tests.
/// Always returns an empty file list.
/// </summary>
public class NoOpGetListOfFiles : IGetListOfFilesService
{
    /// <summary>
    /// Returns an empty list regardless of the provided parameters.
    /// </summary>
    /// <param name="directoryPath">The directory path to search.</param>
    /// <param name="fileExtensions">The file extensions to filter by.</param>
    /// <param name="disableRecursiveSearch">Whether to disable recursive search.</param>
    /// <param name="groupByFolder">Whether to group results by folder.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty <see cref="List{T}"/>.</returns>
    public Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, bool disableRecursiveSearch, bool groupByFolder, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<string>());
    }
}
