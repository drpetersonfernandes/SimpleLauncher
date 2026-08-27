namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides UI host callbacks for pagination controls and file list feedback.
/// </summary>
public interface IPaginationHost
{
    /// <summary>
    /// Enables or disables the previous page navigation button.
    /// </summary>
    /// <param name="enabled">True to enable the button; otherwise, false.</param>
    void SetPrevPageButtonEnabled(bool enabled);

    /// <summary>
    /// Enables or disables the next page navigation button.
    /// </summary>
    /// <param name="enabled">True to enable the button; otherwise, false.</param>
    void SetNextPageButtonEnabled(bool enabled);

    /// <summary>
    /// Scrolls the main content scroller to the top.
    /// </summary>
    void ScrollToTop();

    /// <summary>
    /// Updates the total files label with the specified text.
    /// </summary>
    /// <param name="text">The text to display in the label.</param>
    void UpdateTotalFilesLabel(string? text);

    /// <summary>
    /// Adds a message indicating that no files were found.
    /// </summary>
    void AddNoFilesMessage();
}