using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class DefaultLaunchStrategy : ILaunchStrategy
{
    public int Priority => 999;

    public bool IsMatch(LaunchContext context)
    {
        return true;
    }

    public Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        var ext = Path.GetExtension(context.ResolvedFilePath).ToUpperInvariant();

        switch (ext)
        {
            case ".BAT":
                return launcher.RunBatchFileAsync(context.ResolvedFilePath, context.EmulatorManager, context.MainWindow);
            case ".LNK":
            case ".URL":
                return launcher.LaunchShortcutFileAsync(context.ResolvedFilePath, context.EmulatorManager, context.MainWindow);
            case ".EXE":
                return launcher.LaunchExecutableAsync(context.ResolvedFilePath, context.EmulatorManager, context.MainWindow);
            default:
                // This handles all standard ROMs/Games
                return launcher.LaunchRegularEmulatorAsync(
                    context.ResolvedFilePath,
                    context.EmulatorName,
                    context.SystemManager,
                    context.EmulatorManager,
                    context.Parameters,
                    context.MainWindow,
                    context.LoadingState);
        }
    }
}