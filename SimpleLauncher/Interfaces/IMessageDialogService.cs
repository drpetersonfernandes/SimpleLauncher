using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to display message box dialogs and retrieve the user's selection.
/// </summary>
public interface IMessageDialogService
{
    /// <summary>
    /// Displays an informational message box.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowInfoAsync(string message, string title = "");

    /// <summary>
    /// Displays a warning message box.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowWarningAsync(string message, string title = "");

    /// <summary>
    /// Displays an error message box.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowErrorAsync(string message, string title = "");

    /// <summary>
    /// Displays a confirmation dialog with OK/Cancel buttons, returning true if OK is clicked.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <returns>A task representing the asynchronous operation, resulting in true if OK was clicked; otherwise, false.</returns>
    Task<bool> ShowConfirmAsync(string message, string title = "");

    /// <summary>
    /// Displays a Yes/No dialog, returning true if Yes is clicked.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <returns>A task representing the asynchronous operation, resulting in true if Yes was clicked; otherwise, false.</returns>
    Task<bool> ShowYesNoAsync(string message, string title = "");

    /// <summary>
    /// Displays a message box with the specified buttons and icon, returning the user's choice.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <param name="buttons">The buttons to display in the message box.</param>
    /// <param name="icon">The icon to display in the message box.</param>
    /// <returns>A task representing the asynchronous operation, resulting in the user's selected <see cref="MessageBoxResult"/>.</returns>
    Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
}
