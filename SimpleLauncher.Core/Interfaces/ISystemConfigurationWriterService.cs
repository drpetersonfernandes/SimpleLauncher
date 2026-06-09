using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Core.Interfaces;

public interface ISystemConfigurationWriterService
{
    Task SaveSystemAsync(ISystemManager systemConfig, string? originalSystemName = null);
    Task DeleteSystemAsync(string systemName);
    bool SystemExists(string systemName);
}
