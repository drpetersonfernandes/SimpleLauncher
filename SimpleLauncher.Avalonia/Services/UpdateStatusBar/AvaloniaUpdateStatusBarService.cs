using Avalonia.Threading;

namespace SimpleLauncher.Avalonia.Services.UpdateStatusBar;

/// <summary>
/// UI surface the status bar service drives.
/// </summary>
public interface IAvaloniaStatusBarHost
{
    /// <summary>Updates the status bar text content.</summary>
    void SetStatusText(string text);
}

/// <summary>
/// Manages status bar text updates with auto-clear timeout.
/// Extracted from the inline status-bar logic in MainViewModel and MainWindow.
/// Mirrors the WPF UpdateStatusBarService.
/// </summary>
public class AvaloniaUpdateStatusBarService
{
    private IAvaloniaStatusBarHost? _host;
    private DispatcherTimer? _clearTimer;
    private readonly int _timeoutSeconds;

    public AvaloniaUpdateStatusBarService(int timeoutSeconds = 3)
    {
        _timeoutSeconds = timeoutSeconds;
    }

    /// <summary>Initializes the service with the specified UI host.</summary>
    public void Initialize(IAvaloniaStatusBarHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Updates the status bar content and restarts the auto-clear timer.
    /// </summary>
    public void UpdateContent(string content)
    {
        var host = _host;
        if (host == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            host.SetStatusText(content);

            _clearTimer?.Stop();
            _clearTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_timeoutSeconds)
            };
            _clearTimer.Tick += (_, _) =>
            {
                host.SetStatusText("");
                _clearTimer.Stop();
            };
            _clearTimer.Start();
        });
    }

    /// <summary>
    /// Clears the status bar text immediately and stops the timer.
    /// </summary>
    public void Clear()
    {
        _clearTimer?.Stop();
        var host = _host;
        if (host != null)
        {
            Dispatcher.UIThread.Post(() => host.SetStatusText(""));
        }
    }
}
