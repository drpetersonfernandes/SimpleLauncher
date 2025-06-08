namespace SimpleLauncher.Services;

public static class UpdateStatusBar
{
    public static void UpdateContent(string content, MainWindow mainWindow)
    {
        if (mainWindow.Dispatcher.CheckAccess())
        {
            mainWindow.StatusBarText.Content = content;
        }
        else
        {
            mainWindow.Dispatcher.Invoke(() => { mainWindow.StatusBarText.Content = content; });
        }
    }
}