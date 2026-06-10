using SimpleLauncher.Models;
using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Services.GameFileLoadingOrchestrator;
using SimpleLauncher.Services.GameItemRender;
using SimpleLauncher.Services.GameScan;
using SimpleLauncher.Services.MameData;
using SimpleLauncher.Services.SearchOrchestrator;
using SimpleLauncher.Services.SystemSelectionOrchestrator;

namespace SimpleLauncher.Services.GameBrowser;

public class GameBrowserService : IGameBrowserService
{
    private readonly IGameFileLoadingOrchestrator _gameFileLoadingOrchestrator;
    private readonly ISystemSelectionOrchestrator _systemSelectionOrchestrator;
    private readonly IGameItemRenderService _gameItemRenderService;
    private readonly IGameCacheService _gameCacheService;
    private readonly IMameDataService _mameDataService;
    private readonly GameScannerService _gameScannerService;
    private readonly ISearchOrchestratorService _searchOrchestratorService;

    public GameBrowserService(
        IGameFileLoadingOrchestrator gameFileLoadingOrchestrator,
        ISystemSelectionOrchestrator systemSelectionOrchestrator,
        IGameItemRenderService gameItemRenderService,
        IGameCacheService gameCacheService,
        IMameDataService mameDataService,
        GameScannerService gameScannerService,
        ISearchOrchestratorService searchOrchestratorService)
    {
        _gameFileLoadingOrchestrator = gameFileLoadingOrchestrator;
        _systemSelectionOrchestrator = systemSelectionOrchestrator;
        _gameItemRenderService = gameItemRenderService;
        _gameCacheService = gameCacheService;
        _mameDataService = mameDataService;
        _gameScannerService = gameScannerService;
        _searchOrchestratorService = searchOrchestratorService;
    }

    public void Initialize(IGameFileLoadingHost loadingHost, ISystemSelectionHost selectionHost, IGameItemRenderHost renderHost)
    {
        _gameFileLoadingOrchestrator.Initialize(loadingHost);
        _systemSelectionOrchestrator.Initialize(selectionHost);
        _gameItemRenderService.Initialize(renderHost);
    }

    // System management
    public void LoadOrReloadSystemManager()
    {
        _systemSelectionOrchestrator.LoadOrReloadSystemManager();
    }

    public Task DisplaySystemSelectionScreenAsync(CancellationToken ct = default)
    {
        return _systemSelectionOrchestrator.DisplaySystemSelectionScreenAsync(ct);
    }

    public Task SystemComboBoxSelectionChangedAsync(CancellationToken ct = default)
    {
        return _systemSelectionOrchestrator.SystemComboBoxSelectionChangedAsync(ct);
    }

    public List<SystemManager.SystemManager> SystemManagers
    {
        get;
        set;
    }

    // Game loading
    public Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken ct = default)
    {
        return _gameFileLoadingOrchestrator.LoadGameFilesAsync(startLetter, searchQuery, ct);
    }

    public Task InvalidateGameFileCachesAsync(CancellationToken ct = default)
    {
        return _gameFileLoadingOrchestrator.InvalidateGameFileCachesAsync(ct);
    }

    // Search
    public Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string selectedSystem, CancellationToken ct)
    {
        return _searchOrchestratorService.ValidateAndPrepareAsync(searchQuery, selectedSystem, ct);
    }

    // Rendering
    public void ReloadFactories(List<SystemManager.SystemManager> systemManagers, List<MameManager.MameManager> machines)
    {
        _gameItemRenderService.ReloadFactories(systemManagers, machines);
    }

    public Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManager.SystemManager systemManager, CancellationToken ct)
    {
        return _gameItemRenderService.RenderGameItemsAsync(files, systemName, systemManager, ct);
    }

    public void HandleSelectionChangedAsync(GameListViewItem selectedItem)
    {
        _gameItemRenderService.HandleSelectionChangedAsync(selectedItem);
    }

    public Task HandleDoubleClickAsync(GameListViewItem selectedItem)
    {
        return _gameItemRenderService.HandleDoubleClickAsync(selectedItem);
    }

    public void ClearRenderedItems()
    {
        _gameItemRenderService.ClearRenderedItems();
    }

    public void SetGameButtonsEnabled(bool isEnabled)
    {
        _gameItemRenderService.SetGameButtonsEnabled(isEnabled);
    }

    public int ImageHeight
    {
        get => _gameItemRenderService.ImageHeight;
        set => _gameItemRenderService.ImageHeight = value;
    }

    // Scanning
    public Task ScanForStoreGamesAsync()
    {
        return _gameScannerService.ScanForStoreGamesAsync();
    }

    public bool WasNewSystemCreated => _gameScannerService.WasNewSystemCreated;

    // MAME data
    public IReadOnlyList<MameManager.MameManager> Machines => _mameDataService.Machines;
    public Dictionary<string, string> MameLookup => _mameDataService.Lookup;

    // File watcher
    public void OnGameFilesChanged(string systemName)
    {
        _gameFileLoadingOrchestrator.OnGameFilesChanged(systemName);
    }

    // Cleanup
    public void ClearCache()
    {
        _gameCacheService.ClearSync();
    }
}
