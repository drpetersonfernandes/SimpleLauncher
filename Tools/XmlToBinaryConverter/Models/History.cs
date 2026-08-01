using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

/// <summary>
/// Represents the root history document containing entries and metadata.
/// </summary>
[MessagePackObject]
[XmlRoot("history")]
public class History
{
    /// <summary>
    /// Gets or sets the version of the history document.
    /// </summary>
    [Key(0)]
    [XmlAttribute("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the date of the history document.
    /// </summary>
    [Key(1)]
    [XmlAttribute("date")]
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets the array of history entries.
    /// </summary>
    [Key(2)]
    [XmlElement("entry")]
    public Entry[]? Entries { get; set; } = [];
}