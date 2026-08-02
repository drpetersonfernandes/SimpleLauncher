using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides access to the UI elements controlled by the UI orchestrator.
/// </summary>
public interface IUiOrchestratorHost
{
    /// <summary>
    /// Gets the dispatcher for the host window, used to marshal calls to the UI thread.
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the main content scroller.
    /// </summary>
    ScrollViewer Scroller { get; }

    /// <summary>
    /// Gets the preview image control.
    /// </summary>
    Image PreviewImage { get; }

    /// <summary>
    /// Gets the wrap panel that displays the game file buttons.
    /// </summary>
    WrapPanel GameFileGrid { get; }

    /// <summary>
    /// Gets the list view preview area grid.
    /// </summary>
    Grid ListViewPreviewArea { get; }

    /// <summary>
    /// Gets the frame used to display child pages.
    /// </summary>
    Frame PageContentFrame { get; }

    /// <summary>
    /// Gets the main game content grid.
    /// </summary>
    Grid MainGameContent { get; }

    /// <summary>
    /// Gets the main content grid of the window.
    /// </summary>
    Grid MainContentGrid { get; }

    /// <summary>
    /// Gets the total files label.
    /// </summary>
    Label TotalFilesLabel { get; }

    /// <summary>
    /// Gets the previous page navigation button.
    /// </summary>
    Button PrevPageButton2 { get; }

    /// <summary>
    /// Gets the next page navigation button.
    /// </summary>
    Button NextPageButton2 { get; }

    /// <summary>
    /// Gets the loading overlay element.
    /// </summary>
    UIElement LoadingOverlay { get; }

    /// <summary>
    /// Gets the sort order toggle button.
    /// </summary>
    Button SortOrderToggleButton { get; }

    /// <summary>
    /// Gets the search text box.
    /// </summary>
    TextBox SearchTextBox { get; }

    /// <summary>
    /// Gets the system combo box.
    /// </summary>
    ComboBox SystemComboBox { get; }

    /// <summary>
    /// Gets the emulator combo box.
    /// </summary>
    ComboBox EmulatorComboBox { get; }

    /// <summary>
    /// Gets the collection of game list view items.
    /// </summary>
    ObservableCollection<GameListViewItem> GameListItems { get; }

    /// <summary>
    /// Gets whether games are currently being loaded.
    /// </summary>
    bool IsLoadingGames { get; }

    /// <summary>
    /// Sets the internal flag indicating whether games are currently being loaded.
    /// </summary>
    /// <param name="value">True if games are loading; otherwise, false.</param>
    void SetIsLoadingGamesInternal(bool value);

    /// <summary>
    /// Cancels the current operation and recreates the cancellation token.
    /// </summary>
    void CancelAndRecreateToken();

    /// <summary>
    /// Resets the UI to its default state asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetUiAsync();
}
