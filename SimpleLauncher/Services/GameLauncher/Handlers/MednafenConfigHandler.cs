using System.IO;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;
using Microsoft.Extensions.DependencyInjection;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class MednafenConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;

    public MednafenConfigHandler(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("mednafen.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = false;

        if (context.Settings.Mednafen.ShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectMednafenConfigWindow>();
                win.Owner = context.MainWindow;
                win.Initialize(resolvedExe);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (File.Exists(resolvedExe)) MednafenConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors);
        }

        return shouldRun;
    }
}