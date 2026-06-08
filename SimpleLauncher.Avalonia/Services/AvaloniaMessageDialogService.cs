using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaMessageDialogService : IMessageDialogService
{
    public Task ShowInfoAsync(string message, string title = "")
    {
        // TODO: Implement with Avalonia dialog
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string message, string title = "")
    {
        // TODO: Implement with Avalonia dialog
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string message, string title = "")
    {
        // TODO: Implement with Avalonia dialog
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string message, string title = "")
    {
        // TODO: Implement with Avalonia dialog
        return Task.FromResult(true);
    }

    public Task<bool> ShowYesNoAsync(string message, string title = "")
    {
        // TODO: Implement with Avalonia dialog
        return Task.FromResult(true);
    }

    public Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        // TODO: Implement with Avalonia dialog
        return Task.FromResult(MessageBoxResult.Ok);
    }
}
