using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SimpleLauncher.Avalonia.Views;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaMessageDialogService : IMessageDialogService
{
    private static Window? GetMainWindow()
    {
        return global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }

    private static async Task<MessageBoxResult> ShowDialogAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var tcs = new TaskCompletionSource<MessageBoxResult>();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null)
            {
                tcs.TrySetResult(MessageBoxResult.None);
                return;
            }

            var dialog = new MessageDialog
            {
                Title = string.IsNullOrEmpty(title) ? "SimpleLauncher" : title,
                Message = message,
                IconGlyph = MessageDialog.GetIconGlyph(icon),
                ShowIcon = icon != MessageBoxImage.None,
                ShowOkCancel = buttons is MessageBoxButton.Ok or MessageBoxButton.OkCancel,
                ShowYesNo = buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel
            };

            switch (buttons)
            {
                case MessageBoxButton.Ok:
                    dialog.ShowOkCancel = true;
                    break;
                case MessageBoxButton.YesNo:
                    dialog.ShowYesNo = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    dialog.ShowYesNo = true;
                    break;
            }

            await dialog.ShowDialog(mainWindow);
            tcs.TrySetResult(dialog.Result);
        });

        return await tcs.Task;
    }

    public async Task ShowInfoAsync(string message, string title = "")
    {
        await ShowDialogAsync(message, title, MessageBoxButton.Ok, MessageBoxImage.Information);
    }

    public async Task ShowWarningAsync(string message, string title = "")
    {
        await ShowDialogAsync(message, title, MessageBoxButton.Ok, MessageBoxImage.Warning);
    }

    public async Task ShowErrorAsync(string message, string title = "")
    {
        await ShowDialogAsync(message, title, MessageBoxButton.Ok, MessageBoxImage.Error);
    }

    public async Task<bool> ShowConfirmAsync(string message, string title = "")
    {
        var result = await ShowDialogAsync(message, title, MessageBoxButton.OkCancel, MessageBoxImage.Question);
        return result == MessageBoxResult.Ok;
    }

    public async Task<bool> ShowYesNoAsync(string message, string title = "")
    {
        var result = await ShowDialogAsync(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public async Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        return await ShowDialogAsync(message, title, buttons, icon);
    }
}
