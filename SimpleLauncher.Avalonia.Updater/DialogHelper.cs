using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SimpleLauncher.Avalonia.Updater;

/// <summary>
/// Minimal message-dialog helper (code-built; the updater does not reference the main app's UI).
/// </summary>
internal static class DialogHelper
{
    /// <summary>
    /// Shows a modal message box with an OK button.
    /// </summary>
    public static async Task ShowMessageAsync(Window owner, string message, string title)
    {
        await ShowDialogAsync(owner, message, title, yesNo: false);
    }

    /// <summary>
    /// Shows a modal Yes/No dialog and returns the user's choice.
    /// </summary>
    public static async Task<bool> ShowYesNoAsync(Window owner, string message, string title)
    {
        return await ShowDialogAsync(owner, message, title, yesNo: true);
    }

    private static async Task<bool> ShowDialogAsync(Window owner, string message, string title, bool yesNo)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        if (yesNo)
        {
            var yesButton = new Button { Content = "Yes", Width = 90, IsDefault = true };
            yesButton.Click += (_, _) =>
            {
                tcs.TrySetResult(true);
            };
            var noButton = new Button { Content = "No", Width = 90, IsCancel = true };
            noButton.Click += (_, _) =>
            {
                tcs.TrySetResult(false);
            };
            buttons.Children.Add(yesButton);
            buttons.Children.Add(noButton);
        }
        else
        {
            var okButton = new Button { Content = "OK", Width = 90, IsDefault = true };
            okButton.Click += (_, _) =>
            {
                tcs.TrySetResult(false);
            };
            buttons.Children.Add(okButton);
        }

        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    buttons
                }
            }
        };

        // Closing via the title-bar X must not leave the await pending forever.
        dialog.Closed += (_, _) => tcs.TrySetResult(false);

        await dialog.ShowDialog(owner);

        return await tcs.Task;
    }
}
