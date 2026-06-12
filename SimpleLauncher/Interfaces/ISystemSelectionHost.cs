using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface ISystemSelectionHost
{
    Dispatcher Dispatcher { get; }

    WrapPanel GameFileGrid { get; }
    Border TopSystemSelection { get; }
    Grid StatusBarArea { get; }
    Grid ListViewPreviewArea { get; }
    Image PreviewImage { get; }
    Label TotalFilesLabel { get; }
    Button PrevPageButton2 { get; }
    Button NextPageButton2 { get; }
    TextBox SearchTextBox { get; }
    ComboBox SystemComboBox { get; }
    ComboBox EmulatorComboBox { get; }
    Button SortOrderToggleButton { get; }
    ObservableCollection<GameListViewItem> GameListItems { get; }

    string SelectedSystem { get; set; }
    string PlayTime { get; set; }
    bool IsPlayTimeVisible { get; set; }

    void SetLoadingState(bool isLoading, string message = null);
    void CancelAndRecreateToken();
    CancellationToken CurrentCancellationToken { get; }
    Task ResetUiAsync();
    void ResetPaginationButtons();
    void UpdateSortOrderButtonUi();
    void ClearGameButtonImages(Panel panel);

    List<Services.SystemManager.SystemManager> GetSystemManagers();
    void SetSystemManagers(List<Services.SystemManager.SystemManager> managers);
    void SetSelectedImageFolder(string folder);
    void SetSelectedRomFolders(List<string> folders);
}
