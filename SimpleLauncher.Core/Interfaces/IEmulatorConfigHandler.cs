using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Handles emulator-specific configuration before launching a game.
/// </summary>
public interface IEmulatorConfigHandler
{
    /// <summary>
    ///     Determines whether this handler matches the specified emulator by name and path.
    /// </summary>
    /// <param name="emulatorName">The display name of the emulator.</param>
    /// <param name="emulatorPath">The file path to the emulator executable.</param>
    /// <returns>True if this handler can configure the specified emulator; otherwise, false.</returns>
    bool IsMatch(string emulatorName, string emulatorPath);

    /// <summary>
    ///     Asynchronously applies emulator-specific configuration based on the launch context.
    /// </summary>
    /// <param name="context">The launch context containing game and emulator details.</param>
    /// <returns>True if configuration was successfully applied; otherwise, false.</returns>
    Task<bool> HandleConfigurationAsync(LaunchContext context);
}