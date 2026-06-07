using System.Windows.Threading;

namespace SimpleLauncher.Services.LoadingOverlay;

public interface ILoadingOverlayHost
{
    Dispatcher Dispatcher { get; }
    void SetIsLoadingGamesInternal(bool value);
    void SetLoadingOverlayVisible(bool isVisible);
    void SetLoadingOverlayContent(object content);
    void SetMainContentGridEnabled(bool enabled);
    void CancelAndRecreateToken();
    Task ResetUiAsync();
    UpdateStatusBar.IUpdateStatusBar UpdateStatusBarService { get; }
}
