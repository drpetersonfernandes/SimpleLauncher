namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Manages the loading state of the UI, showing or hiding the loading overlay.
/// </summary>
public interface ILoadingState
{
    /// <summary>
    ///     Toggles the loading overlay and optionally disables user interaction on the window.
    /// </summary>
    /// <param name="isLoading">True to show the overlay, false to hide it.</param>
    /// <param name="message">The message to display on the overlay.</param>
    void SetLoadingState(bool isLoading, string? message = null);
}