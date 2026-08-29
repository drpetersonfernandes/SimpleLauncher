using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides caching services for game file lists and search results per system.
/// </summary>
public interface IGameCacheService
{
    /// <summary>
    ///     Gets the name of the currently selected system.
    /// </summary>
    string SelectedSystem { get; }

    /// <summary>
    ///     Asynchronously retrieves the full list of cached game file paths.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The list of all game file paths.</returns>
    Task<IList<string>> GetAllGamesAsync(CancellationToken ct);

    /// <summary>
    ///     Asynchronously retrieves the cached search result game file paths.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The list of search result game file paths.</returns>
    Task<IList<string>> GetSearchResultsAsync(CancellationToken ct);

    /// <summary>
    ///     Asynchronously determines whether the cache has been populated for the specified system.
    /// </summary>
    /// <param name="systemName">The name of the system to check.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>True if the cache is populated; otherwise, false.</returns>
    Task<bool> IsCachePopulatedForSystemAsync(string systemName, CancellationToken ct);

    /// <summary>
    ///     Asynchronously stores the full list of game file paths in the cache.
    /// </summary>
    /// <param name="games">The list of game file paths to cache.</param>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SetAllGamesAsync(IList<string> games, string systemName, CancellationToken ct);

    /// <summary>
    ///     Asynchronously stores the search result game file paths in the cache.
    /// </summary>
    /// <param name="results">The list of search result file paths to cache.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SetSearchResultsAsync(IList<string> results, CancellationToken ct);

    /// <summary>
    ///     Asynchronously populates the cache by scanning the file system.
    /// </summary>
    /// <param name="config">The system manager configuration to use.</param>
    /// <param name="fileService">The file listing service to scan directories.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task PopulateFromDiskAsync(SystemManagerService config, IGetListOfFilesService fileService,
        CancellationToken ct);

    /// <summary>
    ///     Asynchronously retrieves the source lists needed for re-sorting cached games.
    /// </summary>
    /// <param name="hasActiveFilter">Indicates whether a filter is currently active.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A tuple of all games and search results lists.</returns>
    Task<(List<string> allGames, List<string> searchResults)> GetResortSourceAsync(
        bool hasActiveFilter, CancellationToken ct);

    /// <summary>
    ///     Asynchronously invalidates the cache for the current system.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task InvalidateAsync(CancellationToken ct);

    /// <summary>
    ///     Synchronously clears all cached data.
    /// </summary>
    void ClearSync();
}