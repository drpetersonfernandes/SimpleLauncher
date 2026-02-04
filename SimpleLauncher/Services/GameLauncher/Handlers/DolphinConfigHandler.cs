using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class DolphinConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Dolphin", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("Dolphin.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.DolphinShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectDolphinConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            DolphinConfigurationService.InjectSettings(resolvedExe, context.Settings);
        }

        return shouldRun;
    }
}
