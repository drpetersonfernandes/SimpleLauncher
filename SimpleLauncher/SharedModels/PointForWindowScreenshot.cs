using System.Runtime.InteropServices;

namespace SimpleLauncher.SharedModels;

public static partial class WindowScreenshot
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }
}