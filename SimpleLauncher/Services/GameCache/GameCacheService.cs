using System.IO;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DebugAndBugReport;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameCache;

public class GameCacheService : IGameCacheService, IDisposable
{
    // ReSharper disable once NotAccessedField.Local
    private readonly ILogErrors _logErrors;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<string> _allGamesForCurrentSystem = [];
    private List<string> _currentSearchResults = [];

    public string SelectedSystem { get; private set; } = string.Empty;

    public GameCacheService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public async Task<List<string>> GetAllGamesAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return [.._allGamesForCurrentSystem];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<string>> GetSearchResultsAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return [.._currentSearchResults];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> IsCachePopulatedForSystemAsync(string systemName, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _allGamesForCurrentSystem.Count > 0 &&
                   string.Equals(SelectedSystem, systemName, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAllGamesAsync(List<string> games, string systemName, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _allGamesForCurrentSystem = new List<string>(games);
            SelectedSystem = systemName;
            DebugLogger.Log($"[GameCacheService] SetAllGames for '{systemName}'. Count: {games.Count}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetSearchResultsAsync(List<string> results, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _currentSearchResults = new List<string>(results);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<(List<string> allGames, List<string> searchResults)> GetResortSourceAsync(
        bool hasActiveFilter, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var source = hasActiveFilter
                ? new List<string>(_currentSearchResults)
                : new List<string>(_allGamesForCurrentSystem);
            return (source, [.._currentSearchResults]);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task PopulateFromDiskAsync(SystemManager.SystemManager config, IGetListOfFilesService fileService, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_allGamesForCurrentSystem.Count > 0 &&
                string.Equals(SelectedSystem, config.SystemName, StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"[GameCacheService] Using cached list for '{config.SystemName}'. Count: {_allGamesForCurrentSystem.Count}");
                return;
            }

            DebugLogger.Log($"[GameCacheService] Populating from disk for '{config.SystemName}'.");
            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var folder in config.SystemFolders)
            {
                ct.ThrowIfCancellationRequested();

                var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                if (string.IsNullOrEmpty(resolvedPath) ||
                    !Directory.Exists(resolvedPath) ||
                    config.FileFormatsToSearch == null) continue;

                var filesInFolder = await fileService.GetFilesAsync(resolvedPath, config.FileFormatsToSearch, config.DisableRecursiveSearch, config.GroupByFolder, ct);
                foreach (var file in filesInFolder)
                {
                    uniqueFiles.TryAdd(Path.GetFileName(file), file);
                }
            }

            _allGamesForCurrentSystem = uniqueFiles.Values.ToList();
            SelectedSystem = config.SystemName;
            DebugLogger.Log($"[GameCacheService] Populated {_allGamesForCurrentSystem.Count} games.");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task InvalidateAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _allGamesForCurrentSystem.Clear();
            _currentSearchResults.Clear();
            DebugLogger.Log("[GameCacheService] All game file caches invalidated.");
        }
        finally
        {
            _lock.Release();
        }
    }

    public void ClearSync()
    {
        try
        {
            if (_lock.Wait(100))
            {
                try
                {
                    _currentSearchResults?.Clear();
                    _allGamesForCurrentSystem?.Clear();
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Semaphore was disposed, ignore
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}