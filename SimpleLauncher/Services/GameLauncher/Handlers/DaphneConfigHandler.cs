using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class DaphneConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Daphne", StringComparison.OrdinalIgnoreCase) || (emulatorPath?.Contains("daphne.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var shouldRun = true;
        if (context.Settings.Daphne.ShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = App.ServiceProvider.GetRequiredService<InjectDaphneConfigWindow>();
                win.Owner = context.MainWindow;
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