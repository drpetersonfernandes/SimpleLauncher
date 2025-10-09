using System;
using System.ComponentModel;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SaveApplicationSettings();

        // Delete temp folders and files before close
        CleanSimpleLauncherFolder.CleanupTrash();

        // Stop Controller Timer
        StopControllerTimer();

        // Stop the status bar timer before closing
        if (StatusBarTimer != null)
        {
            StatusBarTimer.Stop();
            StatusBarTimer = null;
        }

        Dispose();
    }

    private void StopControllerTimer()
    {
        if (_controllerCheckTimer == null) return;

        _controllerCheckTimer?.Stop();
        _controllerCheckTimer = null;
    }

    public void Dispose()
    {
        // Dispose tray icon resources
        _trayIconManager?.Dispose();

        // Stop and dispose timers
        if (_controllerCheckTimer != null)
        {
            _controllerCheckTimer?.Stop();
            _controllerCheckTimer = null;
        }

        // Clean up collections
        GameListItems?.Clear();
        _currentSearchResults?.Clear();
        _systemManagers?.Clear();

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }
}