namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Abstracts the platform window used to launch games, providing window state control and handle access.
/// </summary>
public interface IWindowContext
{
    /// <summary>
    ///     Gets the native window handle (HWND).
    /// </summary>
    IntPtr Handle { get; }

    /// <summary>
    ///     Gets the dispatcher service for UI thread operations.
    /// </summary>
    IDispatcherService Dispatcher { get; }

    /// <summary>
    ///     Gets the underlying platform window object.
    /// </summary>
    object PlatformWindow { get; }

    /// <summary>
    ///     Shows the window.
    /// </summary>
    void Show();

    /// <summary>
    ///     Hides the window.
    /// </summary>
    void Hide();

    /// <summary>
    ///     Activates the window and brings it to the foreground.
    /// </summary>
    void Activate();
}