using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.SearchOrchestrator;

/// <summary>
/// Orchestrates search operations by validating queries and preparing the game cache for results.
/// </summary>
public class SearchOrchestratorService : ISearchOrchestratorService
{
    private readonly IGameCacheService _gameCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchOrchestratorService"/> class.
    /// </summary>
    public SearchOrchestratorService(IGameCacheService gameCacheService)
    {
        _gameCacheService = gameCacheService;
    }

    /// <summary>
    /// Validates the search query and selected system, then clears previous search results from the cache.
    /// </summary>
    public async Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string selectedSystem, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(selectedSystem) || string.IsNullOrWhiteSpace(searchQuery))
        {
            return SearchValidationResult.Failure();
        }

        await _gameCacheService.SetSearchResultsAsync([], cancellationToken);

        return SearchValidationResult.Success(searchQuery.Trim());
    }
}
