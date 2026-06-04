using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

[MessagePackObject]
public class Item
{
    [Key(0)]
    [XmlAttribute("list")]
    public string? List { get; set; }

    [Key(1)]
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [Key(2)]
    [XmlAttribute("game")]
    public string? Game { get; set; }
}