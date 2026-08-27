using MessagePack;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Contains a collection of software items associated with a ROM history entry.
/// </summary>
[MessagePackObject]
public class SoftwareData
{
    /// <summary>
    /// The array of software items associated with a ROM history entry.
    /// </summary>
    [Key(0)]
    public ItemData[] Items { get; set; } = [];
}