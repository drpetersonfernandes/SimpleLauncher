using System.Runtime.InteropServices;

namespace SimpleLauncher.Models;

public static partial class WindowScreenshot
{
    /// <summary>
    /// Represents a rectangle with Left, Top, Right, and Bottom coordinates,
    /// matching the native RECT struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        /// <summary>
        /// The X coordinate of the left edge.
        /// </summary>
        public int Left;

        /// <summary>
        /// The Y coordinate of the top edge.
        /// </summary>
        public int Top;

        /// <summary>
        /// The X coordinate of the right edge.
        /// </summary>
        public int Right;

        /// <summary>
        /// The Y coordinate of the bottom edge.
        /// </summary>
        public int Bottom;
    }
}
