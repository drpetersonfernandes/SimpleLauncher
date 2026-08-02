namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides access to the host state and UI elements used when resetting the UI.
/// </summary>
public interface IUiResetHost
{
    // State
    /// <summary>
    /// Gets or sets a value indicating whether the UI is currently being updated.
    /// </summary>
    bool IsUiUpdating { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether games are currently being loaded.
    /// </summary>
    bool IsLoadingGames { get; set; }

    /// <summary>
    /// Gets or sets the current filter string for game list filtering.
    /// </summary>
    string? CurrentFilter { get; set; }

    /// <summary>
    /// Gets or sets the active search query or mode for the current view.
    /// </summary>
    string? ActiveSearchQueryOrMode { get; set; }

    /// <summary>
    /// Gets or sets the name of the currently selected system.
    /// </summary>
    string? SelectedSystem { get; set; }

    /// <summary>
    /// Gets or sets the play time string shown in the status bar.
    /// </summary>
    string PlayTime { get; set; }

    /// <summary>
    /// Gets or sets the current MAME sort order setting.
    /// </summary>
    string MameSortOrder { get; set; }

    /// <summary>
    /// Gets the current cancellation token for the active operation.
    /// </summary>
    CancellationToken CurrentCancellationToken { get; }

    // Operations
    /// <summary>
    /// Cancels the current operation and recreates the cancellation token.
    /// </summary>
    void CancelAndRecreateToken();

    /// <summary>
    /// Resets the pagination buttons to their default state.
    /// </summary>
    void ResetPaginationButtons();

    /// <summary>
    /// Displays the system selection screen asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken);

    // UI element setters
    /// <summary>
    /// Shows or hides the loading overlay.
    /// </summary>
    /// <param name="isVisible">True to show; false to hide.</param>
    void SetLoadingOverlayVisible(bool isVisible);

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
    /// <param name="item">The item to select, or null to clear the selection.</param>
    void SetSystemComboBoxSelectedItem(object? item);

    /// <summary>
    /// Sets the selected item in the emulator combo box.
    /// </summary>
    /// <param name="item">The item to select, or null to clear the selection.</param>
    void SetEmulatorComboBoxSelectedItem(object? item);
}
