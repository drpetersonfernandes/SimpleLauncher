using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides UI elements and collections required by the game item rendering service.
/// </summary>
public interface IGameItemRenderHost
{
    /// <summary>
    ///     Gets the WPF dispatcher for marshaling calls to the UI thread.
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    ///     Gets the scroll viewer containing the game file grid.
    /// </summary>
    ScrollViewer Scroller { get; }

    /// <summary>
    ///     Gets the data grid used to display game files in list view mode.
    /// </summary>
    DataGrid GameDataGrid { get; }

    /// <summary>
    ///     Gets the emulator selection combo box.
    /// </summary>
    ComboBox EmulatorComboBox { get; }

    /// <summary>
    ///     Gets the system selection combo box.
    /// </summary>
    ComboBox SystemComboBox { get; }

    /// <summary>
    ///     Gets the WrapPanel used to display game file items.
    /// </summary>
    WrapPanel GameFileGrid { get; }

    /// <summary>
    ///     Gets the observable collection of game list view items for data binding.
    /// </summary>
    ObservableCollection<GameListViewItem> GameListItems { get; }

    /// <summary>
    ///     Gets the main application window instance.
    /// </summary>
    MainWindow MainWindow { get; }
}