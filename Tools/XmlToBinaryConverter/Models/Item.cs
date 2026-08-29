using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

/// <summary>
///     Represents an item entry with list, name, and game attributes.
/// </summary>
[MessagePackObject]
public class Item
{
    /// <summary>
    ///     Gets or sets the list category for this item.
    /// </summary>
    [Key(0)]
    [XmlAttribute("list")]
    public string? List { get; set; }

    /// <summary>
    ///     Gets or sets the name of this item.
    /// </summary>
    [Key(1)]
    [XmlAttribute("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the game associated with this item.
    /// </summary>
    [Key(2)]
    [XmlAttribute("game")]
    public string? Game { get; set; }
}