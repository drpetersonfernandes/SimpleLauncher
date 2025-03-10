using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SimpleLauncher;

[XmlRoot("PlayHistory")]
public class PlayHistoryManager
{
    // This collection will be serialized.
    [XmlElement("PlayHistoryItem")]
    public ObservableCollection<PlayHistoryItem> PlayHistoryList { get; set; } = [];

    // The XML file path.
    public static string FilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playhistory.xml");

    /// <summary>
    /// Loads play history from the XML file. If the file doesn't exist, creates and saves a new instance.
    /// </summary>
    public static PlayHistoryManager LoadPlayHistory()
    {
        if (!File.Exists(FilePath))
        {
            var defaultManager = new PlayHistoryManager();
            defaultManager.SavePlayHistory(); // Use instance method
            return defaultManager;
        }

        var serializer = new XmlSerializer(typeof(PlayHistoryManager));
        using var reader = new StreamReader(FilePath);
        return (PlayHistoryManager)serializer.Deserialize(reader);
    }

    /// <summary>
    /// Saves the provided play history to the XML file.
    /// </summary>
    public void SavePlayHistory()
    {
        var serializer = new XmlSerializer(typeof(PlayHistoryManager));
        using var writer = new StreamWriter(FilePath);
        serializer.Serialize(writer, this);
    }

    /// <summary>
    /// Adds or updates a play history item based on the game info and play time.
    /// </summary>
    public void AddOrUpdatePlayHistoryItem(string fileName, string systemName, TimeSpan playTime)
    {
        try
        {
            // Skip if playtime is less than 5 seconds (likely just a quick launch/close)
            if (playTime.TotalSeconds < 5)
                return;

            // Check if the game already exists in play history
            var existingItem = PlayHistoryList.FirstOrDefault(item =>
                item.FileName == fileName && item.SystemName == systemName);

            if (existingItem != null)
            {
                // Update existing record
                existingItem.TotalPlayTime += (long)playTime.TotalSeconds;
                existingItem.TimesPlayed += 1;
                existingItem.LastPlayDate = DateTime.Now.ToShortDateString();
                existingItem.LastPlayTime = DateTime.Now.ToShortTimeString();
            }
            else
            {
                // Create new record
                var newItem = new PlayHistoryItem
                {
                    FileName = fileName,
                    SystemName = systemName,
                    TotalPlayTime = (long)playTime.TotalSeconds,
                    TimesPlayed = 1,
                    LastPlayDate = DateTime.Now.ToShortDateString(),
                    LastPlayTime = DateTime.Now.ToShortTimeString()
                };
                PlayHistoryList.Add(newItem);
            }

            // Save the updated list
            SavePlayHistory();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in AddOrUpdatePlayHistoryItem method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }
}

public class PlayHistoryItem
{
    public string FileName { get; set; }
    public string SystemName { get; set; }
    public long TotalPlayTime { get; set; } // In seconds
    public int TimesPlayed { get; set; }
    public string LastPlayDate { get; set; }
    public string LastPlayTime { get; set; }

    [XmlIgnore]
    public string MachineDescription { get; set; }

    [XmlIgnore]
    public string CoverImage { get; set; }

    [XmlIgnore]
    public string DefaultEmulator { get; set; }

    [XmlIgnore]
    public string FormattedPlayTime
    {
        get
        {
            var timeSpan = TimeSpan.FromSeconds(TotalPlayTime);
            return timeSpan.TotalHours >= 1
                ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
    }
}