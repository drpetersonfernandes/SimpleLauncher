using System.Windows;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.LoadingOverlay;

public class LoadingOverlayService
{
    private ILoadingOverlayHost _host;
    private int _loadingOperationsCount;
    private readonly object _loadingStateLock = new();
    private readonly PlaySoundEffects _playSoundEffects;

    public LoadingOverlayService(PlaySoundEffects playSoundEffects)
    {
        _playSoundEffects = playSoundEffects;
    }

    public void Initialize(ILoadingOverlayHost host)
    {
        _host = host;
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
            _host.SetIsLoadingGamesInternal(shouldShowOverlay);
        }

        _host.Dispatcher.Invoke(() =>
        {
            _host.SetLoadingOverlayVisible(shouldShowOverlay);
            _host.SetMainContentGridEnabled(!shouldShowOverlay);

            if (isLoading && shouldShowOverlay && message != null)
            {
                _host.SetLoadingOverlayContent(message);
            }
            else if (!shouldShowOverlay)
            {
                _host.SetLoadingOverlayContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...");
            }
        });
    }

    public void EmergencyRelease()
    {
        _playSoundEffects?.PlayNotificationSound();

        lock (_loadingStateLock)
        {
            _loadingOperationsCount = 0;
            _host.SetIsLoadingGamesInternal(false);
        }

        _host.CancelAndRecreateToken();

        _host.Dispatcher.Invoke(() =>
        {
            _host.SetLoadingOverlayVisible(false);
            _host.SetMainContentGridEnabled(true);
        });

        _host.ResetUiAsync();
        _host.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
        DebugAndBugReport.DebugLogger.Log("[Emergency] User forced overlay dismissal via Return button.");
    }
}
