using System.IO;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class SupermodelConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;

    public SupermodelConfigHandler(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Supermodel", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("Supermodel.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.SupermodelShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectSupermodelConfigWindow>();
                win.Owner = context.MainWindow;
                win.Initialize(resolvedExe);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (File.Exists(resolvedExe)) SupermodelConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors);
        }

        return shouldRun;
    }
}
