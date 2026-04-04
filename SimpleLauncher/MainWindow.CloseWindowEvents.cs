using System;
using System.ComponentModel;
using SimpleLauncher.Services.MountFiles;

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
        // Prevent double disposal
        if (_isDisposed)
            return;

        // 1. Signal cancellation FIRST to help background threads release locks sooner.
        // We catch ObjectDisposedException to prevent errors if cancellation was already triggered.
        try
        {
            _cancellationSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already cancelled/disposed, ignore
        }

        try
        {
            // Kill any lingering CHDMounter processes as a safety net
            MountChdFiles.KillAllChdMounterProcesses();

            // Dispose tray icon resources
            _trayIconManager?.Dispose();

            // Clean up collections
            GameListItems?.Clear();

            // 2. Use a small timeout to avoid blocking the UI thread indefinitely during disposal.
            // If the lock cannot be acquired within 100ms, it's safer to skip clearing and continue shutdown.
            if (_allGamesLock.Wait(100))
            {
                try
                {
                    _currentSearchResults?.Clear();
                }
                finally
                {
                    _allGamesLock.Release();
                }
            }

            _systemManagers?.Clear();
            _allGamesForCurrentSystem?.Clear();

            _cancellationSource?.Dispose();
        }
        finally
        {
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}