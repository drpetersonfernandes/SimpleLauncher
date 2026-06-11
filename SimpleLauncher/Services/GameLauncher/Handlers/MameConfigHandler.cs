using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using MameConfigurationService = SimpleLauncher.Services.InjectEmulatorConfig.MameConfigurationService;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

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
        if (context.EmulatorManager != null)
        {
            var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
            if (context.SystemManager != null)
            {
                var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(context.SystemManager.PrimarySystemFolder);
                var listOfSecondarySystemFolders = context.SystemManager.SystemFolders.ToArray();

                var shouldRun = true;
                if (context.Settings != null && context.Settings.Mame.ShowSettingsBeforeLaunch)
                {
                    if (context.WindowContext != null)
                        await context.WindowContext.Dispatcher.InvokeAsync(() =>
                        {
                            var win = App.ServiceProvider.GetRequiredService<InjectMameConfigWindow>();
                            win.Owner = (Window)context.WindowContext.PlatformWindow;
                            win.Initialize(resolvedExe, true, resolvedSystemFolder, listOfSecondarySystemFolders);
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

        return false;
    }
}