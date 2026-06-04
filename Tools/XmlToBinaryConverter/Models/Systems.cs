using System.Xml.Serialization;
using MessagePack;

namespace XmlToBinaryConverter.Models;

[MessagePackObject]
public class Systems
{
    [Key(0)]
    [XmlElement("system")]
    public SystemItem[] SystemItems { get; set; } = [];
}