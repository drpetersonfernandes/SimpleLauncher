#nullable enable

using MessagePack;

namespace SimpleLauncher.Core.Services.RomHistory.Models;

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

[MessagePackObject]
public class SystemsData
{
    [Key(0)]
    public SystemItemData[] SystemItems { get; set; } = [];
}

[MessagePackObject]
public class SystemItemData
{
    [Key(0)]
    public string? Name { get; set; }

    [Key(1)]
    public string? Game { get; set; }
}

[MessagePackObject]
public class SoftwareData
{
    [Key(0)]
    public ItemData[] Items { get; set; } = [];
}

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
