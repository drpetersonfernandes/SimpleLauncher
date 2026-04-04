using System.Threading.Tasks;
using SimpleLauncher.Services.GameLauncher.Models;

namespace SimpleLauncher.Services.GameLauncher;

public interface ILaunchStrategy
{
    // Priority allows us to ensure "Default" runs last
    int Priority => 100;
    bool IsMatch(LaunchContext context);
    Task ExecuteAsync(LaunchContext context, GameLauncher launcher);
}