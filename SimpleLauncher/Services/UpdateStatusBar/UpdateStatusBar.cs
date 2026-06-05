namespace SimpleLauncher.Services.UpdateStatusBar;

public class UpdateStatusBarService : IUpdateStatusBar
{
    public void UpdateContent(string content, MainWindow mainWindow)
    {
        mainWindow?.Dispatcher.Invoke(() =>
        {
            mainWindow.StatusBarText.Content = content;

            mainWindow.StatusBarTimer?.Stop();
            mainWindow.StatusBarTimer?.Start();
        });
    }
}