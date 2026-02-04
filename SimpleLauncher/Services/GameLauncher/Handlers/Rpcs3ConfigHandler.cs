using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class Rpcs3ConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("rpcs3.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.Rpcs3ShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectRpcs3ConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            Rpcs3ConfigurationService.InjectSettings(resolvedExe, context.Settings);
        }

        return shouldRun;
    }
}
