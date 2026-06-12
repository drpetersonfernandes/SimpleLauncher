using System.Windows;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.LoadingOverlay;

using Interfaces;

public class LoadingOverlayService
{
    private ILoadingOverlayHost _host;
    private int _loadingOperationsCount;
    private readonly object _loadingStateLock = new();
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IDebugLogger _debugLogger;

    public LoadingOverlayService(PlaySoundEffects playSoundEffects, IDebugLogger debugLogger)
    {
        _playSoundEffects = playSoundEffects;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
    }

    public void Initialize(ILoadingOverlayHost host)
    {
        _host = host;
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        var host = _host;
        if (host == null) return;

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
                    _debugLogger.Log("[SetLoadingState] Warning: Attempted to decrement loading count when already at 0");
                }
            }

            shouldShowOverlay = _loadingOperationsCount > 0;
        }

        host.SetIsLoadingGamesInternal(shouldShowOverlay);

        host.Dispatcher.Invoke(() =>
        {
            host.SetLoadingOverlayVisible(shouldShowOverlay);
            host.SetMainContentGridEnabled(!shouldShowOverlay);

            if (isLoading && shouldShowOverlay && message != null)
            {
                host.SetLoadingOverlayContent(message);
            }
            else if (!shouldShowOverlay)
            {
                host.SetLoadingOverlayContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...");
            }
        });
    }

    public void EmergencyRelease()
    {
        var host = _host;
        if (host == null) return;

        _playSoundEffects?.PlayNotificationSound();

        lock (_loadingStateLock)
        {
            _loadingOperationsCount = 0;
        }

        host.SetIsLoadingGamesInternal(false);
        host.CancelAndRecreateToken();

        host.Dispatcher.Invoke(() =>
        {
            host.SetLoadingOverlayVisible(false);
            host.SetMainContentGridEnabled(true);
        });

        _ = host.ResetUiAsync();
        host.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
        _debugLogger.Log("[Emergency] User forced overlay dismissal via Return button.");
    }
}
