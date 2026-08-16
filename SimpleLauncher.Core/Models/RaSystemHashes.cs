namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents the persisted RetroAchievements hash scan for a single system,
/// stored as a JSON file in the local application data folder.
/// </summary>
public class RaSystemHashes
{
    /// <summary>
    /// Gets or sets the name of the system that was scanned.
    /// </summary>
    public string SystemName { get; set; } = "";

    /// <summary>
    /// Gets or sets the UTC timestamp of when the hash scan completed.
    /// </summary>
    public DateTime ScannedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the number of game files that were present in the ROM path at scan time.
    /// Used to skip re-hashing when the game count has not changed.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Gets or sets the map of full game file paths to their RetroAchievements hash values.
    /// </summary>
    public IDictionary<string, string> Hashes { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}