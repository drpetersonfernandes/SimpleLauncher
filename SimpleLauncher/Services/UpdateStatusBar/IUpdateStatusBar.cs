namespace SimpleLauncher.Services.UpdateStatusBar;

public interface IUpdateStatusBar
{
    void Initialize(IStatusBarHost host);
    void UpdateContent(string content);
}
