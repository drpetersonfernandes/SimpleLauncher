using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface ISystemConfigurationService
{
    List<SystemManager> LoadSystemManagers();
}
