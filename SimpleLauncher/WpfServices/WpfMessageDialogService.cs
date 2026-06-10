using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfMessageDialogService : IMessageDialogService
{
    public Task ShowInfoAsync(string message, string title = "")
    {
        ShowMessageBox(message, title, 0, 64);
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string message, string title = "")
    {
        ShowMessageBox(message, title, 0, 48);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string message, string title = "")
    {
        ShowMessageBox(message, title, 0, 16);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string message, string title = "")
    {
        var result = ShowMessageBox(message, title, 1, 32);
        return Task.FromResult(result == 1); // OK
    }

    public Task<bool> ShowYesNoAsync(string message, string title = "")
    {
        var result = ShowMessageBox(message, title, 4, 32);
        return Task.FromResult(result == 6); // Yes
    }

    public Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var wpfButtons = (int)buttons;
        var wpfIcon = (int)icon;

        var result = ShowMessageBox(message, title, wpfButtons, wpfIcon);

        var mappedResult = result switch
        {
            1 => MessageBoxResult.Ok,
            2 => MessageBoxResult.Cancel,
            6 => MessageBoxResult.Yes,
            7 => MessageBoxResult.No,
            _ => MessageBoxResult.None
        };

        return Task.FromResult(mappedResult);
    }

    private static int ShowMessageBox(string message, string title, int buttons, int icon)
    {
        return (int)System.Windows.Application.Current.Dispatcher.Invoke(() =>
            System.Windows.MessageBox.Show(message, title,
                (global::System.Windows.MessageBoxButton)buttons,
                (global::System.Windows.MessageBoxImage)icon));
    }
}
