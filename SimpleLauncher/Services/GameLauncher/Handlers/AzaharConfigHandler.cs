using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class AzaharConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IServiceScopeFactory _scopeFactory;

    public AzaharConfigHandler(ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger, IServiceScopeFactory scopeFactory)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
        _scopeFactory = scopeFactory;
    }

    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Azahar", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("azahar.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager != null)
        {
            var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
            var shouldRun = true;

            if (context.Settings != null && context.Settings.Azahar.ShowSettingsBeforeLaunch)
            {
                if (context.WindowContext != null)
                    await context.WindowContext.Dispatcher.InvokeAsync(() =>
                    {
                        var win = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<InjectAzaharConfigWindow>();
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
                    AzaharConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, _debugLogger);
                }
                catch (AzaharPermissionException)
                {
                    // Show permission error message but allow the game to launch
                    await _messageBox.AzaharConfigurationInjectionPermissionErrorMessageBox();
                    // Return true to allow the game to launch with default settings
                }
            }

            return shouldRun;
        }

        return false;
    }
}