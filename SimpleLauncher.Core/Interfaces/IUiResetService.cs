namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Resets the application UI to its default state.
/// </summary>
public interface IUiResetService
{
    /// <summary>
    /// Initializes the service with the specified UI host.
    /// </summary>
    /// <param name="host">The host providing access to the UI state and elements.</param>
    void Initialize(IUiResetHost host);

    /// <summary>
    /// Asynchronously resets the UI, clearing all filters, selections, and returning to the system selection screen.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetUiAsync();
}
