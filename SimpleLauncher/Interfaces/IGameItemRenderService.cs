using SimpleLauncher.Models;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to render game items in the UI, handle selection, and manage display state.
/// </summary>
public interface IGameItemRenderService
{
    /// <summary>
    /// Initializes the render service with the specified host.
    /// </summary>
    /// <param name="host">The host providing UI elements and collections.</param>
    void Initialize(IGameItemRenderHost host);

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
    /// Handles the selection change of a game list item, such as updating the preview.
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
}
