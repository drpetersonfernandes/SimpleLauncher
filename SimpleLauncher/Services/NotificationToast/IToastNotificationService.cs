namespace SimpleLauncher.Services.NotificationToast;

/// <summary>
///     Displays non-blocking toast notifications in the main application window area.
/// </summary>
public interface IToastNotificationService
{
    /// <summary>
    ///     Shows a toast notification with the given title and message.
    /// </summary>
    /// <param name="title">The toast title.</param>
    /// <param name="message">The toast message.</param>
    void ShowToast(string title, string message);
}