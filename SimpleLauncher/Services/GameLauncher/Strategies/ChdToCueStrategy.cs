using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class ChdToCueStrategy : ILaunchStrategy
{
    public int Priority => 10;

    public bool IsMatch(LaunchContext context)
    {
        var isChd = Path.GetExtension(context.ResolvedFilePath).Equals(".chd", StringComparison.OrdinalIgnoreCase);
        if (!isChd) return false;

        var is4do = context.EmulatorName.Contains("4do", StringComparison.OrdinalIgnoreCase) ||
                    (context.EmulatorManager?.EmulatorLocation?.Contains("4do.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        var isRaine = context.EmulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
                      (context.EmulatorManager?.EmulatorLocation?.Contains("raine", StringComparison.OrdinalIgnoreCase) ?? false);

        return is4do || isRaine;
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        var convertingMsg = (string)Application.Current.TryFindResource("ConvertingChdToCue") ?? "Converting CHD...";
        context.LoadingState.SetLoadingState(true, convertingMsg);

        var cuePath = await Converters.ConvertChdToCueBin.ConvertChdToCueBinAsync(context.ResolvedFilePath);
        if (cuePath == null)
        {
            context.LoadingState.SetLoadingState(false);
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(CheckPaths.PathHelper.ResolveRelativeToAppDirectory(App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue("LogPath", "error_user.log")));
            return;
        }

        try
        {
            await launcher.LaunchRegularEmulatorAsync(cuePath, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, launcher, context.LoadingState);
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
