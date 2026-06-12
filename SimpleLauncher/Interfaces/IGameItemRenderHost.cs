using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IGameItemRenderHost
{
    Dispatcher Dispatcher { get; }
    ScrollViewer Scroller { get; }
    DataGrid GameDataGrid { get; }
    ComboBox EmulatorComboBox { get; }
    ComboBox SystemComboBox { get; }
    WrapPanel GameFileGrid { get; }
    ObservableCollection<GameListViewItem> GameListItems { get; }
    MainWindow MainWindow { get; }
}
