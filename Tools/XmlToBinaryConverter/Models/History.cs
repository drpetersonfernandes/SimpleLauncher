using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

[MessagePackObject]
[XmlRoot("history")]
public class History
{
    [Key(0)]
    [XmlAttribute("version")]
    public string? Version { get; set; }

    [Key(1)]
    [XmlAttribute("date")]
    public string? Date { get; set; }

    [Key(2)]
    [XmlElement("entry")]
    public Entry[]? Entries { get; set; } = [];
}