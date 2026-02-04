using System;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class MednafenConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("mednafen.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.MednafenShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectMednafenConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (File.Exists(resolvedExe)) MednafenConfigurationService.InjectSettings(resolvedExe, context.Settings);
        }

        return shouldRun;
    }
}