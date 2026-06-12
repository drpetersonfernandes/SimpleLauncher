using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;


namespace SimpleLauncher.Services.GameLauncher.Strategies;

/// <summary>
/// Mounts ZIP/7Z/RAR archives as virtual drives for RPCS3 (EBOOT.BIN), ScummVM, and XBLA games.
/// </summary>
public class ZipMountStrategy : ILaunchStrategy
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IMountZipFiles _mountZipFiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipMountStrategy"/> class.
    /// </summary>
    public ZipMountStrategy(IConfiguration configuration, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IMountZipFiles mountZipFiles)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _messageBox = messageBox;
        _mountZipFiles = mountZipFiles;
    }

    /// <inheritdoc />
    public int Priority => 30;

    /// <inheritdoc />
    public bool IsMatch(LaunchContext context)
    {
        if (string.IsNullOrEmpty(context.ResolvedFilePath) ||
            string.IsNullOrEmpty(context.EmulatorName) ||
            string.IsNullOrEmpty(context.SystemName))
        {
            return false;
        }

        var extension = Path.GetExtension(context.ResolvedFilePath);
        var isArchive = extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".7z", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".rar", StringComparison.OrdinalIgnoreCase);

        if (!isArchive)
        {
            return false;
        }

        return context.EmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) ||
               context.SystemName.Contains("Scumm", StringComparison.OrdinalIgnoreCase) ||
               context.SystemName.Contains("xbla", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task ExecuteAsync(LaunchContext context, ILauncherService launcher)
    {
        var log = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
        if (context.EmulatorName.Contains("RPCS3"))
        {
            return _mountZipFiles.MountZipFileAndLoadEbootBinAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.WindowContext, log, launcher, _logErrors, _messageBox);
        }
        else if (context.SystemName.Contains("Scumm"))
        {
            return _mountZipFiles.MountZipFileAndLoadWithScummVmAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, log, _logErrors, _messageBox);
        }
        else
        {
            return _mountZipFiles.MountZipFileAndSearchForFileToLoadAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.WindowContext, log, launcher, _logErrors, _messageBox);
        }
    }
}