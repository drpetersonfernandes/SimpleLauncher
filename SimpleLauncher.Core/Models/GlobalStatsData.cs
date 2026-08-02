namespace SimpleLauncher.Core.Models;

/// <summary>
/// Holds aggregate statistics about the entire game library.
/// </summary>
public sealed class GlobalStatsData
{
    /// <summary>
    /// Gets the total number of systems configured.
    /// </summary>
    public int TotalSystems { get; init; }

    /// <summary>
    /// Gets the total number of emulators configured.
    /// </summary>
    public int TotalEmulators { get; init; }

    /// <summary>
    /// Gets the total number of games in the library.
    /// </summary>
    public int TotalGames { get; init; }

    /// <summary>
    /// Gets the total number of game images.
    /// </summary>
    public int TotalImages { get; init; }

    /// <summary>
    /// Gets the total disk size of the library in bytes.
    /// </summary>
    public long TotalDiskSize { get; init; }

    /// <summary>
    /// Gets the number of systems that have missing images.
    /// </summary>
    public int TotalSystemsWithMissingImages { get; init; }
}
