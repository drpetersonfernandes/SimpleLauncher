namespace SimpleLauncher.Interfaces;

public interface IUpdateStatusBar
{
    void Initialize(IStatusBarHost host);
    void UpdateContent(string content);
}
