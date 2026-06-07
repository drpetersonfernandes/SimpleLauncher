using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.SystemConfiguration;

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly IConfiguration _configuration;

    // ReSharper disable once NotAccessedField.Local
    private readonly ILogErrors _logErrors;

    public SystemConfigurationService(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
    }

    public List<SystemManager.SystemManager> LoadSystemManagers()
    {
        return SystemManager.SystemManager.LoadSystemManagers(_configuration);
    }
}
