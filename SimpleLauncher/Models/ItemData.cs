using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a software item with a list name, display name, and associated game.
/// </summary>
[MessagePackObject]
public class ItemData
{
    /// <summary>
    /// Gets or sets the list name this item belongs to.
    /// </summary>
    [Key(0)]
    public string? List { get; set; }

    /// <summary>
    /// Gets or sets the display name of the item.
    /// </summary>
    [Key(1)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the associated game reference for this item.
    /// </summary>
    [Key(2)]
    public string? Game { get; set; }
}
