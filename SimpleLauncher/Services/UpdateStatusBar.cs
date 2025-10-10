namespace SimpleLauncher.Services;

public static class UpdateStatusBar
{
    public static void UpdateContent(string content, MainWindow mainWindow)
    {
        if (mainWindow == null)
        {
            // Handle null MainWindow (edge case)
            return;
        }

        // Update the status bar text
        mainWindow.Dispatcher.Invoke(() =>
        {
            mainWindow.StatusBarText.Content = content;
        });

        // Start or restart the 20-second timer to clear the status
        if (mainWindow.StatusBarTimer != null)
        {
            mainWindow.StatusBarTimer?.Stop(); // Stop any existing timer
            mainWindow.StatusBarTimer?.Start(); // Start a new one
        }
    }
}