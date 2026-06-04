using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

[MessagePackObject]
public class Software
{
    [Key(0)]
    [XmlElement("item")]
    public Item[] Items { get; set; } = [];
}