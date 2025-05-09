using System.Xml.Serialization;

namespace SimpleLauncher.Models;

[XmlType("Favorite")]
public class XmlFavorite
{
    [XmlElement]
    public string FileName { get; set; }

    [XmlElement]
    public string SystemName { get; set; }
}