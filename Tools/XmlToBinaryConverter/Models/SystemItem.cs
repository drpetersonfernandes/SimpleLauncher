using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

/// <summary>
/// Represents a system entry with name and game information.
/// </summary>
[MessagePackObject]
public class SystemItem
{
    /// <summary>
    /// Gets or sets the name of the system.
    /// </summary>
    [Key(0)]
    [XmlAttribute("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the game associated with this system.
    /// </summary>
    [Key(1)]
    [XmlAttribute("game")]
    public string? Game { get; set; }
}