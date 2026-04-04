using System.Threading.Tasks;
using SimpleLauncher.Services.GameLauncher.Models;

namespace SimpleLauncher.Services.GameLauncher;

public interface IEmulatorConfigHandler
{
    bool IsMatch(string emulatorName, string emulatorPath);
    Task<bool> HandleConfigurationAsync(LaunchContext context);
}