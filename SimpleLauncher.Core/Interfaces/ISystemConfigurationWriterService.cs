namespace SimpleLauncher.Core.Interfaces;

public interface ISystemConfigurationWriterService
{
    Task SaveSystemAsync(ISystemManager systemConfig, string originalSystemName = null);
    Task DeleteSystemAsync(string systemName);
    bool SystemExists(string systemName);
}
