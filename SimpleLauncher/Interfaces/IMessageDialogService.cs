using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IMessageDialogService
{
    Task ShowInfoAsync(string message, string title = "");
    Task ShowWarningAsync(string message, string title = "");
    Task ShowErrorAsync(string message, string title = "");
    Task<bool> ShowConfirmAsync(string message, string title = "");
    Task<bool> ShowYesNoAsync(string message, string title = "");
    Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
}
