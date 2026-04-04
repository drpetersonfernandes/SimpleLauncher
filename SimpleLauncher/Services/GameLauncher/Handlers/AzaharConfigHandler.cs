using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class AzaharConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Azahar", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("azahar.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.AzaharShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectAzaharConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            try
            {
                AzaharConfigurationService.InjectSettings(resolvedExe, context.Settings);
            }
            catch (AzaharPermissionException)
            {
                // Show permission error message but allow the game to launch
                MessageBoxLibrary.AzaharConfigurationInjectionPermissionError();
                // Return true to allow the game to launch with default settings
            }
        }

        return shouldRun;
    }
}