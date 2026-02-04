namespace SimpleLauncher.Services.LoadingInterface;

public interface ILoadingState
{
    /// <summary>
    /// Toggles the loading overlay and optionally disables user interaction on the window.
    /// </summary>
    /// <param name="isLoading">True to show the overlay, false to hide it.</param>
    /// <param name="message">The message to display on the overlay.</param>
    void SetLoadingState(bool isLoading, string message = null);
}