namespace SimpleLauncher.Interfaces;

/// <summary>
///     Updates the status bar text content.
/// </summary>
public interface IUpdateStatusBar
{
    /// <summary>
    ///     Initializes the service with the specified status bar host.
    /// </summary>
    /// <param name="host">The host providing access to the status bar elements.</param>
    void Initialize(IStatusBarHost host);

    /// <summary>
    ///     Updates the status bar text and restarts the auto-clear timer.
    /// </summary>
    /// <param name="content">The text to display in the status bar.</param>
    void UpdateContent(string content);
}