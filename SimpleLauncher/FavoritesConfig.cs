using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SimpleLauncher
{
    [XmlRoot("Favorites")]
    public class FavoritesConfig
    {
        [XmlElement("Favorite")]
        public List<Favorite> FavoriteList { get; set; } = new List<Favorite>();
    }

    public class Favorite
    {
        public string FileName { get; set; }
        public string SystemName { get; set; }
    }

    public class FavoritesManager(
        string filePath)
    {

        public FavoritesConfig LoadFavorites()
        {
            if (!File.Exists(filePath))
            {
                return new FavoritesConfig();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(FavoritesConfig));
            using StreamReader reader = new StreamReader(filePath);
            return (FavoritesConfig)serializer.Deserialize(reader);
        }

        public void SaveFavorites(FavoritesConfig favorites)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FavoritesConfig));
            using StreamWriter writer = new StreamWriter(filePath);
            serializer.Serialize(writer, favorites);
        }
    }
}