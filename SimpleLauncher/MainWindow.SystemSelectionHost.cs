using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Models;
using SimpleLauncher.Services.SystemSelectionOrchestrator;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow : ISystemSelectionHost
{
    Dispatcher ISystemSelectionHost.Dispatcher => Dispatcher;

    WrapPanel ISystemSelectionHost.GameFileGrid => GameFileGrid;
    Border ISystemSelectionHost.TopSystemSelection => TopSystemSelection;
    Grid ISystemSelectionHost.StatusBarArea => StatusBarArea;
    Grid ISystemSelectionHost.ListViewPreviewArea => ListViewPreviewArea;
    Image ISystemSelectionHost.PreviewImage => PreviewImage;
    Label ISystemSelectionHost.TotalFilesLabel => TotalFilesLabel;
    Button ISystemSelectionHost.PrevPageButton2 => PrevPageButton2;
    Button ISystemSelectionHost.NextPageButton2 => NextPageButton2;
    TextBox ISystemSelectionHost.SearchTextBox => SearchTextBox;
    ComboBox ISystemSelectionHost.SystemComboBox => SystemComboBox;
    ComboBox ISystemSelectionHost.EmulatorComboBox => EmulatorComboBox;
    Button ISystemSelectionHost.SortOrderToggleButton => SortOrderToggleButton;
    ObservableCollection<GameListViewItem> ISystemSelectionHost.GameListItems => GameListItems;

    string ISystemSelectionHost.SelectedSystem
    {
        get => SelectedSystem;
        set => SelectedSystem = value;
    }

    string ISystemSelectionHost.PlayTime
    {
        get => PlayTime;
        set => PlayTime = value;
    }

    bool ISystemSelectionHost.IsPlayTimeVisible
    {
        get => IsPlayTimeVisible;
        set => IsPlayTimeVisible = value;
    }

    void ISystemSelectionHost.SetLoadingState(bool isLoading, string message)
    {
        SetLoadingState(isLoading, message);
    }

    void ISystemSelectionHost.CancelAndRecreateToken()
    {
        CancelAndRecreateToken();
    }

    CancellationToken ISystemSelectionHost.CurrentCancellationToken => _cancellationSource.Token;

    Task ISystemSelectionHost.ResetUiAsync()
    {
        try
        {
            ResetUiAsync();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    void ISystemSelectionHost.ResetPaginationButtons()
    {
        ResetPaginationButtons();
    }

    void ISystemSelectionHost.UpdateSortOrderButtonUi()
    {
        UpdateSortOrderButtonUi();
    }

    void ISystemSelectionHost.ClearGameButtonImages(Panel panel)
    {
        ClearGameButtonImages(panel);
    }

    List<SystemManager> ISystemSelectionHost.GetSystemManagers()
    {
        return _systemManagers;
    }

    void ISystemSelectionHost.SetSystemManagers(List<SystemManager> managers)
    {
        _systemManagers = managers;
    }

    void ISystemSelectionHost.SetSelectedImageFolder(string folder)
    {
        _selectedImageFolder = folder;
    }

    void ISystemSelectionHost.SetSelectedRomFolders(List<string> folders)
    {
        _selectedRomFolders = folders;
    }
}
