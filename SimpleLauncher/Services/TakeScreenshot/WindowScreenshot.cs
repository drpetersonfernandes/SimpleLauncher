using System;
using System.Runtime.InteropServices;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.TakeScreenshot;

public static partial class WindowScreenshot
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(IntPtr hWnd, out SharedModels.WindowScreenshot.Rectangle lpRectangle);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetClientRect(IntPtr hWnd, out SharedModels.WindowScreenshot.Rectangle lpRectangle);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref SharedModels.WindowScreenshot.Point lpPoint);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr hWnd);

    /// <summary>
    /// Gets the rectangle of the client area (excluding borders and menu).
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <param name="clientRectangle">The rectangle of the client area in screen coordinates.</param>
    /// <returns>True if the client area was successfully retrieved, false otherwise.</returns>
    public static bool GetClientAreaRect(IntPtr hWnd, out SharedModels.WindowScreenshot.Rectangle clientRectangle)
    {
        clientRectangle = new SharedModels.WindowScreenshot.Rectangle();

        // Check if the window is minimized (iconic)
        if (IsIconic(hWnd))
        {
            DebugLogger.Log($"[WindowScreenshot] Window {hWnd} is iconic (minimized). Cannot get client area.");
            return false; // Indicate failure for minimized windows
        }

        // Get the client area dimensions
        if (!GetClientRect(hWnd, out var localClientRect))
        {
            return false;
        }

        // Get the top-left corner of the client area in screen coordinates
        var clientTopLeft = new SharedModels.WindowScreenshot.Point { X = localClientRect.Left, Y = localClientRect.Top };
        if (!ClientToScreen(hWnd, ref clientTopLeft))
        {
            DebugLogger.Log($"[WindowScreenshot] ClientToScreen failed for window {hWnd}.");
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