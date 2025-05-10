using System;
using System.Runtime.InteropServices;

namespace SimpleLauncher.Services;

public static partial class WindowScreenshot
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(IntPtr hWnd, out Rectangle lpRectangle);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetClientRect(IntPtr hWnd, out Rectangle lpRectangle);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    /// <summary>
    /// Gets the rectangle of the client area (excluding borders and menu).
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <param name="clientRectangle">The rectangle of the client area in screen coordinates.</param>
    /// <returns>True if the client area was successfully retrieved, false otherwise.</returns>
    public static bool GetClientAreaRect(IntPtr hWnd, out Rectangle clientRectangle)
    {
        clientRectangle = new Rectangle();

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
        clientRectangle.Left = clientTopLeft.X;
        clientRectangle.Top = clientTopLeft.Y;
        clientRectangle.Right = clientTopLeft.X + (localClientRect.Right - localClientRect.Left);
        clientRectangle.Bottom = clientTopLeft.Y + (localClientRect.Bottom - localClientRect.Top);

        return true;
    }
}