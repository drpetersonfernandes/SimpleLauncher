namespace SimpleLauncher.Core.Models;

/// <summary>
/// Specifies the buttons to display in a message box.
/// </summary>
public enum MessageBoxButton
{
    /// <summary>
    /// Displays an OK button.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Displays OK and Cancel buttons.
    /// </summary>
    OkCancel = 1,

    /// <summary>
    /// Displays Yes and No buttons.
    /// </summary>
    YesNo = 4,

    /// <summary>
    /// Displays Yes, No, and Cancel buttons.
    /// </summary>
    YesNoCancel = 3
}