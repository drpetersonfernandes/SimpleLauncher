using SimpleLauncher.Services.GameCache;

namespace SimpleLauncher.Services.SearchOrchestrator;

public class SearchOrchestratorService : ISearchOrchestratorService
{
    private readonly IGameCacheService _gameCacheService;

    public SearchOrchestratorService(IGameCacheService gameCacheService)
    {
        _gameCacheService = gameCacheService;
    }

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
