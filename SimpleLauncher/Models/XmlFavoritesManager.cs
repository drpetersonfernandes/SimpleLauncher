using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace SimpleLauncher.Models;

// Legacy XML format for backward compatibility
[XmlRoot("Favorites")]
public class XmlFavoritesManager
{
    [XmlElement("Favorite")]
    public ObservableCollection<XmlFavorite> FavoriteList { get; set; } = [];
}