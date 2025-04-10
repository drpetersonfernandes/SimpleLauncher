using System;
using System.Runtime.InteropServices;

namespace SimpleLauncher;

public static partial class WindowScreenshot
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetClientRect(IntPtr hWnd, out Rect lpRect);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// Gets the rectangle of the client area (excluding borders and menu).
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <param name="clientRect">The rectangle of the client area in screen coordinates.</param>
    /// <returns>True if the client area was successfully retrieved, false otherwise.</returns>
    public static bool GetClientAreaRect(IntPtr hWnd, out Rect clientRect)
    {
        clientRect = new Rect();

        // Get the client area dimensions
        if (!GetClientRect(hWnd, out var localClientRect))
        {
            return false;
        }

        // Get the top-left corner of the client area in screen coordinates
        var clientTopLeft = new Point { X = localClientRect.Left, Y = localClientRect.Top };
        if (!ClientToScreen(hWnd, ref clientTopLeft))
        {
            return false;
        }

        // Calculate the client area rectangle in screen coordinates
        clientRect.Left = clientTopLeft.X;
        clientRect.Top = clientTopLeft.Y;
        clientRect.Right = clientTopLeft.X + (localClientRect.Right - localClientRect.Left);
        clientRect.Bottom = clientTopLeft.Y + (localClientRect.Bottom - localClientRect.Top);

        return true;
    }
}