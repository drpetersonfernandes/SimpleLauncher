namespace SimpleLauncher.Interfaces;

public interface IGameCacheService
{
    string SelectedSystem { get; }

    Task<IList<string>> GetAllGamesAsync(CancellationToken ct);
    Task<IList<string>> GetSearchResultsAsync(CancellationToken ct);
    Task<bool> IsCachePopulatedForSystemAsync(string systemName, CancellationToken ct);

    Task SetAllGamesAsync(IList<string> games, string systemName, CancellationToken ct);
    Task SetSearchResultsAsync(IList<string> results, CancellationToken ct);
    Task PopulateFromDiskAsync(Services.SystemManager.SystemManagerService config, IGetListOfFilesService fileService, CancellationToken ct);

    Task<(List<string> allGames, List<string> searchResults)> GetResortSourceAsync(
        bool hasActiveFilter, CancellationToken ct);

    Task InvalidateAsync(CancellationToken ct);
    void ClearSync();
}
