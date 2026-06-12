using System.Windows;
using System.Windows.Interop;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.WpfServices;

/// <summary>
/// WPF implementation of IWindowContext, wrapping a WPF Window to provide platform-agnostic window operations.
/// </summary>
public class WpfWindowContext : IWindowContext
{
    private readonly Window _window;

    /// <summary>Initializes a new instance of WpfWindowContext wrapping the specified WPF window.</summary>
    public WpfWindowContext(Window window, IDispatcherService dispatcher)
    {
        _window = window;
        Handle = new WindowInteropHelper(window).Handle;
        Dispatcher = dispatcher;
    }

    /// <summary>Gets the native window handle (HWND).</summary>
    public IntPtr Handle { get; }

    /// <summary>Shows the window.</summary>
    public void Show()
    {
        _window.Show();
    }

    /// <summary>Hides the window.</summary>
    public void Hide()
    {
        _window.Hide();
    }

    /// <summary>Activates the window and brings it to the foreground.</summary>
    public void Activate()
    {
        _window.Activate();
    }

    /// <summary>Gets the dispatcher service for UI thread operations.</summary>
    public IDispatcherService Dispatcher { get; }

    /// <summary>Gets the underlying WPF Window object.</summary>
    public object PlatformWindow => _window;

    /// <summary>Creates a WpfWindowContext from the application's main window.</summary>
    public static WpfWindowContext FromMainWindow(Window window)
    {
        return new WpfWindowContext(window, new WpfDispatcherService());
    }
}
