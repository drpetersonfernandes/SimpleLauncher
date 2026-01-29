using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScan;

/// <summary>
/// A utility class to extract icons from executable files.
/// </summary>
public static class IconExtractor
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    /// <summary>
    /// Extracts the first icon from an executable and saves it as a PNG file.
    /// </summary>
    /// <param name="exePath">The path to the executable file.</param>
    /// <param name="savePath">The path where the PNG icon should be saved.</param>
    public static void SaveIconFromExe(string exePath, string savePath)
    {
        if (!File.Exists(exePath) || !savePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return;

        var hIcon = IntPtr.Zero;
        try
        {
            hIcon = ExtractIcon(IntPtr.Zero, exePath, 0);
            if (hIcon != IntPtr.Zero)
            {
                using var icon = Icon.FromHandle(hIcon);
                using var bmp = icon.ToBitmap();
                bmp.Save(savePath, ImageFormat.Png);
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to extract icon from {exePath}");
        }
        finally
        {
            // According to documentation, do not call DestroyIcon on an icon retrieved by ExtractIcon.
            // However, some sources suggest it's necessary to avoid leaks.
            // A check for non-zero handle is a safe practice.
            if (hIcon != IntPtr.Zero)
            {
                DestroyIcon(hIcon);
            }
        }
    }
}