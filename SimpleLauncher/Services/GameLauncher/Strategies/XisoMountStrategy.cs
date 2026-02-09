using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.MountFiles;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class XisoMountStrategy : ILaunchStrategy
{
    private readonly IConfiguration _configuration;

    public XisoMountStrategy(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int Priority => 20;

    public bool IsMatch(LaunchContext context)
    {
        return context.EmulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) &&
               Path.GetExtension(context.ResolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase);
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        await using var mountedDrive = await MountXisoFiles.MountAsync(context.ResolvedFilePath, CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        if (mountedDrive.IsMounted)
        {
            await launcher.LaunchRegularEmulatorAsync(mountedDrive.MountedPath, context.EmulatorName, context.SystemManager, context.EmulatorManager, context.Parameters, context.MainWindow, context.LoadingState);
        }
    }
}