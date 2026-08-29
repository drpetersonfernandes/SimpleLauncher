using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides access to the configured system definitions.
/// </summary>
public interface ISystemConfigurationService
{
    /// <summary>
    ///     Loads and returns the list of configured system managers.
    /// </summary>
    /// <returns>The list of configured system managers.</returns>
    IList<SystemManagerService> LoadSystemManagers();

    /// <summary>
    ///     Asynchronously loads and returns the list of configured system managers.
    /// </summary>
    /// <returns>The list of configured system managers.</returns>
    Task<IList<SystemManagerService>> LoadSystemManagersAsync();
}