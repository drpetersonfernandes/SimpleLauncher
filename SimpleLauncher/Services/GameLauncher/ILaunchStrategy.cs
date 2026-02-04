using System.Threading.Tasks;

namespace SimpleLauncher.Services.GameLauncher;

public interface ILaunchStrategy
{
    // Priority allows us to ensure "Default" runs last
    int Priority => 100;
    bool IsMatch(LaunchContext context);
    Task ExecuteAsync(LaunchContext context, GameLauncher launcher);
}