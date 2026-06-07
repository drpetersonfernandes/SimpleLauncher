using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services.GameListUI;

public interface IGameListUiHost
{
    Dispatcher Dispatcher { get; }
    ScrollViewer Scroller { get; }
    Image PreviewImage { get; }
    WrapPanel GameFileGrid { get; }
    Grid ListViewPreviewArea { get; }
    ObservableCollection<GameListViewItem> GameListItems { get; }
    void SetGameFileGridVisible(bool isVisible);
    void SetListViewPreviewAreaVisible(bool isVisible);
    void SetPaginationButtonsVisible(bool isVisible);
}
