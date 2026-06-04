using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

[MessagePackObject]
public class SystemItem
{
    [Key(0)]
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [Key(1)]
    [XmlAttribute("game")]
    public string? Game { get; set; }
}