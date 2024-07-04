using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace SimpleLauncher
{
    [XmlRoot("Favorites")]
    public class FavoritesConfig
    {
        [XmlElement("Favorite")]
        public ObservableCollection<Favorite> FavoriteList { get; set; } = new ObservableCollection<Favorite>();

        public static string FilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.xml");
    }

    public class Favorite
    {
        public string FileName { get; set; }
        public string SystemName { get; set; }
        public string MachineDescription { get; set; }
    }

    public class FavoritesManager
    {
        private readonly string _filePath = FavoritesConfig.FilePath;

        public FavoritesConfig LoadFavorites()
        {
            if (!File.Exists(_filePath))
            {
                return new FavoritesConfig();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(FavoritesConfig));
            using StreamReader reader = new StreamReader(_filePath);
            return (FavoritesConfig)serializer.Deserialize(reader);
        }

        public void SaveFavorites(FavoritesConfig favorites)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FavoritesConfig));
            using StreamWriter writer = new StreamWriter(_filePath);
            serializer.Serialize(writer, favorites);
        }
    }
}