namespace SimpleLauncher.WpfServices;

public class WpfMessageDialogService : Core.Interfaces.IMessageDialogService
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

    public Task<Core.Interfaces.MessageBoxResult> ShowAsync(string message, string title, Core.Interfaces.MessageBoxButton buttons, Core.Interfaces.MessageBoxImage icon)
    {
        var wpfButtons = (int)buttons;
        var wpfIcon = (int)icon;

        var result = ShowMessageBox(message, title, wpfButtons, wpfIcon);

        var mappedResult = result switch
        {
            1 => Core.Interfaces.MessageBoxResult.Ok,
            2 => Core.Interfaces.MessageBoxResult.Cancel,
            6 => Core.Interfaces.MessageBoxResult.Yes,
            7 => Core.Interfaces.MessageBoxResult.No,
            _ => Core.Interfaces.MessageBoxResult.None
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
