using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

public interface IEmulatorConfigHandler
{
    bool IsMatch(string emulatorName, string emulatorPath);
    Task<bool> HandleConfigurationAsync(LaunchContext context);
}
