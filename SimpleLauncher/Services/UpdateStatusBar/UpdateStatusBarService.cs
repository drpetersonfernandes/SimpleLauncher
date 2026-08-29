using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.UpdateStatusBar;

/// <summary>
///     Updates the status bar text content and manages its auto-clear timer.
/// </summary>
public class UpdateStatusBarService : IUpdateStatusBar
{
    private IStatusBarHost _host = null!;

    /// <summary>Initializes the service with the specified status bar host.</summary>
    public void Initialize(IStatusBarHost host)
    {
        _host = host;
    }

    /// <summary>Updates the status bar text and restarts the auto-clear timer.</summary>
    public void UpdateContent(string content)
    {
        _host?.Dispatcher.Invoke(() =>
        {
            _host.StatusBarText.Content = content;

            _host.StatusBarTimer?.Stop();
            _host.StatusBarTimer?.Start();
        });
    }
}