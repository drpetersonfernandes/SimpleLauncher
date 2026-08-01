namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to control the application lifecycle, such as shutting down or restarting.
/// </summary>
public interface IApplicationLifetime
{
    /// <summary>
    /// Shuts down the application.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Restarts the application.
    /// </summary>
    void Restart();
}
