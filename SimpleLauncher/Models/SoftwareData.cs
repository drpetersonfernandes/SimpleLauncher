using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Contains a collection of software items associated with a ROM history entry.
/// </summary>
[MessagePackObject]
public class SoftwareData
{
    [Key(0)]
    public ItemData[] Items { get; set; } = [];
}
