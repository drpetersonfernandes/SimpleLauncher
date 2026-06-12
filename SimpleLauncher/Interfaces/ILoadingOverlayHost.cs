using System.Windows.Threading;

namespace SimpleLauncher.Interfaces;

public interface ILoadingOverlayHost
{
    Dispatcher Dispatcher { get; }
    void SetIsLoadingGamesInternal(bool value);
    void SetLoadingOverlayVisible(bool isVisible);
    void SetLoadingOverlayContent(object content);
    void SetMainContentGridEnabled(bool enabled);
    void CancelAndRecreateToken();
    Task ResetUiAsync();
    IUpdateStatusBar UpdateStatusBarService { get; }
}
