using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SimpleLauncher;

[XmlRoot("Favorites")]
public class FavoritesManager
{
    // This collection will be serialized.
    [XmlElement("Favorite")]
    public ObservableCollection<Favorite> FavoriteList { get; set; } = [];

    // The XML file path.
    public static string FilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.xml");

    /// <summary>
    /// Loads favorites from the XML file. If the file doesn't exist, creates and saves a new instance.
    /// </summary>
    public static FavoritesManager LoadFavorites()
    {
        if (!File.Exists(FilePath))
        {
            var defaultManager = new FavoritesManager();
            defaultManager.SaveFavorites(); // Use instance method
            return defaultManager;
        }

        var serializer = new XmlSerializer(typeof(FavoritesManager));
        using var reader = new StreamReader(FilePath);
        return (FavoritesManager)serializer.Deserialize(reader);
    }

    /// <summary>
    /// Saves the provided favorites to the XML file.
    /// The favorites are ordered by FileName before saving.
    /// </summary>
    public void SaveFavorites()
    {
        // Order the favorites only by FileName
        var orderedFavorites = new ObservableCollection<Favorite>(
            FavoriteList.OrderBy(fav => fav.FileName)
        );
        FavoriteList = orderedFavorites;

        var serializer = new XmlSerializer(typeof(FavoritesManager));
        using var writer = new StreamWriter(FilePath);
        serializer.Serialize(writer, this);
    }
}

public class Favorite
{
    // Only FileName and SystemName will be serialized.
    public string FileName { get; init; }
    public string SystemName { get; init; }

    // These properties will be ignored during XML serialization.
    [XmlIgnore]
    public string MachineDescription { get; init; }

    [XmlIgnore]
    public string CoverImage { get; init; }

    [XmlIgnore]
    public string DefaultEmulator { get; set; }
}