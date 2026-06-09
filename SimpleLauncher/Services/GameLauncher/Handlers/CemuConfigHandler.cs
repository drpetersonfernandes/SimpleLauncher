using System.IO;
using System.Windows;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class CemuConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;

    public CemuConfigHandler(ILogErrors logErrors, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Cemu", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("Cemu.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.Cemu.ShowSettingsBeforeLaunch)
        {
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectCemuConfigWindow>();
                win.Owner = (Window)context.WindowContext.PlatformWindow;
                win.Initialize(resolvedExe);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            CemuConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, _debugLogger);
        }

        return shouldRun;
    }
}
