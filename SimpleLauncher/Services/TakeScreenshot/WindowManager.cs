using System.Runtime.InteropServices;

namespace SimpleLauncher.Services.TakeScreenshot;

/// <summary>
/// Provides methods to enumerate visible open windows using Win32 API calls.
/// </summary>
public static class WindowManager
{
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, char[] lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary>
    /// Returns a list of all visible open windows with their handles and titles.
    /// </summary>
    public static List<(IntPtr Handle, string Title)> GetOpenWindows()
    {
        var windows = new List<(IntPtr, string)>();

        EnumWindows((hWnd, _) =>
        {
            if (!IsWindowVisible(hWnd)) return true;

            var length = GetWindowTextLength(hWnd);
            if (length <= 0) return true;

            var buffer = new char[length + 1];
            var result = GetWindowText(hWnd, buffer, buffer.Length);
            if (result > 0)
            {
                windows.Add((hWnd, new string(buffer, 0, result)));
            }

            return true;
        }, IntPtr.Zero);

        return windows;
    }
}
