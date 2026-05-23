using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.MountFiles;

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

        // Unsubscribe from events to prevent memory leaks
        UnsubscribeEventHandlers();

        Dispose();
    }

    /// <summary>
    /// Unsubscribes all event handlers to prevent memory leaks.
    /// </summary>
    private void UnsubscribeEventHandlers()
    {
        // Unsubscribe window-level event handlers
        Loaded -= MainWindow_Loaded;
        Closing -= MainWindow_Closing;
        Activated -= MainWindow_Activated;
        Deactivated -= MainWindow_Deactivated;

        // Unsubscribe the async Loaded handler if it was subscribed
        if (_asyncLoadedHandler != null)
        {
            Loaded -= _asyncLoadedHandler;
        }

        // Unsubscribe FilterMenu event handler
        if (_topLetterNumberMenu != null)
        {
            _topLetterNumberMenu.OnLetterSelected -= TopLetterNumberMenu_OnLetterSelected;
        }

        // Unsubscribe emergency button click handler if it was wired
        if (_emergencyButtonClickHandler != null && LoadingOverlay?.Template != null)
        {
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click -= _emergencyButtonClickHandler;
            }
        }
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
            MountChdFiles.KillAllChdMounterProcesses(App.ServiceProvider.GetRequiredService<ILogErrors>());

            // Dispose tray icon resources
            TrayIconManager?.Dispose();

            // Clean up collections
            GameListItems?.Clear();

            // 2. Use a small timeout to avoid blocking the UI thread indefinitely during disposal.
            // If the lock cannot be acquired within 100ms, it's safer to skip clearing and continue shutdown.
            // Also check disposal state to prevent ObjectDisposedException.
            if (!_isDisposed)
            {
                try
                {
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
                }
                catch (ObjectDisposedException)
                {
                    // Semaphore was disposed, ignore and continue shutdown
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