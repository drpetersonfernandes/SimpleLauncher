using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

/// <summary>
/// Handles configuration injection for the Ares emulator before launching a game.
/// </summary>
public class AresConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AresConfigHandler"/> class.
    /// </summary>
    public AresConfigHandler(ILogErrors logErrors, IDebugLogger debugLogger, IServiceScopeFactory scopeFactory)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Ares", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("ares.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <inheritdoc />
    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager != null)
        {
            var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
            var shouldRun = false;

            if (context.Settings != null && context.Settings.Ares.ShowSettingsBeforeLaunch)
            {
                if (context.WindowContext != null)
                    await context.WindowContext.Dispatcher.InvokeAsync(() =>
                    {
                        var aresWindow = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<InjectAresConfigWindow>();
                        aresWindow.Owner = (Window)context.WindowContext.PlatformWindow;
                        aresWindow.Initialize(resolvedEmulatorExePath);
                        aresWindow.ShowDialog();
                        shouldRun = aresWindow.ShouldRun;
                    });
            }
            else
            {
                shouldRun = true;
                if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                {
                    AresConfigurationService.InjectSettings(resolvedEmulatorExePath, context.Settings, _logErrors, _debugLogger);
                }
            }

            return shouldRun;
        }

        return false;
    }
}