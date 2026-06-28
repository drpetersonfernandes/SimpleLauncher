using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SimpleLauncher.Services.TakeScreenshot;

using Interfaces;

/// <summary>
/// Registers a system-wide F8 hotkey and raises an event when it is pressed.
/// Uses Win32 RegisterHotKey/UnregisterHotKey via a hidden HwndSource message hook.
/// </summary>
public partial class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 9001;
    private const uint VkF8 = 0x77;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly IDebugLogger _debugLogger;
    private readonly ILogErrors _logErrors;
    private HwndSource _hwndSource;
    private IntPtr _windowHandle;
    private bool _isDisposed;

    /// <summary>
    /// Raised when the F8 global hotkey is pressed.
    /// </summary>
    public event Func<Task> F8Pressed;

    public GlobalHotkeyService(IDebugLogger debugLogger, ILogErrors logErrors)
    {
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
    }

    /// <summary>
    /// Initializes the hotkey service by obtaining the window handle and registering F8.
    /// Must be called from the UI thread after the main window is loaded.
    /// </summary>
    /// <param name="window">The main application window used to obtain the HwndSource.</param>
    public void Initialize(Window window)
    {
        if (_isDisposed) return;

        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.Handle;
        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource?.AddHook(WndProc);

        if (!RegisterHotKey(_windowHandle, HotkeyId, 0, VkF8))
        {
            var error = Marshal.GetLastWin32Error();
            _debugLogger.Log($"[GlobalHotkeyService] Failed to register F8 hotkey. Win32 error code: {error}");
            _logErrors.LogAndForget(null, $"[GlobalHotkeyService] Failed to register F8 hotkey. Win32 error code: {error}");
        }
        else
        {
            _debugLogger.Log("[GlobalHotkeyService] F8 hotkey registered successfully.");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            var handler = F8Pressed;
            if (handler != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await handler();
                    }
                    catch (Exception ex)
                    {
                        _logErrors.LogAndForget(ex, "[GlobalHotkeyService] Error invoking F8Pressed event.");
                    }
                });
            }

            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;

        try
        {
            if (_windowHandle != IntPtr.Zero)
            {
                _ = UnregisterHotKey(_windowHandle, HotkeyId);
                _debugLogger.Log("[GlobalHotkeyService] F8 hotkey unregistered.");
            }
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[GlobalHotkeyService] Error unregistering hotkey: {ex.Message}");
        }

        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;

        GC.SuppressFinalize(this);
    }
}
