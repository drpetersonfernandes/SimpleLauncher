using System.Windows;
using SimpleLauncher.Services.PlaySound;
using StatusBar = SimpleLauncher.Services.UpdateStatusBar.UpdateStatusBar;

namespace SimpleLauncher.Services.LoadingOverlay;

public class LoadingOverlayService
{
    private MainWindow _mainWindow;
    private int _loadingOperationsCount;
    private readonly object _loadingStateLock = new();
    private readonly PlaySoundEffects _playSoundEffects;

    public LoadingOverlayService(PlaySoundEffects playSoundEffects)
    {
        _playSoundEffects = playSoundEffects;
    }

    public void Initialize(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        bool shouldShowOverlay;

        lock (_loadingStateLock)
        {
            if (isLoading)
            {
                _loadingOperationsCount++;
            }
            else
            {
                if (_loadingOperationsCount > 0)
                {
                    _loadingOperationsCount--;
                }
                else
                {
                    DebugAndBugReport.DebugLogger.Log("[SetLoadingState] Warning: Attempted to decrement loading count when already at 0");
                }
            }

            shouldShowOverlay = _loadingOperationsCount > 0;
            _mainWindow.SetIsLoadingGamesInternal(shouldShowOverlay);
        }

        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.LoadingOverlay.Visibility = shouldShowOverlay ? Visibility.Visible : Visibility.Collapsed;
            _mainWindow.MainContentGrid.IsEnabled = !shouldShowOverlay;

            if (isLoading && shouldShowOverlay && message != null)
            {
                _mainWindow.LoadingOverlay.Content = message;
            }
            else if (!shouldShowOverlay)
            {
                _mainWindow.LoadingOverlay.Content = (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    public void EmergencyRelease()
    {
        _playSoundEffects?.PlayNotificationSound();

        lock (_loadingStateLock)
        {
            _loadingOperationsCount = 0;
            _mainWindow.SetIsLoadingGamesInternal(false);
        }

        _mainWindow.CancelAndRecreateToken();

        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.LoadingOverlay.Visibility = Visibility.Collapsed;
            _mainWindow.MainContentGrid.IsEnabled = true;
        });

        _mainWindow.ResetUiAsync();
        StatusBar.UpdateContent("Emergency reset performed.", _mainWindow);
        DebugAndBugReport.DebugLogger.Log("[Emergency] User forced overlay dismissal via Return button.");
    }
}
