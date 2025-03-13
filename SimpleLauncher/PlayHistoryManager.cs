using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MessagePack;

namespace SimpleLauncher;

[MessagePackObject]
public class PlayHistoryManager
{
    // This collection will be serialized.
    [Key(0)]
    public ObservableCollection<PlayHistoryItem> PlayHistoryList { get; set; } = [];

    // The data file path.
    private static string FilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playhistory.dat");

    /// <summary>
    /// Loads play history from the MessagePack file. If the file doesn't exist, creates and saves a new instance.
    /// </summary>
    public static PlayHistoryManager LoadPlayHistory()
    {
        if (!File.Exists(FilePath))
        {
            var defaultManager = new PlayHistoryManager();
            defaultManager.SavePlayHistory(); // Use instance method
            return defaultManager;
        }

        var bytes = File.ReadAllBytes(FilePath);
        return MessagePackSerializer.Deserialize<PlayHistoryManager>(bytes);
    }

    /// <summary>
    /// Saves the provided play history to the MessagePack file.
    /// </summary>
    public void SavePlayHistory()
    {
        var bytes = MessagePackSerializer.Serialize(this);
        File.WriteAllBytes(FilePath, bytes);
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
                existingItem.LastPlayTime = DateTime.Now.ToString("HH:mm:ss");
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
                    LastPlayTime = DateTime.Now.ToString("HH:mm:ss")
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

[MessagePackObject]
public class PlayHistoryItem
{
    [Key(0)]
    public string FileName { get; set; }

    [Key(1)]
    public string SystemName { get; set; }

    [Key(2)]
    public long TotalPlayTime { get; set; } // In seconds

    [Key(3)]
    public int TimesPlayed { get; set; }

    [Key(4)]
    public string LastPlayDate { get; set; }

    [Key(5)]
    public string LastPlayTime { get; set; }

    [IgnoreMember]
    public string MachineDescription { get; set; }

    [IgnoreMember]
    public string CoverImage { get; set; }

    [IgnoreMember]
    public string DefaultEmulator { get; set; }

    [IgnoreMember]
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