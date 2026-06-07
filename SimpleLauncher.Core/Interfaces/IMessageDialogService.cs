namespace SimpleLauncher.Core.Interfaces;

public enum MessageBoxResult
{
    None = 0,
    Ok = 1,
    Cancel = 2,
    Yes = 6,
    No = 7
}

public enum MessageBoxButton
{
    Ok = 0,
    OkCancel = 1,
    YesNo = 4,
    YesNoCancel = 3
}

public enum MessageBoxImage
{
    None = 0,
    Error = 16,
    Warning = 48,
    Information = 64,
    Question = 32
}

public interface IMessageDialogService
{
    Task ShowInfoAsync(string message, string title = "");
    Task ShowWarningAsync(string message, string title = "");
    Task ShowErrorAsync(string message, string title = "");
    Task<bool> ShowConfirmAsync(string message, string title = "");
    Task<bool> ShowYesNoAsync(string message, string title = "");
    Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
}
