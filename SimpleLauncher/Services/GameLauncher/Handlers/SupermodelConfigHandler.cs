using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class SupermodelConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Supermodel", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("Supermodel.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.SupermodelShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectSupermodelConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (File.Exists(resolvedExe)) SupermodelConfigurationService.InjectSettings(resolvedExe, context.Settings);
        }

        return shouldRun;
    }
}
