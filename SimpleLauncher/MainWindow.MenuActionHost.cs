using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.MenuActionHandler;
using SimpleLauncher.Services.UIReset;

namespace SimpleLauncher;

public partial class MainWindow : IMenuActionHost
{
    // Core operations - explicit implementations calling existing internal/private methods
    void IMenuActionHost.CancelAndRecreateToken()
    {
        CancelAndRecreateToken();
    }

    void IMenuActionHost.SetLoadingState(bool isLoading, string message)
    {
        SetLoadingState(isLoading, message);
    }

    Task IMenuActionHost.LoadGameFilesAsync(string startLetter, string searchQuery, CancellationToken cancellationToken)
    {
        return _gameBrowser.LoadGameFilesAsync(startLetter, searchQuery, cancellationToken);
    }

    (string startLetter, string searchQuery) IMenuActionHost.GetLoadGameFilesParams()
    {
        return GetLoadGameFilesParams();
    }

    void IMenuActionHost.ResetUiAsync()
    {
        ResetUiAsync();
    }

    void IMenuActionHost.LoadOrReloadSystemManager()
    {
        _gameBrowser.LoadOrReloadSystemManager();
    }

    void IMenuActionHost.NavigateToPage(Page page)
    {
        NavigateToPage(page);
    }

    void IMenuActionHost.NavigateBackToMainContent()
    {
        NavigateBackToMainContent();
    }

    Task IMenuActionHost.DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken)
    {
        return _gameBrowser.DisplaySystemSelectionScreenAsync(cancellationToken);
    }

    void IMenuActionHost.ResetPaginationButtons()
    {
        ResetPaginationButtons();
    }

    Task IMenuActionHost.ShowSystemFavoriteGamesAsync()
    {
        return ShowSystemFavoriteGamesClickAsync();
    }

    Task IMenuActionHost.ShowSystemFeelingLuckyAsync()
    {
        return ShowSystemFeelingLuckyClickAsync();
    }

    void IMenuActionHost.DeselectTopLetterNumberMenu()
    {
        _topLetterNumberMenu.DeselectLetter();
    }

    void IMenuActionHost.UpdateSortOrderButtonUi()
    {
        UpdateSortOrderButtonUi();
    }

    // State
    CancellationToken IMenuActionHost.CurrentCancellationToken => _cancellationSource.Token;

    string IMenuActionHost.GetMameSortOrder()
    {
        return ((IUiResetHost)this).MameSortOrder;
    }

    // Field setters
    void IMenuActionHost.SetGameButtonImageHeight(int height)
    {
        _gameBrowser.ImageHeight = height;
    }

    void IMenuActionHost.SetFilesPerPage(int count)
    {
        UiOrchestrator.PaginationFilesPerPage = count;
    }

    void IMenuActionHost.SetPaginationThreshold(int threshold)
    {
        UiOrchestrator.PaginationThreshold = threshold;
    }

    void IMenuActionHost.SetMameSortOrder(string sortOrder)
    {
        ((IUiResetHost)this).MameSortOrder = sortOrder;
    }

    void IMenuActionHost.SetIsLoadingGames(bool value)
    {
        _isLoadingGames = value;
        IsLoadingGames = value;
    }

    void IMenuActionHost.SetIsUiUpdating(bool value)
    {
        ((IUiResetHost)this).IsUiUpdating = value;
    }

    void IMenuActionHost.SetCurrentFilter(string filter)
    {
        ((IUiResetHost)this).CurrentFilter = filter;
    }

    void IMenuActionHost.SetActiveSearchQueryOrMode(string mode)
    {
        ((IUiResetHost)this).ActiveSearchQueryOrMode = mode;
    }

    void IMenuActionHost.SetIsResortOperation(bool value)
    {
        _isResortOperation = value;
    }

    // UI state setters
    void IMenuActionHost.SetViewModeUi(string viewMode)
    {
        SetViewMode(viewMode);
    }

    void IMenuActionHost.SetGridViewChecked(bool isChecked)
    {
        GridView.IsChecked = isChecked;
    }

    void IMenuActionHost.SetListViewChecked(bool isChecked)
    {
        ListView.IsChecked = isChecked;
    }

    void IMenuActionHost.SetGameFileGridVisibility(Visibility visibility)
    {
        GameFileGrid.Visibility = visibility;
    }

    void IMenuActionHost.SetListViewPreviewAreaVisibility(Visibility visibility)
    {
        ListViewPreviewArea.Visibility = visibility;
    }

    void IMenuActionHost.SetSearchTextBoxText(string text)
    {
        SearchTextBox.Text = text;
    }

    void IMenuActionHost.ClearPreviewImage()
    {
        PreviewImage.Source = null;
    }

    void IMenuActionHost.SetSystemComboBoxSelectedItem(object item)
    {
        SystemComboBox.SelectedItem = item;
    }

    void IMenuActionHost.SetEmulatorComboBoxSelectedItem(object item)
    {
        EmulatorComboBox.SelectedItem = item;
    }

    void IMenuActionHost.SetSortOrderToggleButtonVisibility(Visibility visibility)
    {
        SortOrderToggleButton.Visibility = visibility;
    }

    void IMenuActionHost.SetLoadingOverlayVisibility(Visibility visibility)
    {
        LoadingOverlay.Visibility = visibility;
    }

    void IMenuActionHost.SetSortOrderToggleButtonToolTip(string toolTip)
    {
        SortOrderToggleButton.ToolTip = toolTip;
    }

    // UI state getters
    string IMenuActionHost.GetSelectedSystem()
    {
        return SelectedSystem;
    }

    Visibility IMenuActionHost.GetTopSystemSelectionVisibility()
    {
        return TopSystemSelection.Visibility;
    }

    string IMenuActionHost.GetViewMode()
    {
        return _settings.ViewMode;
    }

    MenuItem IMenuActionHost.GetGridViewMenuItem()
    {
        return GridView;
    }

    MenuItem IMenuActionHost.GetListViewMenuItem()
    {
        return ListView;
    }

    // Data access
    List<Services.SystemManager.SystemManager> IMenuActionHost.GetSystemManagers()
    {
        return _systemManagers;
    }

    List<Services.MameManager.MameManager> IMenuActionHost.GetMachines()
    {
        return _gameBrowser.Machines.ToList();
    }

    Dictionary<string, string> IMenuActionHost.GetMameLookup()
    {
        return _gameBrowser.MameLookup;
    }

    // Language
    void IMenuActionHost.ChangeLanguageFromMenu(MenuItem menuItem)
    {
        _menuOrchestrator.ChangeLanguage(menuItem);
    }
}