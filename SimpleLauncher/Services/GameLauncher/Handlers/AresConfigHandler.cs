using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class AresConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Ares", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("ares.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.AresShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var aresWindow = new InjectAresConfigWindow(context.Settings, resolvedEmulatorExePath) { Owner = context.MainWindow };
                aresWindow.ShowDialog();
                shouldRun = aresWindow.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
            {
                AresConfigurationService.InjectSettings(resolvedEmulatorExePath, context.Settings);
            }
        }

        return shouldRun;
    }
}