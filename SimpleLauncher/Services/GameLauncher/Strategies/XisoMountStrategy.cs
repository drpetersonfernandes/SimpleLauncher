using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;


namespace SimpleLauncher.Services.GameLauncher.Strategies;

/// <summary>
/// Mounts original Xbox ISO images as virtual drives and launches them with Cxbx-Reloaded.
/// </summary>
public class XisoMountStrategy : ILaunchStrategy
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IMountXisoFiles _mountXisoFiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="XisoMountStrategy"/> class.
    /// </summary>
    public XisoMountStrategy(IConfiguration configuration, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IMountXisoFiles mountXisoFiles)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _messageBox = messageBox;
        _mountXisoFiles = mountXisoFiles;
    }

    /// <inheritdoc />
    public int Priority => 20;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task ExecuteAsync(LaunchContext context, ILauncherService launcher)
    {
        await using var mountedDrive = await _mountXisoFiles.MountAsync(context.ResolvedFilePath, PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"), _logErrors, _messageBox);
        if (mountedDrive.IsMounted)
        {
            await launcher.LaunchRegularEmulatorAsync(mountedDrive.MountedPath, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.WindowContext, context.LoadingState, context.ResolvedFilePath);
        }
    }
}