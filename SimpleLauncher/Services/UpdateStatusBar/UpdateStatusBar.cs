namespace SimpleLauncher.Services.UpdateStatusBar;

public class UpdateStatusBarService : IUpdateStatusBar
{
    private IStatusBarHost _host;

    public void Initialize(IStatusBarHost host)
    {
        _host = host;
    }

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
