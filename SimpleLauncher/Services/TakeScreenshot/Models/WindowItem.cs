namespace SimpleLauncher.Services.TakeScreenshot.Models;

/// <summary>
/// Represents a visible window with its title and native handle, used for screenshot target selection.
/// </summary>
public class WindowItem
{
    /// <summary>Gets or sets the window title text.</summary>
    public string Title { get; set; }

    /// <summary>Gets the native window handle (HWND).</summary>
    public IntPtr Handle { get; init; }
}
