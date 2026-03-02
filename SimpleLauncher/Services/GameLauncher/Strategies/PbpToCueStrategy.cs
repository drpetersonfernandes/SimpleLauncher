using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class PbpToCueStrategy : ILaunchStrategy
{
    public int Priority => 15; // Higher priority than Default (999) but lower than CHD (10) to handle specific case

    public bool IsMatch(LaunchContext context)
    {
        var isPbp = Path.GetExtension(context.ResolvedFilePath).Equals(".pbp", StringComparison.OrdinalIgnoreCase);
        if (!isPbp) return false;

        // Check if emulator is Mednafen (which doesn't support PBP files)
        var isMednafen = context.EmulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) ||
                         (context.EmulatorManager?.EmulatorLocation?.Contains("mednafen", StringComparison.OrdinalIgnoreCase) ?? false);

        return isMednafen;
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        var convertingMsg = (string)Application.Current.TryFindResource("ConvertingPbpToCue") ?? "Converting PBP to CUE/BIN...";
        context.LoadingState.SetLoadingState(true, convertingMsg);

        var cuePath = await Converters.ConvertPbpToCueBin.ConvertPbpToCueBinAsync(context.ResolvedFilePath);
        if (cuePath == null)
        {
            context.LoadingState.SetLoadingState(false);
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(CheckPaths.PathHelper.ResolveRelativeToAppDirectory(App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue("LogPath", "error_user.log")));
            return;
        }

        try
        {
            await launcher.LaunchRegularEmulatorAsync(cuePath, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, context.LoadingState);
        }
        finally
        {
            // CLEANUP: Delete the temporary .cue and .bin files
            // Also handle potential _disc1 suffix that psxpackager may add
            try
            {
                var tempFolder = Path.GetDirectoryName(cuePath);
                var baseFileName = Path.GetFileNameWithoutExtension(cuePath);

                // Delete the main .cue and .bin files
                if (File.Exists(cuePath)) File.Delete(cuePath);
                var binPath = Path.ChangeExtension(cuePath, ".bin");
                if (File.Exists(binPath)) File.Delete(binPath);

                // Also delete potential _disc1 variants
                if (!string.IsNullOrEmpty(tempFolder))
                {
                    var disc1CuePath = Path.Combine(tempFolder, $"{baseFileName}_disc1.cue");
                    var disc1BinPath = Path.Combine(tempFolder, $"{baseFileName}_disc1.bin");
                    if (File.Exists(disc1CuePath)) File.Delete(disc1CuePath);
                    if (File.Exists(disc1BinPath)) File.Delete(disc1BinPath);
                }

                DebugLogger.Log($"Cleaned up temporary PBP conversion files: {baseFileName}");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Failed to cleanup PBP temp files: {ex.Message}");
            }
        }
    }
}
