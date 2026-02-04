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

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        var ext = Path.GetExtension(context.ResolvedFilePath).ToUpperInvariant();

        switch (ext)
        {
            case ".BAT":
                await launcher.RunBatchFileAsync(context.ResolvedFilePath, context.EmulatorManager, context.MainWindow);
                break;
            case ".LNK":
            case ".URL":
                await launcher.LaunchShortcutFileAsync(context.ResolvedFilePath, context.EmulatorManager, context.MainWindow);
                break;
            case ".EXE":
                await launcher.LaunchExecutableAsync(context.ResolvedFilePath, context.EmulatorManager, context.MainWindow);
                break;
            default:
                // This handles all standard ROMs/Games
                await launcher.LaunchRegularEmulatorAsync(
                    context.ResolvedFilePath,
                    context.EmulatorName,
                    context.SystemManager,
                    context.EmulatorManager,
                    context.Parameters,
                    context.MainWindow,
                    launcher,
                    context.LoadingState);
                break;
        }
    }
}