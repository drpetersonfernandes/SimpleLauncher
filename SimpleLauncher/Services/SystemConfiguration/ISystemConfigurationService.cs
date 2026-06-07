namespace SimpleLauncher.Services.SystemConfiguration;

public interface ISystemConfigurationService
{
    List<SystemManager.SystemManager> LoadSystemManagers();
}
