using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher.Services.MenuActionHandler;

public interface IMenuActionHost
{
    // Core operations
    void CancelAndRecreateToken();
    void SetLoadingState(bool isLoading, string message = null);
    Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default);
    (string startLetter, string searchQuery) GetLoadGameFilesParams();
    void ResetUiAsync();
    void LoadOrReloadSystemManager();
    void NavigateToPage(Page page);
    void NavigateBackToMainContent();
    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default);
    void ResetPaginationButtons();
    Task ShowSystemFavoriteGamesAsync();
    Task ShowSystemFeelingLuckyAsync();
    void DeselectTopLetterNumberMenu();
    void UpdateSortOrderButtonUi();

    // State
    bool IsLoadingGames { get; }
    CancellationToken CurrentCancellationToken { get; }
    string GetMameSortOrder();

    // Field setters
    void SetGameButtonImageHeight(int height);
    void SetFilesPerPage(int count);
    void SetPaginationThreshold(int threshold);
    void SetMameSortOrder(string sortOrder);
    void SetIsLoadingGames(bool value);
    void SetIsUiUpdating(bool value);
    void SetCurrentFilter(string filter);
    void SetActiveSearchQueryOrMode(string mode);
    void SetIsResortOperation(bool value);

    // UI state setters
    void SetViewModeUi(string viewMode);
    void SetGridViewChecked(bool isChecked);
    void SetListViewChecked(bool isChecked);
    void SetGameFileGridVisibility(Visibility visibility);
    void SetListViewPreviewAreaVisibility(Visibility visibility);
    void SetSearchTextBoxText(string text);
    void ClearPreviewImage();
    void SetSystemComboBoxSelectedItem(object item);
    void SetEmulatorComboBoxSelectedItem(object item);
    void SetSortOrderToggleButtonVisibility(Visibility visibility);
    void SetLoadingOverlayVisibility(Visibility visibility);
    void SetSortOrderToggleButtonToolTip(string toolTip);

    // UI state getters
    string GetSelectedSystem();
    Visibility GetTopSystemSelectionVisibility();
    string GetViewMode();
    MenuItem GetGridViewMenuItem();
    MenuItem GetListViewMenuItem();

    // Data access for page construction
    List<SystemManager.SystemManager> GetSystemManagers();
    List<MameManager.MameManager> GetMachines();
    Dictionary<string, string> GetMameLookup();

    // Language
    void ChangeLanguageFromMenu(MenuItem menuItem);
}