namespace SimpleLauncher.Avalonia.Interfaces;

/// <summary>
///     UI surface the loading overlay service drives.
/// </summary>
public interface IAvaloniaLoadingOverlayHost
{
    /// <summary>Sets the IsLoading observable property.</summary>
    void SetIsLoading(bool isLoading);

    /// <summary>Sets the loading message observable property.</summary>
    void SetLoadingMessage(string message);

    /// <summary>Resets the UI to a non-loading state (clears filters, returns to main content).</summary>
    Task ResetUiAsync();

    /// <summary>Cancels any in-flight background work and recreates the cancellation token.</summary>
    void CancelAndRecreateToken();

    /// <summary>Enables or disables the main content grid.</summary>
    void SetMainContentGridEnabled(bool enabled);
}