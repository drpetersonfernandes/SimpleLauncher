using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services.UiOrchestrator;

public interface IUiOrchestratorHost
{
    Dispatcher Dispatcher { get; }

    ScrollViewer Scroller { get; }
    Image PreviewImage { get; }
    WrapPanel GameFileGrid { get; }
    Grid ListViewPreviewArea { get; }
    Frame PageContentFrame { get; }
    Grid MainGameContent { get; }
    Grid MainContentGrid { get; }
    Label TotalFilesLabel { get; }
    Button PrevPageButton2 { get; }
    Button NextPageButton2 { get; }
    UIElement LoadingOverlay { get; }
    Button SortOrderToggleButton { get; }
    TextBox SearchTextBox { get; }
    ComboBox SystemComboBox { get; }
    ComboBox EmulatorComboBox { get; }
    ObservableCollection<GameListViewItem> GameListItems { get; }

    bool IsLoadingGames { get; }

    void SetIsLoadingGamesInternal(bool value);
    void CancelAndRecreateToken();
    Task ResetUiAsync();
}
