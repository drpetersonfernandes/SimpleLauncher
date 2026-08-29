using MessagePack;

namespace SimpleLauncher.Core.Models;

/// <summary>
///     Contains a collection of system items associated with a ROM history entry.
/// </summary>
[MessagePackObject]
public class SystemsData
{
    /// <summary>
    ///     The array of system items associated with a ROM history entry.
    /// </summary>
    [Key(0)]
    public SystemItemData[] SystemItems { get; set; } = [];
}