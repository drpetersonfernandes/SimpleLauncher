using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface ISystemConfigurationService
{
    IList<SystemManagerService> LoadSystemManagers();
}
