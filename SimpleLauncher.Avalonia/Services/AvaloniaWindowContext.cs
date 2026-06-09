using Avalonia.Controls;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaWindowContext : IWindowContext
{
    private readonly Window _window;

    public AvaloniaWindowContext(Window window, IDispatcherService dispatcher)
    {
        _window = window;
        Dispatcher = dispatcher;
    }

    public IntPtr Handle
    {
        get
        {
            if (_window.TryGetPlatformHandle()?.Handle is { } handle)
            {
                return handle;
            }

            return IntPtr.Zero;
        }
    }

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

    public static AvaloniaWindowContext FromMainWindow(Window window)
    {
        return new AvaloniaWindowContext(window, new AvaloniaDispatcherService());
    }
}
