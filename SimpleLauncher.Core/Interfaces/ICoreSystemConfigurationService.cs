using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Core.Interfaces;

public interface ICoreSystemConfigurationService
{
    List<ISystemManager> LoadSystemManagers();
}
