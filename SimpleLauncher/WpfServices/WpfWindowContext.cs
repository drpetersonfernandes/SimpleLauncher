using System.Windows;
using System.Windows.Interop;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfWindowContext : IWindowContext
{
    private readonly Window _window;

    public WpfWindowContext(Window window, IDispatcherService dispatcher)
    {
        _window = window;
        Handle = new WindowInteropHelper(window).Handle;
        Dispatcher = dispatcher;
    }

    public IntPtr Handle { get; }

    public void Show()
    {
        _window.Show();
    }

    public void Hide()
    {
        _window.Hide();
    }

    public void Activate()
    {
        _window.Activate();
    }

    public IDispatcherService Dispatcher { get; }

    public object PlatformWindow => _window;

    public static WpfWindowContext FromMainWindow(Window window)
    {
        return new WpfWindowContext(window, new WpfDispatcherService());
    }
}
