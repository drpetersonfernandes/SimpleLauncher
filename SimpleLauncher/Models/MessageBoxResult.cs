namespace SimpleLauncher.Models;

/// <summary>
/// Specifies which button was clicked on a message box.
/// </summary>
public enum MessageBoxResult
{
    /// <summary>
    /// Indicates that no button was clicked.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that the OK button was clicked.
    /// </summary>
    Ok = 1,

    /// <summary>
    /// Indicates that the Cancel button was clicked.
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// Indicates that the Yes button was clicked.
    /// </summary>
    Yes = 6,

    /// <summary>
    /// Indicates that the No button was clicked.
    /// </summary>
    No = 7
}
