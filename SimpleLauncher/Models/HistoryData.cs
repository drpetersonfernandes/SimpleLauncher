using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents the root ROM history data containing version info and a collection of entries.
/// </summary>
[MessagePackObject]
public class HistoryData
{
    [Key(0)]
    public string? Version { get; set; }

    [Key(1)]
    public string? Date { get; set; }

    [Key(2)]
    public EntryData[]? Entries { get; set; } = [];
}
