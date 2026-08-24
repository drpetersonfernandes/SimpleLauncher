namespace SimpleLauncher.Avalonia.Services.GameLauncher;

/// <summary>
/// Optional launch-feedback surface implemented by the host UI (WPF
/// GameLauncherService used IToastNotificationService + IUpdateStatusBar;
/// this is the Avalonia equivalent). The launcher checks whether its
/// <see cref="SimpleLauncher.Core.Interfaces.ILoadingState"/> provider also
/// implements this interface and, when it does, emits launch and playtime
/// toasts/status updates instead of staying silent.
/// </summary>
public interface ILaunchFeedback
{
    /// <summary>Shows a toast notification with the given title and message.</summary>
    void ShowToast(string title, string message);

    /// <summary>Sets the status bar text (empty string clears it).</summary>
    void SetStatusText(string text);
}