using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class SupermodelConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;

    public SupermodelConfigHandler(ILogErrors logErrors, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Supermodel", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("Supermodel.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.Supermodel.ShowSettingsBeforeLaunch)
        {
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectSupermodelConfigWindow>();
                win.Owner = (Window)context.WindowContext.PlatformWindow;
                win.Initialize(resolvedExe);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (File.Exists(resolvedExe)) SupermodelConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, _debugLogger);
        }

        return shouldRun;
    }
}
