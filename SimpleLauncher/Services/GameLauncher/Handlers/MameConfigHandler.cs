using System;
using System.Threading.Tasks;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class MameConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ?? false) ||
               (emulatorPath?.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(context.SystemManager.PrimarySystemFolder);

        var shouldRun = true;
        if (context.Settings.MameShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectMameConfigWindow(context.Settings, resolvedExe, resolvedSystemFolder) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            MameConfigurationService.InjectSettings(resolvedExe, context.Settings, resolvedSystemFolder);
        }

        return shouldRun;
    }
}