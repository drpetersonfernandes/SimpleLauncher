using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Provides persistence for per-system RetroAchievements hash scans,
///     stored as JSON files under <c>%LocalAppData%\SimpleLauncher\RetroAchievementsHashes\</c>.
/// </summary>
public interface IRetroAchievementsHashStore
{
    /// <summary>
    ///     Gets the full path of the JSON hash file for the given system.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <returns>The full file path of the system's hash file.</returns>
    string GetSystemHashFilePath(string systemName);

    /// <summary>
    ///     Determines whether a hash scan result file exists for the given system.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <returns>True if a hash scan result file exists; otherwise, false.</returns>
    bool HasSystemHashes(string systemName);

    /// <summary>
    ///     Loads the persisted hash scan for the given system.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <returns>The loaded hash scan, or null if no scan result exists for the system.</returns>
    RaSystemHashes? LoadSystemHashes(string systemName);

    /// <summary>
    ///     Persists the hash scan for the given system as a JSON file.
    /// </summary>
    /// <param name="systemHashes">The hash scan data to persist.</param>
    void SaveSystemHashes(RaSystemHashes systemHashes);
}