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

        Dispose();

        // Stop Controller Timer
        StopControllerTimer();
    }

    private void StopControllerTimer()
    {
        if (_controllerCheckTimer == null) return;

        _controllerCheckTimer.Stop();
        _controllerCheckTimer = null;
    }

    public void Dispose()
    {
        // Dispose gamepad resources
        GamePadController.Instance2.Stop();
        GamePadController.Instance2.Dispose();

        // Dispose tray icon resources
        _trayIconManager?.Dispose();

        // Dispose instances of HttpClient
        Stats.DisposeHttpClient();
        UpdateChecker.DisposeHttpClient();
        SupportWindow.DisposeHttpClient();
        LogErrors.DisposeHttpClient();

        // Stop and dispose timers
        if (_controllerCheckTimer != null)
        {
            _controllerCheckTimer.Stop();
            _controllerCheckTimer = null;
        }

        // Clean up collections
        GameListItems?.Clear();
        _cachedFiles?.Clear();
        _currentSearchResults?.Clear();
        _systemConfigs?.Clear();

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }
}