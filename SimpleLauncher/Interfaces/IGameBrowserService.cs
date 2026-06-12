using SimpleLauncher.Models;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.SearchOrchestrator;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IGameBrowserService
{
    void Initialize(IGameFileLoadingHost loadingHost, ISystemSelectionHost selectionHost, IGameItemRenderHost renderHost);

    // System management
    void LoadOrReloadSystemManager();
    Task DisplaySystemSelectionScreenAsync(CancellationToken ct = default);
    Task SystemComboBoxSelectionChangedAsync(CancellationToken ct = default);
    List<SystemManager> SystemManagers { get; }

    // Game loading
    Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken ct = default);
    Task InvalidateGameFileCachesAsync(CancellationToken ct = default);

    // Search
    Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string selectedSystem, CancellationToken ct);

    // Rendering
    void ReloadFactories(List<SystemManager> systemManagers, List<MameManager> machines);
    Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManager systemManager, CancellationToken ct);
    void HandleSelectionChangedAsync(GameListViewItem selectedItem);
    Task HandleDoubleClickAsync(GameListViewItem selectedItem);
    void ClearRenderedItems();
    void SetGameButtonsEnabled(bool isEnabled);
    int ImageHeight { get; set; }

    // Scanning
    Task ScanForStoreGamesAsync();
    bool WasNewSystemCreated { get; }

    // MAME data
    IReadOnlyList<MameManager> Machines { get; }
    Dictionary<string, string> MameLookup { get; }

    // File watcher
    void OnGameFilesChangedAsync(string systemName);

    // Cleanup
    void ClearCache();
}
