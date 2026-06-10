using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface ILaunchStrategy
{
    int Priority => 100;
    bool IsMatch(LaunchContext context);
    Task ExecuteAsync(LaunchContext context, ILauncherService launcher);
}
