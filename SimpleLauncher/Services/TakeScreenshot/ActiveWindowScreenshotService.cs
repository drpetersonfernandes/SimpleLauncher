using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Services.TakeScreenshot;

using Interfaces;

/// <summary>
/// Captures a screenshot of the currently active (foreground) window and saves it
/// to the .\screenshot folder relative to the application directory.
/// </summary>
public partial class ActiveWindowScreenshotService
{
    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    private readonly IDebugLogger _debugLogger;
    private readonly ILogErrors _logErrors;
    private readonly IPlaySoundEffects _playSoundEffects;
    private readonly IServiceProvider _serviceProvider;

    public ActiveWindowScreenshotService(
        IDebugLogger debugLogger,
        ILogErrors logErrors,
        IPlaySoundEffects playSoundEffects,
        IServiceProvider serviceProvider)
    {
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Captures a screenshot of the current foreground window and saves it as a PNG file
    /// in the .\screenshot directory (relative to the application base directory).
    /// </summary>
    public Task CaptureActiveWindowAsync()
    {
        try
        {
            try
            {
                var hWnd = GetForegroundWindow();
                if (hWnd == IntPtr.Zero)
                {
                    _debugLogger.Log("[ActiveWindowScreenshot] No foreground window found.");
                    return Task.CompletedTask;
                }

                SimpleLauncher.Models.WindowScreenshot.Rectangle rectangle;

                if (!WindowScreenshot.GetClientAreaRect(hWnd, out var clientRect))
                {
                    if (!WindowScreenshot.GetWindowRect(hWnd, out rectangle))
                    {
                        _debugLogger.Log("[ActiveWindowScreenshot] Failed to retrieve window dimensions.");
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    rectangle = clientRect;
                }

                var width = rectangle.Right - rectangle.Left;
                var height = rectangle.Bottom - rectangle.Top;

                if (width <= 0 || height <= 0)
                {
                    _debugLogger.Log("[ActiveWindowScreenshot] Cannot take a screenshot of a minimized or zero-size window.");
                    return Task.CompletedTask;
                }

                var screenshotDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshot");
                Directory.CreateDirectory(screenshotDir);

                var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
                var screenshotPath = Path.Combine(screenshotDir, fileName);

                using (var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(
                            new System.Drawing.Point(rectangle.Left, rectangle.Top),
                            System.Drawing.Point.Empty,
                            new System.Drawing.Size(width, height));
                    }

                    bitmap.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                _debugLogger.Log($"[ActiveWindowScreenshot] Screenshot saved: {screenshotPath}");

                PlaySoundAndFlash();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "[ActiveWindowScreenshot] Error capturing the active window screenshot.");
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    private void PlaySoundAndFlash()
    {
        try
        {
            _playSoundEffects.PlayShutterSound();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var flashWindow = _serviceProvider.GetRequiredService<FlashOverlayWindow>();
                        return flashWindow.ShowFlashAsync();
                    });
                }
                catch (Exception ex)
                {
                    _logErrors.LogAndForget(ex, "[ActiveWindowScreenshot] Error showing flash overlay.");
                }
            });
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "[ActiveWindowScreenshot] Error playing shutter sound or flash.");
        }
    }
}
