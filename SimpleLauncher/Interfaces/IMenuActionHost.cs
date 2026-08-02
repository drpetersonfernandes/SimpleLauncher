using SimpleLauncher.Core.Services.MameManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides host operations and state access for menu-driven actions in the launcher UI.
/// </summary>
public interface IMenuActionHost
{
    /// <summary>
    /// Cancels the current operation and recreates the cancellation token.
    /// </summary>
    void CancelAndRecreateToken();

    /// <summary>
    /// Sets the loading state and optionally displays a loading message.
    /// </summary>
    /// <param name="isLoading">True to show loading; false to hide it.</param>
    /// <param name="message">Optional message to display during loading.</param>
    void SetLoadingState(bool isLoading, string? message = null);

    /// <summary>
    /// Loads game files for the selected system, optionally starting from a letter or filtering by search query.
    /// </summary>
    /// <param name="startLetter">Optional letter to start loading from.</param>
    /// <param name="searchQuery">Optional search query to filter games.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadGameFilesAsync(string? startLetter = null, string? searchQuery = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current parameters used for loading game files.
    /// </summary>
    /// <returns>A tuple containing the start letter and search query.</returns>
    (string? startLetter, string? searchQuery) GetLoadGameFilesParams();

    /// <summary>
    /// Resets the UI to its default state asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetUiAsync();

    /// <summary>
    /// Loads or reloads the system manager with the current system configuration.
    /// </summary>
    void LoadOrReloadSystemManager();

    /// <summary>
    /// Asynchronously loads or reloads the system manager with the current system configuration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadOrReloadSystemManagerAsync();

    /// <summary>
    /// Navigates to the specified page in the page content frame.
    /// </summary>
    /// <param name="page">The page to navigate to.</param>
    void NavigateToPage(object page);

    /// <summary>
    /// Navigates back to the main content view.
    /// </summary>
    void NavigateBackToMainContent();

    /// <summary>
    /// Displays the system selection screen asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the pagination buttons to their default state.
    /// </summary>
    void ResetPaginationButtons();

    /// <summary>
    /// Displays the favorite games for the selected system.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowSystemFavoriteGamesAsync();

    /// <summary>
    /// Displays a random selection of games for the selected system.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowSystemFeelingLuckyAsync();

    /// <summary>
    /// Deselects the currently selected item in the top letter/number menu.
    /// </summary>
    void DeselectTopLetterNumberMenu();

    /// <summary>
    /// Updates the sort order toggle button UI to reflect the current sort order.
    /// </summary>
    void UpdateSortOrderButtonUi();

    /// <summary>
    /// Gets whether games are currently being loaded.
    /// </summary>
    bool IsLoadingGames { get; }

    /// <summary>
    /// Gets the current cancellation token for the active operation.
    /// </summary>
    CancellationToken CurrentCancellationToken { get; }

    /// <summary>
    /// Gets the current MAME sort order setting.
    /// </summary>
    /// <returns>The sort order string.</returns>
    string GetMameSortOrder();

    /// <summary>
    /// Sets the height of game button images.
    /// </summary>
    /// <param name="height">The image height in pixels.</param>
    void SetGameButtonImageHeight(int height);

    /// <summary>
    /// Sets the number of files to display per page.
    /// </summary>
    /// <param name="count">The number of files per page.</param>
    void SetFilesPerPage(int count);

    /// <summary>
    /// Sets the pagination threshold that determines when pagination is applied.
    /// </summary>
    /// <param name="threshold">The pagination threshold value.</param>
    void SetPaginationThreshold(int threshold);

    /// <summary>
    /// Sets the MAME sort order.
    /// </summary>
    /// <param name="sortOrder">The sort order string to apply.</param>
    void SetMameSortOrder(string sortOrder);

    /// <summary>
    /// Sets the flag indicating whether games are currently being loaded.
    /// </summary>
    /// <param name="value">True if loading; otherwise, false.</param>
    void SetIsLoadingGames(bool value);

    /// <summary>
    /// Sets the flag indicating whether the UI is currently being updated.
    /// </summary>
    /// <param name="value">True if updating; otherwise, false.</param>
    void SetIsUiUpdating(bool value);

