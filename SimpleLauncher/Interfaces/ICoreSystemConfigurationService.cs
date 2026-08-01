namespace SimpleLauncher.Interfaces;

public interface ICoreSystemConfigurationService
{
    IList<ISystemManager> LoadSystemManagers();
}
