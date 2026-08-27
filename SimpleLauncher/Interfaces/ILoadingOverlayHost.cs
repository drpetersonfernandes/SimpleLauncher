using System.Windows.Threading;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides access to UI host capabilities for managing the loading overlay and related UI state.
/// </summary>
public interface ILoadingOverlayHost
{
    /// <summary>
    /// Gets the dispatcher for the host window, used to marshal calls to the UI thread.
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    /// Sets the internal flag indicating whether games are currently being loaded.
    /// </summary>
    /// <param name="value">True if games are loading; otherwise, false.</param>
    void SetIsLoadingGamesInternal(bool value);

    /// <summary>
    /// Shows or hides the loading overlay.
    /// </summary>
    /// <param name="isVisible">True to show the overlay; false to hide it.</param>
    void SetLoadingOverlayVisible(bool isVisible);

    /// <summary>
    /// Sets the content displayed on the loading overlay.
    /// </summary>
    /// <param name="content">The content to display on the overlay.</param>
    void SetLoadingOverlayContent(object content);

    /// <summary>
    /// Enables or disables the main content grid, preventing or allowing user interaction.
    /// </summary>
    /// <param name="enabled">True to enable the grid; false to disable it.</param>
    void SetMainContentGridEnabled(bool enabled);

    /// <summary>
    /// Cancels the current operation and recreates the cancellation token.
    /// </summary>
    void CancelAndRecreateToken();

    /// <summary>
    /// Resets the UI to its default state asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetUiAsync();

    /// <summary>
    /// Gets the service used to update the status bar content.
    /// </summary>
    IUpdateStatusBar UpdateStatusBarService { get; }
}