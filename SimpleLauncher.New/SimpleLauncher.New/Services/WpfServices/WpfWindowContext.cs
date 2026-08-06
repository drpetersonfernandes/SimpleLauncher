using System.Windows;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.New.Services.WpfServices;

/// <summary>
/// WPF implementation of IWindowContext — wraps the main window.
/// </summary>
public class WpfWindowContext : IWindowContext
{
    public WpfWindowContext(IDispatcherService dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public Window? OwnerWindow { get; set; }

    private Window GetWindow()
    {
        return OwnerWindow ?? Application.Current?.MainWindow ?? throw new InvalidOperationException("No window available");
    }

    public IntPtr Handle =>
        new System.Windows.Interop.WindowInteropHelper(GetWindow()).Handle;

    public void Show()
    {
        GetWindow().Show();
    }

    public void Hide()
    {
        GetWindow().Hide();
    }

    public void Activate()
    {
        var window = GetWindow();
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Activate();
    }

    public IDispatcherService Dispatcher { get; }

    public object PlatformWindow => GetWindow();
}
