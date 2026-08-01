using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Resolves the display image path for system configurations.
/// </summary>
public interface ISystemImageResolverService
{
    /// <summary>
    /// Asynchronously resolves the display image path for a system, using fuzzy matching if enabled.
    /// </summary>
    /// <param name="config">The system configuration to resolve the image for.</param>
    /// <returns>The resolved display image path.</returns>
    Task<string> ResolveDisplayImageAsync(SystemManagerService config);
}
