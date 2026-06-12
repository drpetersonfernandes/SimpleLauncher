using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

/// <summary>
/// Converts CHD files to CUE/BIN format for emulators that do not support CHD natively (e.g., 4DO, Raine).
/// </summary>
public class ChdToCueStrategy : ILaunchStrategy
{
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IDebugLogger _debugLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChdToCueStrategy"/> class.
    /// </summary>
    public ChdToCueStrategy(IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _messageBox = messageBox;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
    }

    /// <inheritdoc />
    public int Priority => 25;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task ExecuteAsync(LaunchContext context, ILauncherService launcher)
    {
        var convertingMsg = (string)Application.Current.TryFindResource("ConvertingChdToCue") ?? "Converting CHD...";
        if (context.LoadingState != null)
        {
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
                    _debugLogger.Log($"Cleaned up temporary CHD conversion files: {cuePath}");
                }
                catch (Exception ex)
                {
                    _debugLogger.Log($"Failed to cleanup CHD temp files: {ex.Message}");
                }
            }
        }
    }
}