using System.IO;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class Rpcs3ConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;

    public Rpcs3ConfigHandler(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("rpcs3.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.Rpcs3.ShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectRpcs3ConfigWindow>();
                win.Owner = context.MainWindow;
                win.Initialize(resolvedExe);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            Rpcs3ConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors);
        }

        return shouldRun;
    }
}
