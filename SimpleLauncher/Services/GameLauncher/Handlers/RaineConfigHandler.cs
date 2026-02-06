using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class RaineConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("raine", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(context.SystemManager.PrimarySystemFolder);
        var shouldRun = true;

        if (context.Settings.RaineShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                // Pass the game path and system folder to the window
                var win = new InjectRaineConfigWindow(context.Settings, resolvedExe, context.ResolvedFilePath, resolvedSystemFolder)
                {
                    Owner = context.MainWindow
                };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            RaineConfigurationService.InjectSettings(resolvedExe, context.Settings, context.ResolvedFilePath, resolvedSystemFolder);
        }

        return shouldRun;
    }
}