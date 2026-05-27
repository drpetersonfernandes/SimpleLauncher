using System.IO;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.InjectEmulatorConfig;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

public class BlastemConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Blastem", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("blastem.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        var emulatorLocation = context.EmulatorManager.EmulatorLocation;
        var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(emulatorLocation);

        // Log the path resolution for debugging
        DebugLogger.Log($"[BlastemConfigHandler] Emulator location: {emulatorLocation ?? "NULL"}");
        DebugLogger.Log($"[BlastemConfigHandler] Resolved executable: {resolvedExe ?? "NULL"}");

        // Early validation: Check if emulator path is configured
        if (string.IsNullOrWhiteSpace(emulatorLocation))
        {
            DebugLogger.Log("[BlastemConfigHandler] ERROR: Emulator location is not configured");
            App.LogErrorAsync(new InvalidOperationException("Blastem emulator location is not configured"),
                "BlastemConfigHandler: Emulator location is null or empty in system configuration");
            // Allow game to launch anyway, user will be prompted to select emulator
        }
        // Early validation: Check if resolved path is valid
        else if (string.IsNullOrEmpty(resolvedExe))
        {
            DebugLogger.Log($"[BlastemConfigHandler] ERROR: Failed to resolve emulator path: {emulatorLocation}");
            App.LogErrorAsync(new InvalidOperationException($"Failed to resolve Blastem emulator path: {emulatorLocation}"),
                $"BlastemConfigHandler: Path resolution failed for '{emulatorLocation}'");
            // Allow game to launch anyway, user will be prompted to select emulator
        }
        // Early validation: Check if file exists
        else if (!File.Exists(resolvedExe))
        {
            DebugLogger.Log($"[BlastemConfigHandler] WARNING: Emulator not found at: {resolvedExe}");
            // Allow game to launch anyway, user will be prompted to select emulator
        }

        var shouldRun = false;

        if (context.Settings.BlastemShowSettingsBeforeLaunch)
        {
            await context.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var win = new InjectBlastemConfigWindow(context.Settings, resolvedExe) { Owner = context.MainWindow };
                win.ShowDialog();
                shouldRun = win.ShouldRun;
            });
        }
        else
        {
            shouldRun = true;
            if (!string.IsNullOrEmpty(resolvedExe) && File.Exists(resolvedExe))
            {
                try
                {
                    BlastemConfigurationService.InjectSettings(resolvedExe, context.Settings);
                    DebugLogger.Log("[BlastemConfigHandler] Configuration injected successfully");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[BlastemConfigHandler] ERROR: Configuration injection failed: {ex.Message}");
                    App.LogErrorAsync(ex, $"BlastemConfigHandler: Configuration injection failed for path: {resolvedExe}");
                    // Continue launching the game even if injection fails
                }
            }
            else
            {
                DebugLogger.Log("[BlastemConfigHandler] Skipping configuration injection - emulator not found");
            }
        }

        return shouldRun;
    }
}