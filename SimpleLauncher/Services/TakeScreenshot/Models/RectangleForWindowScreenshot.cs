using System.Runtime.InteropServices;

namespace SimpleLauncher.Services.TakeScreenshot.Models;

public static partial class WindowScreenshot
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}