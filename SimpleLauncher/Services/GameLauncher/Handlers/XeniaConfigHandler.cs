using System.Windows;
using SimpleLauncher.Services.DebugAndBugReport;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class XeniaConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IServiceScopeFactory _scopeFactory;

    public XeniaConfigHandler(ILogErrors logErrors, IDebugLogger debugLogger, IServiceScopeFactory scopeFactory)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _scopeFactory = scopeFactory;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("xenia.exe", StringComparison.OrdinalIgnoreCase) ?? false) ||
               (emulatorPath?.Contains("xenia_canary.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager != null)
        {
            var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
            var shouldRun = true;

            if (context.Settings != null && context.Settings.Xenia.ShowSettingsBeforeLaunch)
            {
                if (context.WindowContext != null)
                    await context.WindowContext.Dispatcher.InvokeAsync(() =>
                    {
                        var win = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<InjectXeniaConfigWindow>();
                        win.Owner = (Window)context.WindowContext.PlatformWindow;
                        win.Initialize(resolvedExe);
                        win.ShowDialog();
                        shouldRun = win.ShouldRun;
                    });
            }
            else if (File.Exists(resolvedExe))
            {
                try
                {
                    XeniaConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, _debugLogger);
                }
                catch (Exception ex)
                {
                    // Log error but allow game to launch with default Xenia settings
                    DebugLogger.Log($"[XeniaConfigHandler] Failed to inject settings: {ex.Message}");
                }
            }

            return shouldRun;
        }

        return false;
    }
}