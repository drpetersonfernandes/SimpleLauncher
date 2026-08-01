using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

/// <summary>
/// Represents a single entry in the history document containing software, systems, or text.
/// </summary>
[MessagePackObject]
public class Entry
{
    /// <summary>
    /// Gets or sets the software information for this entry.
    /// </summary>
    [Key(0)]
    [XmlElement("software", IsNullable = true)]
    public Software? Software { get; set; }

    /// <summary>
    /// Gets or sets the systems information for this entry.
    /// </summary>
    [Key(1)]
    [XmlElement("systems", IsNullable = true)]
    public Systems? Systems { get; set; }

    /// <summary>
    /// Gets or sets the text content of this entry.
    /// </summary>
    [Key(2)]
    [XmlElement("text")]
    public string? Text { get; set; }
}