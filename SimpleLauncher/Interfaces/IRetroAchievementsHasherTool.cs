using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Determines RetroAchievements hash support and calculates hashes for game files.
/// </summary>
public interface IRetroAchievementsHasherTool
{
    /// <summary>
    /// Determines whether the specified system supports RetroAchievements hashing.
    /// </summary>
    /// <param name="systemName">The system name to check.</param>
    /// <returns>True if the system is supported for RetroAchievements hashing; otherwise, false.</returns>
    bool IsSystemSupportedForHashing(string systemName);

    /// <summary>
    /// Calculates the RetroAchievements hash for a game file, handling system matching, extraction, and format conversion as needed.
    /// </summary>
    /// <param name="filePath">The full path to the game file to hash.</param>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileFormatsToLaunch">The list of file extensions considered valid for launching.</param>
    /// <param name="loadingState">The optional loading state to update during hash calculation.</param>
    /// <param name="logErrors">The logger instance for error logging.</param>
    /// <returns>A <see cref="RaHashResult"/> containing the hash, temp extraction path, and any error information.</returns>
    Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, IList<string> fileFormatsToLaunch, ILoadingState loadingState, ILogger logErrors);
}
