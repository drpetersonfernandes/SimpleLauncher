using Microsoft.Extensions.DependencyInjection;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

/// <summary>
/// Resolves the file path of an emulator executable by searching the configured systems.
/// </summary>
public static class EmulatorPathResolver
{
    /// <summary>
    /// Tries to find the resolved executable path of an emulator whose name contains the given hint.
    /// </summary>
    /// <param name="emulatorNameHint">A name fragment used to identify the emulator.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <returns>The resolved emulator executable path, or null if not found.</returns>
    public static string? TryFindEmulatorPath(string emulatorNameHint, ILogger logErrors)
    {
        if (string.IsNullOrWhiteSpace(emulatorNameHint))
            return null;

        try
        {
            var configuration = App.ServiceProvider?.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            if (configuration == null)
                return null;

            var systems = SystemManager.SystemManagerService.LoadSystemManagers(configuration);
            if (systems == null || systems.Count == 0)
                return null;

            foreach (var system in systems)
            {
                if (system.Emulators == null)
                    continue;

                foreach (var emulator in system.Emulators)
                {
                    if (string.IsNullOrWhiteSpace(emulator.EmulatorLocation))
                        continue;

                    if (emulator.EmulatorName?.Contains(emulatorNameHint, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var resolved = PathHelper.ResolveRelativeToAppDirectory(emulator.EmulatorLocation);
                        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                        {
                            return resolved;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logErrors.Error(ex, $"Error resolving emulator path for hint: {emulatorNameHint}");
        }

        return null;
    }
}
