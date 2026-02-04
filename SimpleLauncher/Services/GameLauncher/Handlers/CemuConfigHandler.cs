using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class CemuConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Cemu", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("Cemu.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.CemuShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectCemuConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            CemuConfigurationService.InjectSettings(resolvedExe, context.Settings);
        }

        return shouldRun;
    }
}
