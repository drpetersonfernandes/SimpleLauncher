using System.Windows;
using SimpleLauncher.Interfaces;
using MessageBoxButton = SimpleLauncher.Interfaces.MessageBoxButton;
using MessageBoxImage = SimpleLauncher.Interfaces.MessageBoxImage;
using MessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

namespace SimpleLauncher.WpfServices;

/// <summary>
/// WPF implementation of IMessageDialogService, displaying message boxes via the WPF dispatcher.
/// </summary>
public class WpfMessageDialogService : IMessageDialogService
{
    /// <summary>Displays an informational message box.</summary>
    public Task ShowInfoAsync(string message, string title = "")
    {
        const System.Windows.MessageBoxButton wpfButtons = (int)MessageBoxButton.Ok;
        const System.Windows.MessageBoxImage wpfIcon = (System.Windows.MessageBoxImage)(int)MessageBoxImage.Information;

        Application.Current.Dispatcher.InvokeAsync(() =>
            MessageBox.Show(message, title, wpfButtons, wpfIcon));

        return Task.CompletedTask;
    }

    /// <summary>Displays a warning message box.</summary>
    public Task ShowWarningAsync(string message, string title = "")
    {
        const System.Windows.MessageBoxButton wpfButtons = (int)MessageBoxButton.Ok;
        const System.Windows.MessageBoxImage wpfIcon = (System.Windows.MessageBoxImage)(int)MessageBoxImage.Warning;

        Application.Current.Dispatcher.InvokeAsync(() =>
            MessageBox.Show(message, title, wpfButtons, wpfIcon));

        return Task.CompletedTask;
    }

    /// <summary>Displays an error message box.</summary>
    public Task ShowErrorAsync(string message, string title = "")
    {
        const System.Windows.MessageBoxButton wpfButtons = (int)MessageBoxButton.Ok;
        const System.Windows.MessageBoxImage wpfIcon = (System.Windows.MessageBoxImage)(int)MessageBoxImage.Error;

        Application.Current.Dispatcher.InvokeAsync(() =>
            MessageBox.Show(message, title, wpfButtons, wpfIcon));

        return Task.CompletedTask;
    }

    /// <summary>Displays a confirmation dialog with OK/Cancel buttons, returning true if OK is clicked.</summary>
    public Task<bool> ShowConfirmAsync(string message, string title = "")
    {
        var result = ShowMessageBox(message, title, MessageBoxButton.OkCancel, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Ok);
    }

    /// <summary>Displays a Yes/No dialog, returning true if Yes is clicked.</summary>
    public Task<bool> ShowYesNoAsync(string message, string title = "")
    {
        var result = ShowMessageBox(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    /// <summary>Displays a message box with the specified buttons and icon, returning the user's choice.</summary>
    public Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var result = ShowMessageBox(message, title, buttons, icon);
        return Task.FromResult(result);
    }

    private static MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var wpfButtons = (System.Windows.MessageBoxButton)(int)buttons;
        var wpfIcon = (System.Windows.MessageBoxImage)(int)icon;

        var wpfResult = Application.Current.Dispatcher.Invoke(() =>
            MessageBox.Show(message, title, wpfButtons, wpfIcon));

        return (MessageBoxResult)(int)wpfResult;
    }
}
