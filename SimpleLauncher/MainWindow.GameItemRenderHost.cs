using System.Collections.ObjectModel;
using System.Windows.Controls;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameItemRender;

namespace SimpleLauncher;

public partial class MainWindow : IGameItemRenderHost
{
    ScrollViewer IGameItemRenderHost.Scroller => Scroller;
    DataGrid IGameItemRenderHost.GameDataGrid => GameDataGrid;
    ComboBox IGameItemRenderHost.EmulatorComboBox => EmulatorComboBox;
    ComboBox IGameItemRenderHost.SystemComboBox => SystemComboBox;

    // ReSharper disable once ConvertToAutoProperty
    WrapPanel IGameItemRenderHost.GameFileGrid => _gameFileGrid;
    ObservableCollection<GameListViewItem> IGameItemRenderHost.GameListItems => GameListItems;
    MainWindow IGameItemRenderHost.MainWindow => this;
}
