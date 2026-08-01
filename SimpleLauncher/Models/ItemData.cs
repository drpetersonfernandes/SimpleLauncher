using MessagePack;

namespace SimpleLauncher.Models;

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
