using SimpleLauncher.Models;
using SimpleLauncher.Services.GameScan;
using SimpleLauncher.Services.SearchOrchestrator;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameBrowser;

/// <summary>
/// Facade service that coordinates game browsing operations including system selection,
/// game file loading, rendering, searching, scanning, and caching.
/// </summary>
public class GameBrowserService : IGameBrowserService
{
    private readonly IGameFileLoadingOrchestrator _gameFileLoadingOrchestrator;
    private readonly ISystemSelectionOrchestrator _systemSelectionOrchestrator;
    private readonly IGameItemRenderService _gameItemRenderService;
    private readonly IGameCacheService _gameCacheService;
    private readonly IMameDataService _mameDataService;
    private readonly GameScannerService _gameScannerService;
    private readonly ISearchOrchestratorService _searchOrchestratorService;

    /// <summary>
    /// Initializes a new instance of <see cref="GameBrowserService"/> with the required orchestrators and services.
    /// </summary>
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

    /// <summary>
    /// Initializes all sub-orchestrators with their respective host implementations.
    /// </summary>
    public void Initialize(IGameFileLoadingHost loadingHost, ISystemSelectionHost selectionHost, IGameItemRenderHost renderHost)
    {
        _gameFileLoadingOrchestrator.Initialize(loadingHost);
        _systemSelectionOrchestrator.Initialize(selectionHost);
        _gameItemRenderService.Initialize(renderHost);
    }

    /// <summary>
    /// Loads or reloads the system manager configuration.
    /// </summary>
    public void LoadOrReloadSystemManager()
    {
        _systemSelectionOrchestrator.LoadOrReloadSystemManager();
    }

    /// <summary>
    /// Displays the system selection screen when no system is currently selected.
    /// </summary>
    public Task DisplaySystemSelectionScreenAsync(CancellationToken ct = default)
    {
        return _systemSelectionOrchestrator.DisplaySystemSelectionScreenAsync(ct);
    }

    /// <summary>
    /// Handles the system combo box selection change event by reloading game files.
    /// </summary>
    public Task SystemComboBoxSelectionChangedAsync(CancellationToken ct = default)
    {
        return _systemSelectionOrchestrator.SystemComboBoxSelectionChangedAsync(ct);
    }

    /// <summary>
    /// Gets or sets the list of available system managers.
    /// </summary>
    public List<SystemManager.SystemManager> SystemManagers
    {
        get;
        set;
    }

    /// <summary>
    /// Loads game files for the selected system, optionally filtered by start letter or search query.
    /// </summary>
    public Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken ct = default)
    {
        return _gameFileLoadingOrchestrator.LoadGameFilesAsync(startLetter, searchQuery, ct);
    }

    /// <summary>
    /// Invalidates all cached game file lists, forcing a reload on next access.
    /// </summary>
    public Task InvalidateGameFileCachesAsync(CancellationToken ct = default)
    {
        return _gameFileLoadingOrchestrator.InvalidateGameFileCachesAsync(ct);
    }

    /// <summary>
    /// Validates a search query against the selected system and prepares it for execution.
    /// </summary>
    public Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string selectedSystem, CancellationToken ct)
    {
        return _searchOrchestratorService.ValidateAndPrepareAsync(searchQuery, selectedSystem, ct);
    }

    /// <summary>
    /// Rebuilds the game button and list item factories with updated system and MAME machine data.
    /// </summary>
    public void ReloadFactories(List<SystemManager.SystemManager> systemManagers, List<MameManager.MameManager> machines)
    {
        _gameItemRenderService.ReloadFactories(systemManagers, machines);
    }

    /// <summary>
    /// Renders game items for the specified file list in the current view mode (grid or list).
    /// </summary>
    public Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManager.SystemManager systemManager, CancellationToken ct)
    {
        return _gameItemRenderService.RenderGameItemsAsync(files, systemName, systemManager, ct);
    }

    /// <summary>
    /// Handles a selection change event by updating the preview image for the selected item.
    /// </summary>
    public void HandleSelectionChangedAsync(GameListViewItem selectedItem)
    {
        _gameItemRenderService.HandleSelectionChangedAsync(selectedItem);
    }

    /// <summary>
    /// Handles a double-click event on a game item by launching the selected game.
    /// </summary>
    public Task HandleDoubleClickAsync(GameListViewItem selectedItem)
    {
        return _gameItemRenderService.HandleDoubleClickAsync(selectedItem);
    }

    /// <summary>
    /// Clears all rendered game items from the UI.
    /// </summary>
    public void ClearRenderedItems()
    {
        _gameItemRenderService.ClearRenderedItems();
    }

    /// <summary>
    /// Enables or disables all game item buttons in the UI.
    /// </summary>
    public void SetGameButtonsEnabled(bool isEnabled)
    {
        _gameItemRenderService.SetGameButtonsEnabled(isEnabled);
    }

    /// <summary>
    /// Gets or sets the height (in pixels) of game item thumbnail images.
    /// </summary>
    public int ImageHeight
    {
        get => _gameItemRenderService.ImageHeight;
        set => _gameItemRenderService.ImageHeight = value;
    }

    /// <summary>
    /// Scans store/installed game locations and registers any newly discovered systems.
    /// </summary>
    public Task ScanForStoreGamesAsync()
    {
        return _gameScannerService.ScanForStoreGamesAsync();
    }

    /// <summary>
    /// Gets a value indicating whether a new system configuration was created during the last scan.
    /// </summary>
    public bool WasNewSystemCreated => _gameScannerService.WasNewSystemCreated;

    /// <summary>
    /// Gets the list of loaded MAME machine definitions.
    /// </summary>
    public IReadOnlyList<MameManager.MameManager> Machines => _mameDataService.Machines;
    /// <summary>
    /// Gets the MAME filename-to-description lookup dictionary.
    /// </summary>
    public Dictionary<string, string> MameLookup => _mameDataService.Lookup;

    /// <summary>
    /// Notifies the system that game files have changed for the specified system, triggering a reload.
    /// </summary>
    public void OnGameFilesChangedAsync(string systemName)
    {
        _gameFileLoadingOrchestrator.OnGameFilesChangedAsync(systemName);
    }

    /// <summary>
    /// Clears all cached game file data synchronously.
    /// </summary>
    public void ClearCache()
    {
        _gameCacheService.ClearSync();
    }
}
