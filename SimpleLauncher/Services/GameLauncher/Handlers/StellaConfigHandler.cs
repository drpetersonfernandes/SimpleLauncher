using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class StellaConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Stella", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("stella.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.StellaShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectStellaConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (File.Exists(resolvedExe)) StellaConfigurationService.InjectSettings(resolvedExe, context.Settings);
        }

        return shouldRun;
    }
}
