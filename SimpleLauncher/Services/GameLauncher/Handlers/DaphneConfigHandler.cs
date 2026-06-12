using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

/// <summary>
/// Handles configuration injection for the Daphne (laserdisc arcade) emulator before launching a game.
/// </summary>
public class DaphneConfigHandler : IEmulatorConfigHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DaphneConfigHandler"/> class.
    /// </summary>
    public DaphneConfigHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Daphne", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("daphne.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <inheritdoc />
    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var shouldRun = true;
        if (context.Settings != null && context.Settings.Daphne.ShowSettingsBeforeLaunch)
        {
            if (context.WindowContext != null)
                await context.WindowContext.Dispatcher.InvokeAsync(() =>
                {
                    var win = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<InjectDaphneConfigWindow>();
                    win.Owner = (Window)context.WindowContext.PlatformWindow;
                    win.Initialize();
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                });
        }

        if (shouldRun)
        {
            var daphneArgs = DaphneConfigurationService.BuildArguments(context.Settings);
            context.Parameters = $"{context.Parameters} {daphneArgs}".Trim();
        }

        return shouldRun;
    }
}