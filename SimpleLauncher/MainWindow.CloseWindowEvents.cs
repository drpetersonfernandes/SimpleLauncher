using System;
using System.ComponentModel;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SaveApplicationSettings();

        // Stop the status bar timer before closing
        if (StatusBarTimer != null)
        {
            StatusBarTimer.Stop();
            StatusBarTimer = null;
        }

        Dispose();
    }

    public void Dispose()
    {
        // Dispose tray icon resources
        _trayIconManager?.Dispose();

        // Clean up collections
        GameListItems?.Clear();
        _currentSearchResults?.Clear();
        _systemManagers?.Clear();
        _allGamesForCurrentSystem?.Clear();

        _cancellationSource?.Cancel();
        _cancellationSource?.Dispose();

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }
}