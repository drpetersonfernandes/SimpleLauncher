using System.Windows;
using System.Windows.Interop;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfWindowContext : IWindowContext
{
    private readonly Window _window;

    public WpfWindowContext(Window window)
    {
        _window = window;
    }

    public IntPtr Handle => new WindowInteropHelper(_window).Handle;

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
}
