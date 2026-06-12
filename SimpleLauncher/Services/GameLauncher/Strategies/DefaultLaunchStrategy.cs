using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

/// <summary>
/// Fallback strategy that handles standard file types (.BAT, .LNK, .URL, .EXE) and regular ROM/game launches.
/// Has the lowest priority so all specialized strategies are tried first.
/// </summary>
public class DefaultLaunchStrategy : ILaunchStrategy
{
    /// <inheritdoc />
    public int Priority => 999;

    /// <inheritdoc />
    public bool IsMatch(LaunchContext context)
    {
        return true;
    }

    /// <inheritdoc />
    public Task ExecuteAsync(LaunchContext context, ILauncherService launcher)
    {
        var ext = Path.GetExtension(context.ResolvedFilePath).ToUpperInvariant();

        switch (ext)
        {
            case ".BAT":
                return launcher.RunBatchFileAsync(context.ResolvedFilePath, context.EmulatorManager, context.WindowContext);
            case ".LNK":
            case ".URL":
                return launcher.LaunchShortcutFileAsync(context.ResolvedFilePath, context.EmulatorManager, context.WindowContext);
            case ".EXE":
                return launcher.LaunchExecutableAsync(context.ResolvedFilePath, context.EmulatorManager, context.WindowContext);
            default:
                // This handles all standard ROMs/Games
                return launcher.LaunchRegularEmulatorAsync(
                    context.ResolvedFilePath,
                    context.EmulatorName,
                    context.SystemManager,
                    context.EmulatorManager,
                    context.Parameters,
                    context.WindowContext,
                    context.LoadingState);
        }
    }
}