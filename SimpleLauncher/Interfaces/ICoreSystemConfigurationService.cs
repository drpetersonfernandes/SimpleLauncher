namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides a method to load the core system manager configurations.
/// </summary>
public interface ICoreSystemConfigurationService
{
    /// <summary>
    /// Loads and returns the list of configured system managers.
    /// </summary>
    /// <returns>A list of system manager instances.</returns>
    IList<ISystemManager> LoadSystemManagers();
}
