using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Core.Interfaces;

public interface ISystemConfigurationService
{
    List<ISystemManager> LoadSystemManagers();
}
