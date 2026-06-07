using System.IO;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class XeniaConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;

    public XeniaConfigHandler(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("xenia.exe", StringComparison.OrdinalIgnoreCase) ?? false) ||
               (emulatorPath?.Contains("xenia_canary.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        var shouldRun = true;

        if (context.Settings.Xenia.ShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectXeniaConfigWindow>();
                win.Owner = context.MainWindow;
                win.Initialize(resolvedExe);
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else if (File.Exists(resolvedExe))
        {
            try
            {
                XeniaConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors);
            }
            catch (Exception ex)
            {
                // Log error but allow game to launch with default Xenia settings
                DebugLogger.Log($"[XeniaConfigHandler] Failed to inject settings: {ex.Message}");
            }
        }

        return shouldRun;
    }
}