#nullable enable

using MessagePack;

namespace SimpleLauncher.Services.RomHistory.Models;

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

/// <summary>
/// Represents a single ROM history entry containing software, systems, and descriptive text.
/// </summary>
[MessagePackObject]
public class EntryData
{
    [Key(0)]
    public SoftwareData? Software { get; set; }

    [Key(1)]
    public SystemsData? Systems { get; set; }

    [Key(2)]
    public string? Text { get; set; }
}

/// <summary>
/// Contains a collection of system items associated with a ROM history entry.
/// </summary>
[MessagePackObject]
public class SystemsData
{
    [Key(0)]
    public SystemItemData[] SystemItems { get; set; } = [];
}

/// <summary>
/// Represents a system entry with a name and associated game.
/// </summary>
[MessagePackObject]
public class SystemItemData
{
    [Key(0)]
    public string? Name { get; set; }

    [Key(1)]
    public string? Game { get; set; }
}

/// <summary>
/// Contains a collection of software items associated with a ROM history entry.
/// </summary>
[MessagePackObject]
public class SoftwareData
{
    [Key(0)]
    public ItemData[] Items { get; set; } = [];
}

/// <summary>
/// Represents a software item with a list name, display name, and associated game.
/// </summary>
[MessagePackObject]
public class ItemData
{
    [Key(0)]
    public string? List { get; set; }

    [Key(1)]
    public string? Name { get; set; }

    [Key(2)]
    public string? Game { get; set; }
}
