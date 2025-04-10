using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SimpleLauncher;

public static partial class WindowManager
{
    // Delegate for the EnumWindows callback
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

        // Allocate buffer for the window title
        Span<char> buffer = stackalloc char[length + 1];
        unsafe
        {
            fixed (char* bufferPtr = buffer)
            {
                var result = GetWindowText(hWnd, bufferPtr, buffer.Length);
                if (result == 0)
                {
                    return string.Empty;
                }
            }
        }

        return new string(buffer.Slice(0, length));
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static unsafe partial int GetWindowText(IntPtr hWnd, char* lpString, int nMaxCount);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetWindowTextLength(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);
}