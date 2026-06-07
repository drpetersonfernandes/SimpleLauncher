using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.GameLauncher.MountFiles;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class ZipMountStrategy : ILaunchStrategy
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;

    public ZipMountStrategy(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
    }

    public int Priority => 30;

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

    public Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        var log = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
        if (context.EmulatorName.Contains("RPCS3"))
        {
            return MountZipFiles.MountZipFileAndLoadEbootBinAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, log, launcher, _logErrors);
        }
        else if (context.SystemName.Contains("Scumm"))
        {
            return MountZipFiles.MountZipFileAndLoadWithScummVmAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, log, _logErrors);
        }
        else
        {
            return MountZipFiles.MountZipFileAndSearchForFileToLoadAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, log, launcher, _logErrors);
        }
    }
}