using System.Collections.ObjectModel;
using System.Windows.Controls;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Partial MainWindow implementing <see cref="IGameItemRenderHost"/> for game item rendering operations.
/// </summary>
public partial class MainWindow : IGameItemRenderHost
{
    ScrollViewer IGameItemRenderHost.Scroller => Scroller;
    DataGrid IGameItemRenderHost.GameDataGrid => GameDataGrid;
    ComboBox IGameItemRenderHost.EmulatorComboBox => EmulatorComboBox;
    ComboBox IGameItemRenderHost.SystemComboBox => SystemComboBox;

    WrapPanel IGameItemRenderHost.GameFileGrid => GameFileGrid;
    ObservableCollection<GameListViewItem> IGameItemRenderHost.GameListItems => GameListItems;
    MainWindow IGameItemRenderHost.MainWindow => this;
}