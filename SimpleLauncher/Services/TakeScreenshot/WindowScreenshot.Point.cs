using System.Runtime.InteropServices;

namespace SimpleLauncher.Services.TakeScreenshot;

public static partial class WindowScreenshot
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }
}