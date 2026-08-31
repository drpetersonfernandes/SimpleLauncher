namespace SimpleLauncher.Avalonia.Interfaces;

/// <summary>
///     UI surface the status bar service drives.
/// </summary>
public interface IAvaloniaStatusBarHost
{
    /// <summary>Updates the status bar text content.</summary>
    void SetStatusText(string text);
}
