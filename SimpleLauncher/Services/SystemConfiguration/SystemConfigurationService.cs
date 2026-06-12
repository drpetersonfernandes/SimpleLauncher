using Microsoft.Extensions.Configuration;

namespace SimpleLauncher.Services.SystemConfiguration;

using Interfaces;

/// <summary>
/// Loads system manager configurations from the application configuration source.
/// </summary>
public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly IConfiguration _configuration;

    // ReSharper disable once NotAccessedField.Local
    private readonly ILogErrors _logErrors;

    /// <summary>
    /// Initializes a new instance of the SystemConfigurationService with the specified dependencies.
    /// </summary>
    public SystemConfigurationService(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
    }

    /// <summary>
    /// Loads and returns the list of configured system managers.
    /// </summary>
    public List<SystemManager.SystemManager> LoadSystemManagers()
    {
        return SystemManager.SystemManager.LoadSystemManagers(_configuration);
    }
}
