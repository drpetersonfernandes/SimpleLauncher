using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

[MessagePackObject]
public class PlayHistoryManager
{
    // This collection will be serialized.
    [Key(0)]
    public ObservableCollection<PlayHistoryItem> PlayHistoryList { get; set; } = [];

    // The data file path.
    private static string FilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playhistory.dat");

    // Constants for date and time formats
    private const string IsoDateFormat = "yyyy-MM-dd";
    private const string IsoTimeFormat = "HH:mm:ss";

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

        try
        {
            var bytes = File.ReadAllBytes(FilePath);
            var manager = MessagePackSerializer.Deserialize<PlayHistoryManager>(bytes);

            // Migrate old date formats to new ISO format if needed
            manager.MigrateOldDateFormats();

            return manager;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error loading play history");

            return new PlayHistoryManager(); // Return default instance if error occurs
        }
    }

    /// <summary>
    /// Migrates any records with old date formats to the new culture-invariant ISO format
    /// </summary>
    private void MigrateOldDateFormats()
    {
        var needsSaving = false;

        foreach (var item in PlayHistoryList)
        {
            // Check if date is already in ISO format
            var isIsoDate = IsIsoDateFormat(item.LastPlayDate);
            var isIsoTime = IsIsoTimeFormat(item.LastPlayTime);

            if (isIsoDate && isIsoTime) continue;

            // Convert the old format to ISO format
            if (!TryParseAndConvertDate(item.LastPlayDate, item.LastPlayTime,
                    out var newDate, out var newTime)) continue;

            item.LastPlayDate = newDate;
            item.LastPlayTime = newTime;
            needsSaving = true;
        }

        // Save the updated data if any records were migrated
        if (needsSaving)
        {
            SavePlayHistory();
        }
    }

    /// <summary>
    /// Checks if a date string is in ISO format (yyyy-MM-dd)
    /// </summary>
    private static bool IsIsoDateFormat(string dateStr)
    {
        return DateTime.TryParseExact(dateStr, IsoDateFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Checks if a time string is in ISO time format (HH:mm:ss)
    /// </summary>
    private static bool IsIsoTimeFormat(string timeStr)
    {
        return DateTime.TryParseExact(timeStr, IsoTimeFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Attempts to parse a date/time from any format and convert to ISO format
    /// </summary>
    private static bool TryParseAndConvertDate(string dateStr, string timeStr,
        out string newDateStr, out string newTimeStr)
    {
        // Initialize out parameters
        newDateStr = dateStr;
        newTimeStr = timeStr;

        try
        {
            // Try parsing with various methods

            // Try combined string with current culture
            if (DateTime.TryParse($"{dateStr} {timeStr}", out var dateTime))
            {
                newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
                newTimeStr = dateTime.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
                return true;
            }

            // Try with invariant culture
            if (DateTime.TryParse($"{dateStr} {timeStr}",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
                newTimeStr = dateTime.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
                return true;
            }

            // Try with common formats
            string[] dateFormats = ["MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "d", "D", "yyyy.MM.dd", "dd.MM.yyyy"];
            foreach (var df in dateFormats)
            {
                if (!DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {IsoTimeFormat}", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out dateTime)) continue;

                newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
                newTimeStr = dateTime.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
                return true;
            }

            // If we can at least parse the date part
            foreach (var df in dateFormats)
            {
                if (!DateTime.TryParseExact(dateStr, df, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out dateTime)) continue;

                newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);

                // Try to parse time part separately
                if (TimeSpan.TryParse(timeStr, out var timeSpan))
                {
                    newTimeStr = timeSpan.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    newTimeStr = "00:00:00"; // Default if time can't be parsed
                }

                return true;
            }

            // If everything fails, create a timestamp from current time
            // but only as a last resort since this loses the original timestamp
            dateTime = DateTime.Now;
            newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
            newTimeStr = dateTime.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);

            // Notify developer
            const string contextMessage = "Failed to parse date/time, using current time as fallback";
            _ = LogErrors.LogErrorAsync(new Exception($"{contextMessage}: {dateStr} {timeStr}"), contextMessage);

            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            // Log the error but don't crash
            const string contextMessage = "Error in date format migration";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            return false;
        }
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

            // Get the current date and time in a culture-invariant format
            // This ensures it can be parsed regardless of the UI language
            var now = DateTime.Now;
            var dateStr = now.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
            var timeStr = now.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);

            // Check if the game already exists in play history
            var existingItem = PlayHistoryList.FirstOrDefault(item =>
                item.FileName == fileName && item.SystemName == systemName);

            if (existingItem != null)
            {
                // Update existing record
                existingItem.TotalPlayTime += (long)playTime.TotalSeconds;
                existingItem.TimesPlayed += 1;
                existingItem.LastPlayDate = dateStr;
                existingItem.LastPlayTime = timeStr;
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
                    LastPlayDate = dateStr,
                    LastPlayTime = timeStr
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