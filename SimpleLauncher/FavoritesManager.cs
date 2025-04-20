using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using MessagePack;
using SimpleLauncher.Services;

namespace SimpleLauncher;

[MessagePackObject]
public class FavoritesManager
{
    // This collection will be serialized with MessagePack
    [Key(0)]
    public ObservableCollection<Favorite> FavoriteList { get; set; } = [];

    // The file paths for both formats.
    private static string DatFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat");
    private static string XmlFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.xml");

    /// <summary>
    /// Loads favorites from the DAT file. If the DAT file doesn't exist,
    /// attempts to convert from XML if it exists, or create a new instance.
    /// Also handles deletion of old XML files if marked for deletion.
    /// </summary>
    public static FavoritesManager LoadFavorites()
    {
        // Check for and handle any XML file marked for deletion from previous session
        CheckForXmlDeletionMarker();

        // First, try to load from the new MessagePack format
        if (File.Exists(DatFilePath))
        {
            try
            {
                var bytes = File.ReadAllBytes(DatFilePath);
                return MessagePackSerializer.Deserialize<FavoritesManager>(bytes);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error loading favorites.dat";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }

        // If DAT file doesn't exist or couldn't be loaded, check if XML exists for conversion
        if (File.Exists(XmlFilePath))
        {
            return ConvertXmlToDat();
        }

        // If no files exist, create a new instance
        var defaultManager = new FavoritesManager();
        defaultManager.SaveFavorites(); // Use instance method
        return defaultManager;
    }

    /// <summary>
    /// Checks for any XML files marked for deletion from previous runs and deletes them.
    /// </summary>
    private static void CheckForXmlDeletionMarker()
    {
        var markerPath = XmlFilePath + ".delete";
        if (!File.Exists(markerPath)) return;

        try
        {
            // Delete the marker first
            File.Delete(markerPath);

            // Then try to delete the XML file
            if (File.Exists(XmlFilePath))
            {
                File.Delete(XmlFilePath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error deleting marked favorites.xml";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    /// <summary>
    /// Converts favorites from XML to DAT format and returns the loaded FavoritesManager.
    /// If conversion is successful, schedule the XML file for deletion at the next application start.
    /// </summary>
    private static FavoritesManager ConvertXmlToDat()
    {
        XmlFavoritesManager xmlManager = null;

        // Read the XML file in a separate block to ensure it's fully closed
        try
        {
            // Create an XML serializer for the legacy format
            var serializer = new XmlSerializer(typeof(XmlFavoritesManager));

            // Create secure XML reader settings
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit, // Disable DTD processing
                XmlResolver = null, // Disable external references
                CloseInput = true // Ensure the reader is closed
            };

            using var reader = XmlReader.Create(XmlFilePath, settings);
            // Deserialize using the legacy format
            xmlManager = (XmlFavoritesManager)serializer.Deserialize(reader);
        }
        catch (Exception ex)
        {
            // Log error during XML read
            const string contextMessage = "Error reading favorites.xml for conversion";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }

        // Process the data if read was successful
        if (xmlManager != null)
        {
            try
            {
                // Convert from XML format to MessagePack format
                var manager = new FavoritesManager
                {
                    FavoriteList = new ObservableCollection<Favorite>(
                        xmlManager.FavoriteList.Select(static xmlFav => new Favorite
                        {
                            FileName = xmlFav.FileName,
                            SystemName = xmlFav.SystemName
                            // Copy other properties as needed
                        })
                    )
                };

                // Save to DAT
                manager.SaveFavorites();

                // Mark the old XML file for deletion on next start
                // Instead of deleting immediately, we'll create a marker file
                try
                {
                    var markerPath = XmlFilePath + ".delete";
                    File.WriteAllText(markerPath, "This file indicates that favorites.xml should be deleted on next application start.");
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error creating marker for favorites.xml deletion";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);
                }

                return manager;
            }
            catch (Exception ex)
            {
                // Log error during the conversion process
                const string contextMessage = "Error during favorites XML to DAT conversion";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }

        // Fallback to new instance
        var defaultManager = new FavoritesManager();
        defaultManager.SaveFavorites();
        return defaultManager;
    }

    /// <summary>
    /// Saves the provided favorites to the DAT file.
    /// The favorites are ordered by FileName before saving.
    /// </summary>
    public void SaveFavorites()
    {
        // Order the favorites only by FileName
        var orderedFavorites = new ObservableCollection<Favorite>(
            FavoriteList.OrderBy(static fav => fav.FileName)
        );
        FavoriteList = orderedFavorites;

        try
        {
            // Serialize using MessagePack
            var bytes = MessagePackSerializer.Serialize(this);
            File.WriteAllBytes(DatFilePath, bytes);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error saving favorites.dat";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            throw; // Re-throw to notify caller
        }
    }
}

// Legacy XML format for backward compatibility
[XmlRoot("Favorites")]
public class XmlFavoritesManager
{
    [XmlElement("Favorite")]
    public ObservableCollection<XmlFavorite> FavoriteList { get; set; } = [];
}

[XmlType("Favorite")]
public class XmlFavorite
{
    [XmlElement]
    public string FileName { get; set; }

    [XmlElement]
    public string SystemName { get; set; }
}

// New MessagePack format
[MessagePackObject]
public class Favorite
{
    [Key(0)]
    public string FileName { get; init; }

    [Key(1)]
    public string SystemName { get; init; }

    [IgnoreMember]
    public string MachineDescription { get; init; }

    [IgnoreMember]
    public string CoverImage { get; init; }

    [IgnoreMember]
    public string DefaultEmulator { get; set; }
}