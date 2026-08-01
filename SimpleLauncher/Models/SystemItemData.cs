using MessagePack;

namespace SimpleLauncher.Models;

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
