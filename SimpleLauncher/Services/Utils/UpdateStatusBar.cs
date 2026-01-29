namespace SimpleLauncher.Services.Utils;

internal static class UpdateStatusBar
{
    internal static void UpdateContent(string content, MainWindow mainWindow)
    {
        mainWindow?.Dispatcher.Invoke(() =>
        {
            // Update the status bar text
            mainWindow.StatusBarText.Content = content;

            // Start or restart the timer to clear the status
            mainWindow.StatusBarTimer?.Stop(); // Stop any existing timer
            mainWindow.StatusBarTimer?.Start(); // Start a new one
        });
    }
}