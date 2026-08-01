using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Contains a collection of system items associated with a ROM history entry.
/// </summary>
[MessagePackObject]
public class SystemsData
{
    [Key(0)]
    public SystemItemData[] SystemItems { get; set; } = [];
}
