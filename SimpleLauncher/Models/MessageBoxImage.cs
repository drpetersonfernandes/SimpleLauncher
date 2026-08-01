namespace SimpleLauncher.Models;

/// <summary>
/// Specifies the icon to display in a message box.
/// </summary>
public enum MessageBoxImage
{
    /// <summary>
    /// Displays no icon.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays an error icon.
    /// </summary>
    Error = 16,

    /// <summary>
    /// Displays a warning icon.
    /// </summary>
    Warning = 48,

    /// <summary>
    /// Displays an information icon.
    /// </summary>
    Information = 64,

    /// <summary>
    /// Displays a question icon.
    /// </summary>
    Question = 32
}
