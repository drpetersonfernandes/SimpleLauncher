using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfMessageDialogService : IMessageDialogService
{
    public Task ShowInfoAsync(string message, string title = "")
    {
        ShowMessageBox(message, title, MessageBoxButton.Ok, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string message, string title = "")
    {
        ShowMessageBox(message, title, MessageBoxButton.Ok, MessageBoxImage.Warning);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string message, string title = "")
    {
        ShowMessageBox(message, title, MessageBoxButton.Ok, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string message, string title = "")
    {
        var result = ShowMessageBox(message, title, MessageBoxButton.OkCancel, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Ok);
    }

    public Task<bool> ShowYesNoAsync(string message, string title = "")
    {
        var result = ShowMessageBox(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var result = ShowMessageBox(message, title, buttons, icon);
        return Task.FromResult(result);
    }

    private static MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var wpfButtons = (global::System.Windows.MessageBoxButton)(int)buttons;
        var wpfIcon = (global::System.Windows.MessageBoxImage)(int)icon;

        var wpfResult = System.Windows.Application.Current.Dispatcher.Invoke(() =>
            System.Windows.MessageBox.Show(message, title, wpfButtons, wpfIcon));

        return (MessageBoxResult)(int)wpfResult;
    }
}
