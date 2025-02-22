using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleLauncher;

public static class WindowManager
{
    // Import native methods
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    // Delegate for the EnumWindows callback
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary>
    /// Gets the list of open windows with their handles and titles.
    /// </summary>
    public static List<(IntPtr Handle, string Title)> GetOpenWindows()
    {
        var windows = new List<(IntPtr, string)>();

        // The callback processes each window handle.
        EnumWindows(WindowEnumCallback, (IntPtr)GCHandle.Alloc(windows));
        return windows;
    }

    /// <summary>
    /// Callback method for window enumeration.
    /// </summary>
    private static bool WindowEnumCallback(IntPtr hWnd, IntPtr lParam)
    {
        if (!IsWindowVisible(hWnd))
            return true;

        var title = GetWindowTitle(hWnd);
        if (string.IsNullOrEmpty(title))
            return true;

        // Retrieve the list from the GCHandle.
        var handle = GCHandle.FromIntPtr(lParam);
        if (handle.Target is List<(IntPtr, string)> windows)
        {
            windows.Add((hWnd, title));
        }

        return true;
    }

    /// <summary>
    /// Retrieves the title of the given window handle.
    /// </summary>
    private static string GetWindowTitle(IntPtr hWnd)
    {
        var length = GetWindowTextLength(hWnd);
        if (length <= 0)
            return string.Empty;

        var builder = new StringBuilder(length + 1);
        var result = GetWindowText(hWnd, builder, builder.Capacity);
        if (result == 0)
        {
            // Optionally log an error or handle the failure
            return string.Empty;
        }

        return builder.ToString();
    }
}