using System.IO;
using System.Windows;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class RaineConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;

    public RaineConfigHandler(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("raine", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(context.SystemManager.PrimarySystemFolder);
        var resolvedRaineRomDirectory = PathHelper.ResolveRelativeToAppDirectory(context.Settings.Raine.RomDirectory);
        var shouldRun = true;

        if (context.Settings.Raine.ShowSettingsBeforeLaunch)
        {
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectRaineConfigWindow>();
                win.Owner = (Window)context.WindowContext.PlatformWindow;
                win.Initialize(resolvedExe, true, context.ResolvedFilePath, resolvedSystemFolder);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            // Pass the resolved RaineRomDirectory to the service
            RaineConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, context.ResolvedFilePath, resolvedSystemFolder, resolvedRaineRomDirectory);
        }

        return shouldRun;
    }
}