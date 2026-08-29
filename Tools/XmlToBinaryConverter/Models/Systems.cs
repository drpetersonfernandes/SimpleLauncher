using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

/// <summary>
///     Represents a collection of system items for XML/binary serialization.
/// </summary>
[MessagePackObject]
public class Systems
{
    /// <summary>
    ///     Gets or sets the array of system items.
    /// </summary>
    [Key(0)]
    [XmlElement("system")]
    public SystemItem[] SystemItems { get; set; } = [];
}