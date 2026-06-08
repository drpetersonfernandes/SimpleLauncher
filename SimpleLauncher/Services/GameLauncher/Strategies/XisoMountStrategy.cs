using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.GameLauncher.MountFiles;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class XisoMountStrategy : ILaunchStrategy
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;

    public XisoMountStrategy(IConfiguration configuration, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _messageBox = messageBox;
    }

    public int Priority => 20;

    public bool IsMatch(LaunchContext context)
    {
        if (string.IsNullOrEmpty(context.ResolvedFilePath) ||
            string.IsNullOrEmpty(context.EmulatorName))
        {
            return false;
        }

        return context.EmulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) &&
               Path.GetExtension(context.ResolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase);
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        await using var mountedDrive = await MountXisoFiles.MountAsync(context.ResolvedFilePath, PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"), _logErrors, _messageBox);
        if (mountedDrive.IsMounted)
        {
            // Pass the original ISO file path for display in notifications
            await launcher.LaunchRegularEmulatorAsync(mountedDrive.MountedPath, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.WindowContext, context.LoadingState, context.ResolvedFilePath);
        }
    }
}