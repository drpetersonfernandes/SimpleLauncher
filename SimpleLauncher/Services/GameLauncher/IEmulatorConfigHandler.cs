using System.Threading.Tasks;

namespace SimpleLauncher.Services.GameLauncher;

public interface IEmulatorConfigHandler
{
    bool IsMatch(string emulatorName, string emulatorPath);
    Task<bool> HandleConfigurationAsync(LaunchContext context);
}