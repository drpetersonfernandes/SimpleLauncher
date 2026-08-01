using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents the root ROM history data containing version info and a collection of entries.
/// </summary>
[MessagePackObject]
public class HistoryData
{
    /// <summary>
    /// Gets or sets the version of the ROM history database.
    /// </summary>
    [Key(0)]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the date of the ROM history database.
    /// </summary>
    [Key(1)]
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets the collection of ROM history entries.
    /// </summary>
    [Key(2)]
    public EntryData[]? Entries { get; set; } = [];
}
