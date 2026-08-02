namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Configures emulator configuration files with RetroAchievements credentials.
/// </summary>
public interface IRetroAchievementsEmulatorConfiguratorService
{
    /// <summary>
    /// Configures RetroArch's retroarch.cfg with RetroAchievements credentials.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="password">The RetroAchievements password.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigureRetroArch(string exePath, string username, string password);

    /// <summary>
    /// Configures PCSX2's PCSX2.ini with RetroAchievements credentials.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="token">The RetroAchievements web API token.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigurePcsx2(string exePath, string username, string token);

    /// <summary>
    /// Configures DuckStation's settings.ini with RetroAchievements credentials and encrypted token.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="token">The RetroAchievements web API token.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigureDuckStation(string exePath, string username, string token);

    /// <summary>
    /// Configures PPSSPP's ppsspp.ini and session key file with RetroAchievements credentials.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="token">The RetroAchievements web API token.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigurePpspp(string exePath, string username, string token);

    /// <summary>
    /// Configures Dolphin's RetroAchievements.ini with RetroAchievements credentials.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="token">The RetroAchievements web API token.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigureDolphin(string exePath, string username, string token);

    /// <summary>
    /// Configures Flycast's emu.cfg with RetroAchievements credentials.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="token">The RetroAchievements web API token.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigureFlycast(string exePath, string username, string token);

    /// <summary>
    /// Configures BizHawk's config.ini with RetroAchievements credentials.
    /// </summary>
    /// <param name="exePath">The full path to the emulator executable.</param>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="token">The RetroAchievements web API token.</param>
    /// <returns>True if the configuration was applied successfully; otherwise, false.</returns>
    bool ConfigureBizHawk(string exePath, string username, string token);
}
