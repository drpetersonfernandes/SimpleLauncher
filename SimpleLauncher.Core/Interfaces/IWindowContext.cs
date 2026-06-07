namespace SimpleLauncher.Core.Interfaces;

public interface IWindowContext
{
    IntPtr Handle { get; }
    void Show();
    void Hide();
    void Activate();
    IDispatcherService Dispatcher { get; }
    object PlatformWindow { get; }
}
