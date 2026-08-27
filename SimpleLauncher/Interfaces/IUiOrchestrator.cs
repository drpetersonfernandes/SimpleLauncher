using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Orchestrates UI state changes for loading, navigation, pagination, and content display.
/// </summary>
public interface IUiOrchestrator
{
    /// <summary>
    /// Initializes the orchestrator and its child services with the specified UI host.
    /// </summary>
    /// <param name="host">The host providing access to the UI elements.</param>
    void Initialize(IUiOrchestratorHost host);

    /// <summary>
    /// Sets the loading state, optionally displaying a loading overlay with a message.
    /// </summary>
    /// <param name="isLoading">True to show loading; false to hide it.</param>
    /// <param name="message">Optional message to display during loading.</param>
    void SetLoadingState(bool isLoading, string? message = null);

    /// <summary>
    /// Forces an emergency release of the loading overlay regardless of current state.
    /// </summary>
    void EmergencyRelease();

    /// <summary>
    /// Navigates to a Page within the content frame, hiding the main game content.
    /// </summary>
    /// <param name="page">The page to navigate to.</param>
    void NavigateToPage(Page page);

    /// <summary>
    /// Navigates back to the main game content, clearing the content frame.
    /// </summary>
    void NavigateBackToMainContent();

    /// <summary>
    /// Resets pagination buttons to their initial state.
    /// </summary>
    void ResetPaginationButtons();

    /// <summary>
    /// Sets pagination buttons to their default disabled state.
    /// </summary>
    void SetPaginationButtonsDefault();

    /// <summary>
    /// Sets the visibility of pagination buttons.
    /// </summary>
    /// <param name="isVisible">Whether the buttons should be visible.</param>
    void SetPaginationButtonsVisible(bool isVisible);

    /// <summary>
    /// Enables or disables the previous and next page buttons.
    /// </summary>
    /// <param name="prevEnabled">Whether the previous page button is enabled.</param>
    /// <param name="nextEnabled">Whether the next page button is enabled.</param>
    void SetPaginationButtonsEnabled(bool prevEnabled, bool nextEnabled);

    /// <summary>
    /// Enables or disables the game buttons.
    /// </summary>
    /// <param name="isEnabled">Whether the game buttons should be enabled.</param>
    void SetGameButtonsEnabled(bool isEnabled);

    /// <summary>
    /// Clears all images from the game file grid buttons.
    /// </summary>
    void ClearGameButtonImages();

    /// <summary>
    /// Sets the visibility of the game file grid.
    /// </summary>
    /// <param name="isVisible">Whether the grid should be visible.</param>
    void SetGameFileGridVisible(bool isVisible);

    /// <summary>
    /// Sets the visibility of the list view preview area.
    /// </summary>
    /// <param name="isVisible">Whether the preview area should be visible.</param>
    void SetListViewPreviewAreaVisible(bool isVisible);

    /// <summary>
    /// Scrolls the main content scroller to the top.
    /// </summary>
    void ScrollToTop();

    /// <summary>
    /// Updates the total files label with the specified text.
    /// </summary>
    /// <param name="text">The text to display in the label.</param>
    void UpdateTotalFilesLabel(string text);

    /// <summary>
    /// Adds a message indicating no files were found.
    /// </summary>
    void AddNoFilesMessage();

    /// <summary>
    /// Clears the preview image.
    /// </summary>
    void ClearPreviewImage();

    /// <summary>
    /// Sets the text content of the search text box.
    /// </summary>
    /// <param name="text">The text to set.</param>
    void SetSearchTextBoxText(string text);

    /// <summary>
    /// Sets the visibility of the loading overlay.
    /// </summary>
    /// <param name="isVisible">Whether the overlay should be visible.</param>
    void SetLoadingOverlayVisible(bool isVisible);

    /// <summary>
    /// Configures the UI state before loading game files.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetUiBeforeLoadGameFilesAsync();

    /// <summary>
    /// Gets or sets the number of files displayed per page.
    /// </summary>
    int PaginationFilesPerPage { get; set; }

    /// <summary>
    /// Gets or sets the file count threshold at which pagination activates.
    /// </summary>
    int PaginationThreshold { get; set; }

    /// <summary>
    /// Applies pagination to the given file list and returns the current page subset.
    /// </summary>
    /// <param name="allFiles">The complete list of files to paginate.</param>
    /// <returns>The subset of files for the current page.</returns>
    IList<string> ApplyPagination(IList<string> allFiles);

    /// <summary>
    /// Determines whether navigation to the previous page is possible.
    /// </summary>
    /// <returns><c>true</c> if a previous page exists; otherwise <c>false</c>.</returns>
    bool CanGoToPrevPage();

    /// <summary>
    /// Determines whether navigation to the next page is possible.
    /// </summary>
    /// <returns><c>true</c> if a next page exists; otherwise <c>false</c>.</returns>
    bool CanGoToNextPage();

    /// <summary>
    /// Navigates to the previous page of results.
    /// </summary>
    void GoToPreviousPage();

    /// <summary>
    /// Navigates to the next page of results.
    /// </summary>
    void GoToNextPage();
}