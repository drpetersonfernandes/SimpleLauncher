using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a system entry with a name and associated game.
/// </summary>
[MessagePackObject]
public class SystemItemData
{
    /// <summary>
    /// The name of the system or platform.
    /// </summary>
    [Key(0)]
    public string? Name { get; set; }

    /// <summary>
    /// The name of the game or ROM associated with this entry.
    /// </summary>
    [Key(1)]
    public string? Game { get; set; }
}
