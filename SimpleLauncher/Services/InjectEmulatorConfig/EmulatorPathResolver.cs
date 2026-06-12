using Microsoft.Extensions.DependencyInjection;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

using Interfaces;

public static class EmulatorPathResolver
{
    public static string TryFindEmulatorPath(string emulatorNameHint, ILogErrors logErrors)
    {
        if (string.IsNullOrWhiteSpace(emulatorNameHint))
            return null;

        try
        {
            var configuration = App.ServiceProvider?.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            if (configuration == null)
                return null;

            var systems = SystemManager.SystemManager.LoadSystemManagers(configuration);
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
            logErrors.LogAndForget(ex, $"Error resolving emulator path for hint: {emulatorNameHint}");
        }

        return null;
    }
}
