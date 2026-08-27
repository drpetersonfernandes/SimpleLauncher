namespace SimpleLauncher.Interfaces;

/// <summary>
/// Orchestrates the loading, caching, and refreshing of game files in the UI.
/// </summary>
public interface IGameFileLoadingOrchestrator
{
    /// <summary>
    /// Initializes the orchestrator with the specified host.
    /// </summary>
    /// <param name="host">The host providing UI elements and system information.</param>
    void Initialize(IGameFileLoadingHost host);

    /// <summary>
    /// Asynchronously loads game files, optionally filtered by starting letter or search query.
    /// </summary>
    /// <param name="startLetter">An optional letter to filter games by.</param>
    /// <param name="searchQuery">An optional search query to filter games.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LoadGameFilesAsync(string? startLetter = null, string? searchQuery = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously invalidates all game file caches, forcing a refresh on the next load.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task InvalidateGameFileCachesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a file system change event for the specified system's game files.
    /// </summary>
    /// <param name="systemName">The name of the system whose files changed.</param>
    void OnGameFilesChangedAsync(string systemName);
}