    /// <summary>
    /// Sets the current filter string for game list filtering.
    /// </summary>
    /// <param name="filter">The filter string, or null to clear the filter.</param>
    void SetCurrentFilter(string? filter);

    /// <summary>
    /// Sets the active search query or mode for the current view.
    /// </summary>
    /// <param name="mode">The search query or mode string.</param>
    void SetActiveSearchQueryOrMode(string? mode);

    /// <summary>
    /// Sets the flag indicating whether the current operation is a resort.
    /// </summary>
    /// <param name="value">True if resorting; otherwise, false.</param>
    void SetIsResortOperation(bool value);

    /// <summary>
    /// Sets the view mode UI to the specified mode (e.g., grid or list).
    /// </summary>
    /// <param name="viewMode">The view mode identifier.</param>
    void SetViewModeUi(string viewMode);

    /// <summary>
    /// Sets the checked state of the grid view menu item.
    /// </summary>
    /// <param name="isChecked">True to check; false to uncheck.</param>
    void SetGridViewChecked(bool isChecked);

    /// <summary>
    /// Sets the checked state of the list view menu item.
    /// </summary>
    /// <param name="isChecked">True to check; false to uncheck.</param>
    void SetListViewChecked(bool isChecked);

    /// <summary>
    /// Shows or hides the game file grid.
    /// </summary>
    /// <param name="isVisible">True to show; false to hide.</param>
    void SetGameFileGridVisible(bool isVisible);

    /// <summary>
    /// Shows or hides the list view preview area.
    /// </summary>
    /// <param name="isVisible">True to show; false to hide.</param>
    void SetListViewPreviewAreaVisible(bool isVisible);

    /// <summary>
    /// Sets the text content of the search text box.
    /// </summary>
    /// <param name="text">The text to set.</param>
    void SetSearchTextBoxText(string text);

    /// <summary>
    /// Clears the preview image display.
    /// </summary>
    void ClearPreviewImage();

    /// <summary>
    /// Sets the selected item in the system combo box.
    /// </summary>
    /// <param name="item">The item to select.</param>
    void SetSystemComboBoxSelectedItem(object item);

    /// <summary>
    /// Sets the selected item in the emulator combo box.
    /// </summary>
    /// <param name="item">The item to select.</param>
    void SetEmulatorComboBoxSelectedItem(object item);

    /// <summary>
    /// Shows or hides the loading overlay.
    /// </summary>
    /// <param name="isVisible">True to show; false to hide.</param>
    void SetLoadingOverlayVisible(bool isVisible);

    /// <summary>
    /// Sets the tooltip text for the sort order toggle button.
    /// </summary>
    /// <param name="toolTip">The tooltip text.</param>
    void SetSortOrderToggleButtonToolTip(string toolTip);

    /// <summary>
    /// Gets the name of the currently selected system.
    /// </summary>
    /// <returns>The selected system name.</returns>
    string GetSelectedSystem();

    /// <summary>
    /// Gets whether the top system selection area is visible.
    /// </summary>
    /// <returns>True if visible; otherwise, false.</returns>
    bool IsTopSystemSelectionVisible();

    /// <summary>
    /// Gets the current view mode.
    /// </summary>
    /// <returns>The view mode identifier string.</returns>
    string GetViewMode();

    /// <summary>
    /// Gets the identifier for the grid view menu item.
    /// </summary>
    string GridViewMenuItemId { get; }

    /// <summary>
    /// Gets the identifier for the list view menu item.
    /// </summary>
    string ListViewMenuItemId { get; }

    /// <summary>
    /// Gets the list of available system managers.
    /// </summary>
    /// <returns>The list of system managers.</returns>
    IList<SystemManagerService> GetSystemManagers();

    /// <summary>
    /// Gets the list of MAME machines.
    /// </summary>
    /// <returns>The list of MAME machines.</returns>
    IList<MameManagerService> GetMachines();

    /// <summary>
    /// Gets the MAME lookup dictionary mapping machine names to descriptions.
    /// </summary>
    /// <returns>The MAME lookup dictionary.</returns>
    IDictionary<string, string> GetMameLookup();

    /// <summary>
    /// Changes the application language to the specified language code.
    /// </summary>
    /// <param name="languageCode">The language code to apply.</param>
    void ChangeLanguageAsync(string languageCode);
}
