using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

/// <summary>
/// Represents a collection of software items for XML/binary serialization.
/// </summary>
[MessagePackObject]
public class Software
{
    /// <summary>
    /// Gets or sets the array of software items.
    /// </summary>
    [Key(0)]
    [XmlElement("item")]
    public Item[] Items { get; set; } = [];
}