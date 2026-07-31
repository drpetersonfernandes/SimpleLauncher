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
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the SystemConfigurationService with the specified dependencies.
    /// </summary>
    public SystemConfigurationService(IConfiguration configuration, ILogger logErrors)
    {
        _configuration = configuration;
        _logger = logErrors;
    }

    /// <summary>
    /// Loads and returns the list of configured system managers.
    /// </summary>
    public List<SystemManager.SystemManager> LoadSystemManagers()
    {
        return SystemManager.SystemManager.LoadSystemManagers(_configuration);
    }
}
