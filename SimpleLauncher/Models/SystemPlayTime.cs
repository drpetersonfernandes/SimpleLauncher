using System.Globalization;
using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Tracks cumulative play time for a specific system, persisted via MessagePack.
/// </summary>
[MessagePackObject]
public class SystemPlayTime
{
    /// <summary>
    /// Gets the name of the system.
    /// </summary>
    [Key(0)]
    public string SystemName { get; init; }

    /// <summary>
    /// Gets or sets the total play time in seconds.
    /// </summary>
    [Key(1)]
    public long PlayTimeSeconds { get; set; }

    /// <summary>
    /// Gets the play time formatted as HH:mm:ss.
    /// </summary>
    [IgnoreMember]
    public string FormattedPlayTime =>
        TimeSpan.FromSeconds(PlayTimeSeconds).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
}
