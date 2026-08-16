using System.Windows;

namespace SimpleLauncher.Services.NotificationToast;

/// <summary>
/// Shows toast notifications on the UI thread through a single reusable
/// <see cref="ToastNotificationWindow"/> instance.
/// </summary>
public class ToastNotificationService : IToastNotificationService
{
    private readonly ILogger _logger;
    private ToastNotificationWindow? _toastWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastNotificationService"/> class.
    /// </summary>
    /// <param name="logErrors">The logger instance used for error logging.</param>
    public ToastNotificationService(ILogger logErrors)
    {
        _logger = logErrors;
    }

    /// <summary>
    /// Shows a toast notification with the given title and message.
    /// Never blocks the calling thread: when invoked from a background thread the
    /// toast is dispatched asynchronously to the UI thread (fire-and-forget).
    /// </summary>
    public void ShowToast(string title, string message)
    {
        try
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                ShowToastCore(title, message);
            }
            else
            {
                _ = dispatcher.BeginInvoke(() => ShowToastCore(title, message));
            }
        }
        catch (Exception ex)
        {
            _logger.Debug($"[Toast] Failed to show notification: {ex.Message}");
        }
    }

    private void ShowToastCore(string title, string message)
    {
        _toastWindow ??= new ToastNotificationWindow();
        _toastWindow.ShowToast(title, message);
    }
}