using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher;

/// <summary>
///     Partial MainWindow containing window closing, disposal, and event unsubscription logic.
/// </summary>
public partial class MainWindow
{
    private bool _isCloseSaveDeferred;

    /// <summary>
    ///     Disposes all managed resources including cancellation tokens, event handlers, and background services.
    /// </summary>
    public void Dispose()
    {
        // Prevent double disposal
        if (_isDisposed)
            return;

        try
        {
            // Set flag to signal async handlers to bail out
            _isDisposed = true;

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

            // Kill any lingering CHDMounter processes as a safety net
            _mountChdFiles?.KillAllChdMounterProcesses(_logger);

            // Stop and release the status bar timer
            if (StatusBarTimer != null)
            {
                StatusBarTimer.Stop();
                StatusBarTimer = null;
            }

            // Dispose tray icon resources
            TrayIconManager?.Dispose();

            // Dispose F8 global hotkey
            _globalHotkeyService?.Dispose();

            // Clean up collections
            GameListItems?.Clear();
            GameDataGrid?.ItemsSource = null;

            // Clear game caches via the cache service
            _gameBrowser?.ClearCache();

            _systemManagers?.Clear();

            _cancellationSource?.Dispose();
        }
        catch (Exception ex)
        {
            // Log but don't throw during disposal
            Log.Debug($"Error during Dispose: {ex.Message}");
        }

        GC.SuppressFinalize(this);
    }

    private async void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        try
        {
            // Defer the actual close until the settings save completes; otherwise the
            // process can exit before the background write finishes and lose the last changes.
            if (!_isCloseSaveDeferred)
            {
                e.Cancel = true;
                _isCloseSaveDeferred = true;

                try
                {
                    await SaveApplicationSettings();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error saving settings during close: {ex.Message}");
                }

                Close();
                return;
            }

            // Unsubscribe from events to prevent memory leaks
            UnsubscribeEventHandlers();

            Dispose();
        }
        catch (Exception ex)
        {
            Log.Error($"Error in Main Window_Closing. Error: {ex.Message}");
        }
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            ShowInTaskbar = false;
        }
    }

    /// <summary>
    ///     Unsubscribes all event handlers to prevent memory leaks.
    /// </summary>
    private void UnsubscribeEventHandlers()
    {
        // Unsubscribe window-level event handlers
        Closing -= MainWindow_Closing;
        StateChanged -= MainWindow_StateChanged;
        Activated -= MainWindow_Activated;
        Deactivated -= MainWindow_Deactivated;

        // Unsubscribe the async Loaded handler if it was subscribed
        if (_asyncLoadedHandler != null) Loaded -= _asyncLoadedHandler;

        // Unsubscribe FilterMenu event handler
        if (_topLetterNumberMenu != null)
            _topLetterNumberMenu.OnLetterSelected -= TopLetterNumberMenu_OnLetterSelectedAsync;

        // Unsubscribe emergency button click handler if it was wired
        if (_emergencyButtonClickHandler != null && LoadingOverlay?.Template != null)
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
                emergencyBtn.Click -= _emergencyButtonClickHandler;

        // Unsubscribe and stop game file watcher
        _lifecycle.UnsubscribeGameFilesChanged(_gameFilesChangedHandler);
        _lifecycle.StopWatching();

        // Unsubscribe game played event
        _gameLauncherService.GamePlayed -= _gamePlayedHandler;
    }
}