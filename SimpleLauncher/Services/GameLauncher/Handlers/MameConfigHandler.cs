using System.Windows;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class MameConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;

    public MameConfigHandler(ILogErrors logErrors, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
    }

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
        var listOfSecondarySystemFolders = context.SystemManager.SystemFolders.ToArray();

        var shouldRun = true;
        if (context.Settings.Mame.ShowSettingsBeforeLaunch)
        {
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectMameConfigWindow>();
                win.Owner = (Window)context.WindowContext.PlatformWindow;
                win.Initialize(resolvedExe, resolvedSystemFolder, true, listOfSecondarySystemFolders);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            MameConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, _debugLogger, resolvedSystemFolder, listOfSecondarySystemFolders);
        }

        return shouldRun;
    }
}