namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Calculates RetroAchievements hashes for game files using the RetroAchievementsSharp library.
/// </summary>
public interface IRetroAchievementsFileHasher
{
    /// <summary>
    /// Calculates the RetroAchievements hash for a game file using the console ID of the given system.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="systemName">The RetroAchievements system name (resolved to a console ID internally).</param>
    /// <returns>The 32-character lowercase hex hash, or null if the file could not be hashed.</returns>
    Task<string?> CalculateHashAsync(string filePath, string systemName);
}