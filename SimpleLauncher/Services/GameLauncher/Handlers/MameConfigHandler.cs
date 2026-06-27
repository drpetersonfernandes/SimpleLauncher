using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using MameConfigurationService = SimpleLauncher.Services.InjectEmulatorConfig.MameConfigurationService;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Handlers;

/// <summary>
/// Handles configuration injection for the MAME emulator before launching a game.
/// </summary>
public class MameConfigHandler : IEmulatorConfigHandler
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MameConfigHandler"/> class.
    /// </summary>
    public MameConfigHandler(ILogErrors logErrors, IDebugLogger debugLogger, IMessageBoxLibraryService messageBoxLibrary, IServiceScopeFactory scopeFactory)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBoxLibrary = messageBoxLibrary;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ?? false) ||
               (emulatorPath?.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <inheritdoc />
    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager != null)
        {
            var resolvedExe = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
            if (context.SystemManager != null)
            {
                var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(context.SystemManager.PrimarySystemFolder);
                var listOfSecondarySystemFolders = context.SystemManager.SystemFolders.ToArray();

                var shouldRun = true;
                if (context.Settings != null && context.Settings.Mame.ShowSettingsBeforeLaunch)
                {
                    if (context.WindowContext != null)
                        await context.WindowContext.Dispatcher.InvokeAsync(() =>
                        {
                            var win = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<InjectMameConfigWindow>();
                            win.Owner = (Window)context.WindowContext.PlatformWindow;
                            win.Initialize(resolvedExe, true, resolvedSystemFolder, listOfSecondarySystemFolders);
                            win.ShowDialog();
                            shouldRun = win.ShouldRun;
                        });
                }
                else
                {
                    try
                    {
                        MameConfigurationService.InjectSettings(resolvedExe, context.Settings, _logErrors, _debugLogger, resolvedSystemFolder, listOfSecondarySystemFolders);
                    }
                    catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
                    {
                        _debugLogger.Log($"[MameConfigHandler] Failed to inject MAME configuration: {ex.Message}");
                        _logErrors.LogAndForget(ex, "[MameConfigHandler] Failed to inject MAME configuration. The game will launch with existing MAME settings.");
                        await _messageBoxLibrary.FailedToInjectMameConfigurationMessageBoxAsync();
                    }
                }

                return shouldRun;
            }
        }

        return false;
    }
}