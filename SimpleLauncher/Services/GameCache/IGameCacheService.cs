using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Services.GameCache;

public interface IGameCacheService
{
    string SelectedSystem { get; }

    Task<List<string>> GetAllGamesAsync(CancellationToken ct);
    Task<List<string>> GetSearchResultsAsync(CancellationToken ct);
    Task<bool> IsCachePopulatedForSystemAsync(string systemName, CancellationToken ct);

    Task SetAllGamesAsync(List<string> games, string systemName, CancellationToken ct);
    Task SetSearchResultsAsync(List<string> results, CancellationToken ct);
    Task PopulateFromDiskAsync(SystemManager.SystemManager config, IGetListOfFilesService fileService, CancellationToken ct);

    Task<(List<string> allGames, List<string> searchResults)> GetResortSourceAsync(
        bool hasActiveFilter, CancellationToken ct);

    Task InvalidateAsync(CancellationToken ct);
    void ClearSync();
}