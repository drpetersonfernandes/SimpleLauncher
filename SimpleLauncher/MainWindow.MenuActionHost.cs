using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.MenuActionHandler;

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
        return LoadGameFilesAsync(startLetter, searchQuery, cancellationToken);
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
        LoadOrReloadSystemManager();
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
        return DisplaySystemSelectionScreenAsync(cancellationToken);
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
        return _mameSortOrder;
    }

    // Field setters
    void IMenuActionHost.SetGameButtonImageHeight(int height)
    {
        _gameButtonFactory.ImageHeight = height;
    }

    void IMenuActionHost.SetFilesPerPage(int count)
    {
        _filesPerPage = count;
    }

    void IMenuActionHost.SetPaginationThreshold(int threshold)
    {
        _paginationThreshold = threshold;
    }

    void IMenuActionHost.SetMameSortOrder(string sortOrder)
    {
        _mameSortOrder = sortOrder;
    }

    void IMenuActionHost.SetIsLoadingGames(bool value)
    {
        _isLoadingGames = value;
        IsLoadingGames = value;
    }

    void IMenuActionHost.SetIsUiUpdating(bool value)
    {
        _isUiUpdating = value;
    }

    void IMenuActionHost.SetCurrentFilter(string filter)
    {
        _currentFilter = filter;
    }

    void IMenuActionHost.SetActiveSearchQueryOrMode(string mode)
    {
        _activeSearchQueryOrMode = mode;
    }

    void IMenuActionHost.SetIsResortOperation(bool value)
    {
        _isResortOperation = value;
    }

    // Check mark updates
    void IMenuActionHost.UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
        UpdateThumbnailSizeCheckMarks(selectedSize);
    }

    void IMenuActionHost.UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        UpdateButtonAspectRatioCheckMarks(selectedValue);
    }

    void IMenuActionHost.UpdateNumberOfGamesPerPageCheckMarks(int selectedSize)
    {
        UpdateNumberOfGamesPerPageCheckMarks(selectedSize);
    }

    void IMenuActionHost.UpdateShowGamesCheckMarks(string selectedValue)
    {
        UpdateShowGamesCheckMarks(selectedValue);
    }

    void IMenuActionHost.UpdateFilenameDisplayModeCheckMarks(string selectedValue)
    {
        UpdateFilenameDisplayModeCheckMarks(selectedValue);
    }

    void IMenuActionHost.UpdateFilenameFontSizeCheckMarks(string selectedValue)
    {
        UpdateFilenameFontSizeCheckMarks(selectedValue);
    }

    void IMenuActionHost.UpdateMachineNameFontSizeCheckMarks(string selectedValue)
    {
        UpdateMachineNameFontSizeCheckMarks(selectedValue);
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
        return _machines;
    }

    Dictionary<string, string> IMenuActionHost.GetMameLookup()
    {
        return _mameLookup;
    }

    // Language
    void IMenuActionHost.ChangeLanguageFromMenu(MenuItem menuItem)
    {
        _languageMenuService.ChangeLanguage(menuItem);
    }
}