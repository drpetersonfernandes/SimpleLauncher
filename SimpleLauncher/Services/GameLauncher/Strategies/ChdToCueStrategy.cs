using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class ChdToCueStrategy : ILaunchStrategy
{
    private readonly IMessageBoxLibraryService _messageBox;

    public ChdToCueStrategy(IMessageBoxLibraryService messageBox)
    {
        _messageBox = messageBox;
    }

    public int Priority => 25;

    public bool IsMatch(LaunchContext context)
    {
        if (string.IsNullOrEmpty(context.ResolvedFilePath) ||
            string.IsNullOrEmpty(context.EmulatorName))
        {
            return false;
        }

        var isChd = Path.GetExtension(context.ResolvedFilePath).Equals(".chd", StringComparison.OrdinalIgnoreCase);
        if (!isChd)
        {
            return false;
        }

        var is4Do = context.EmulatorName.Contains("4do", StringComparison.OrdinalIgnoreCase) ||
                    (context.EmulatorManager?.EmulatorLocation?.Contains("4do.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        var isRaine = context.EmulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
                      (context.EmulatorManager?.EmulatorLocation?.Contains("raine", StringComparison.OrdinalIgnoreCase) ?? false);

        return is4Do || isRaine;
    }

    public async Task ExecuteAsync(LaunchContext context, ILauncherService launcher)
    {
        var convertingMsg = (string)Application.Current.TryFindResource("ConvertingChdToCue") ?? "Converting CHD...";
        context.LoadingState.SetLoadingState(true, convertingMsg);

        string cuePath;
        try
        {
            cuePath = await Converters.ConvertChdToCueBin.ConvertChdToCueBinAsync(context.ResolvedFilePath);
        }
        finally
        {
            // Always end conversion loading state before launching
            context.LoadingState.SetLoadingState(false);
        }

        if (cuePath == null)
        {
            await _messageBox.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue("LogPath", "error_user.log")));
            return;
        }

        try
        {
            await launcher.LaunchRegularEmulatorAsync(cuePath, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.WindowContext, context.LoadingState);
        }
        finally
        {
            // CLEANUP: Delete the temporary .cue and .bin files
            try
            {
                var binPath = Path.ChangeExtension(cuePath, ".bin");
                if (File.Exists(cuePath)) File.Delete(cuePath);
                if (File.Exists(binPath)) File.Delete(binPath);
                DebugLogger.Log($"Cleaned up temporary CHD conversion files: {cuePath}");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Failed to cleanup CHD temp files: {ex.Message}");
            }
        }
    }
}