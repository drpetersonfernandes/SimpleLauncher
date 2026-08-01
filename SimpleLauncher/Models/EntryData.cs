using MessagePack;

namespace SimpleLauncher.Models;

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
