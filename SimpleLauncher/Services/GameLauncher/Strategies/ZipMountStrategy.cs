using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.MountFiles;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class ZipMountStrategy : ILaunchStrategy
{
    public int Priority => 30;

    public bool IsMatch(LaunchContext context)
    {
        var isZip = Path.GetExtension(context.ResolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase);
        if (!isZip) return false;

        return context.EmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) ||
               context.SystemName.Contains("Scumm", StringComparison.OrdinalIgnoreCase) ||
               context.SystemName.Contains("xbla", StringComparison.OrdinalIgnoreCase);
    }

    public Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        var log = App.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("LogPath").ToString();
        if (context.EmulatorName.Contains("RPCS3"))
            return MountZipFiles.MountZipFileAndLoadEbootBinAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, log, launcher);
        else if (context.SystemName.Contains("Scumm"))
            return MountZipFiles.MountZipFileAndLoadWithScummVmAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, log);
        else
            return MountZipFiles.MountZipFileAndSearchForFileToLoadAsync(context.ResolvedFilePath, context.SystemName, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, log, launcher);
    }
}