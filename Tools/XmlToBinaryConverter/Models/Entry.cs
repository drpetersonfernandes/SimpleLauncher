using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

[MessagePackObject]
public class Entry
{
    [Key(0)]
    [XmlElement("software", IsNullable = true)]
    public Software? Software { get; set; }

    [Key(1)]
    [XmlElement("systems", IsNullable = true)]
    public Systems? Systems { get; set; }

    [Key(2)]
    [XmlElement("text")]
    public string? Text { get; set; }
}