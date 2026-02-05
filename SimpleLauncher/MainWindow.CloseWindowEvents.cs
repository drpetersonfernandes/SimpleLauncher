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
        // Prevent double disposal
        if (_isDisposed)
            return;

        try
        {
            // Dispose tray icon resources
            _trayIconManager?.Dispose();

            // Clean up collections
            GameListItems?.Clear();
            _allGamesLock.Wait();
            try
            {
                _currentSearchResults?.Clear();
            }
            finally
            {
                _allGamesLock.Release();
            }

            _systemManagers?.Clear();
            _allGamesForCurrentSystem?.Clear();

            // Safely cancel and dispose the cancellation token source
            // Cancel() throws ObjectDisposedException if already disposed, so we catch it
            try
            {
                _cancellationSource?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already cancelled/disposed, ignore
            }

            _cancellationSource?.Dispose();
        }
        finally
        {
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}