using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides UI elements and methods for managing the game list display.
/// </summary>
public interface IGameListUiHost
{
    /// <summary>
    /// Gets the WPF dispatcher for marshaling calls to the UI thread.
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the scroll viewer containing the game file grid.
    /// </summary>
    ScrollViewer Scroller { get; }

    /// <summary>
    /// Gets the image control used to display game previews.
    /// </summary>
    Image PreviewImage { get; }

    /// <summary>
    /// Gets the WrapPanel used to display game file items.
    /// </summary>
    WrapPanel GameFileGrid { get; }

    /// <summary>
    /// Gets the grid area used for list view preview display.
    /// </summary>
    Grid ListViewPreviewArea { get; }

    /// <summary>
    /// Gets the observable collection of game list view items for data binding.
    /// </summary>
    ObservableCollection<GameListViewItem> GameListItems { get; }

    /// <summary>
    /// Shows or hides the game file grid.
    /// </summary>
    /// <param name="isVisible">True to show the grid; false to hide it.</param>
    void SetGameFileGridVisible(bool isVisible);

    /// <summary>
    /// Shows or hides the list view preview area.
    /// </summary>
    /// <param name="isVisible">True to show the preview area; false to hide it.</param>
    void SetListViewPreviewAreaVisible(bool isVisible);

    /// <summary>
    /// Shows or hides the pagination buttons.
    /// </summary>
    /// <param name="isVisible">True to show the buttons; false to hide them.</param>
    void SetPaginationButtonsVisible(bool isVisible);
}