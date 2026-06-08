using System.Runtime.InteropServices;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Contains native interop structures for window screenshot capture.
/// </summary>
public static partial class WindowScreenshot
{
    /// <summary>
    /// Represents a point with X and Y coordinates, matching the native POINT struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y;
    }
}