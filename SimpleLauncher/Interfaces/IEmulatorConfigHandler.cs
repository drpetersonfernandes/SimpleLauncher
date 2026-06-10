using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IEmulatorConfigHandler
{
    bool IsMatch(string emulatorName, string emulatorPath);
    Task<bool> HandleConfigurationAsync(LaunchContext context);
}
