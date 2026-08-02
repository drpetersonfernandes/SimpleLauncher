using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.MameManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Orchestrates game browsing, including system management, game file loading, searching, rendering, and scanning.
/// </summary>
public interface IGameBrowserService
{
    /// <summary>
    /// Initializes the game browser with the required host references.
    /// </summary>
    /// <param name="loadingHost">The host providing game file loading UI elements.</param>
    /// <param name="selectionHost">The host providing system selection UI elements.</param>
    /// <param name="renderHost">The host providing game item rendering UI elements.</param>
    void Initialize(IGameFileLoadingHost loadingHost, ISystemSelectionHost selectionHost, IGameItemRenderHost renderHost);

    /// <summary>
    /// Loads or reloads the system manager configurations.
    /// </summary>
    void LoadOrReloadSystemManager();

    /// <summary>
    /// Asynchronously loads or reloads the system manager configurations.
    /// </summary>
    Task LoadOrReloadSystemManagerAsync();

    /// <summary>
    /// Asynchronously displays the system selection screen.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task DisplaySystemSelectionScreenAsync(CancellationToken ct = default);

    /// <summary>
    /// Handles changes to the system combo box selection.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SystemComboBoxSelectionChangedAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the list of available system managers.
    /// </summary>
    IList<SystemManagerService> SystemManagers { get; }

    /// <summary>
    /// Asynchronously loads game files for the current system, optionally filtered by starting letter or search query.
    /// </summary>
    /// <param name="startLetter">An optional letter to filter games by.</param>
    /// <param name="searchQuery">An optional search query to filter games.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task LoadGameFilesAsync(string? startLetter = null, string? searchQuery = null, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously invalidates all game file caches, forcing a refresh on the next load.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task InvalidateGameFileCachesAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously validates the search query and prepares the system for a search operation.
    /// </summary>
    /// <param name="searchQuery">The search query to validate.</param>
    /// <param name="selectedSystem">The currently selected system name, or null.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The search validation result.</returns>
    Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string? selectedSystem, CancellationToken ct);

    /// <summary>
    /// Reloads the render factories for the specified system managers and MAME machines.
    /// </summary>
    /// <param name="systemManagers">The list of system managers.</param>
    /// <param name="machines">The list of MAME machine services.</param>
    void ReloadFactories(IList<SystemManagerService> systemManagers, IList<MameManagerService> machines);

    /// <summary>
    /// Asynchronously renders game items in the UI from the specified file list.
    /// </summary>
    /// <param name="files">The list of game file paths to render.</param>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="systemManager">The system manager service.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManagerService systemManager, CancellationToken ct);

    /// <summary>
    /// Handles the selection change of a game list item.
    /// </summary>
    /// <param name="selectedItem">The selected game list view item.</param>
    Task HandleSelectionChangedAsync(GameListViewItem selectedItem);

    /// <summary>
    /// Handles a double-click on a game list item, typically launching the game.
    /// </summary>
    /// <param name="selectedItem">The double-clicked game list view item.</param>
    Task HandleDoubleClickAsync(GameListViewItem selectedItem);

    /// <summary>
    /// Clears all rendered game items from the UI.
    /// </summary>
    void ClearRenderedItems();

    /// <summary>
    /// Enables or disables game-related buttons in the UI.
    /// </summary>
    /// <param name="isEnabled">True to enable the buttons; false to disable them.</param>
    void SetGameButtonsEnabled(bool isEnabled);

    /// <summary>
    /// Gets or sets the height of game item images in pixels.
    /// </summary>
    int ImageHeight { get; set; }

    /// <summary>
    /// Asynchronously scans for Store games installed on the system.
    /// </summary>
    Task ScanForStoreGamesAsync();

    /// <summary>
    /// Gets a value indicating whether a new system was created during the last scan.
    /// </summary>
    bool WasNewSystemCreated { get; }

    /// <summary>
    /// Gets the read-only list of MAME machine services.
    /// </summary>
    IReadOnlyList<MameManagerService> Machines { get; }

    /// <summary>
    /// Gets the MAME lookup dictionary mapping ROM names to descriptions.
    /// </summary>
    IDictionary<string, string> MameLookup { get; }

    /// <summary>
    /// Handles a file system change event for the specified system's game files.
    /// </summary>
    /// <param name="systemName">The name of the system whose files changed.</param>
    void OnGameFilesChangedAsync(string systemName);

    /// <summary>
    /// Clears all cached game data.
    /// </summary>
    void ClearCache();
}
