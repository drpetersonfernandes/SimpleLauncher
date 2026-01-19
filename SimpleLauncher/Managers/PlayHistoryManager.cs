using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
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
    private static string TempFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playhistory.dat.tmp");

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
            defaultManager.SavePlayHistory();
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading play history");

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new Exception($"{contextMessage}: {dateStr} {timeStr}"), contextMessage);

            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in date format migration";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            return false;
        }
    }

    /// <summary>
    /// Saves the provided play history to the MessagePack file.
    /// </summary>
    public void SavePlayHistory()
    {
        try
        {
            // Notify user
            Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingPlayHistory") ?? "Saving play history...", Application.Current.MainWindow as MainWindow));

            var bytes = MessagePackSerializer.Serialize(this);

            // Write to a temporary file first to prevent data loss on crash
            File.WriteAllBytes(TempFilePath, bytes);

            // Atomically replace the main file with the temp file
            File.Move(TempFilePath, FilePath, true);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error saving playhistory.dat";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Attempt to clean up temp file if it exists
            try
            {
                if (File.Exists(TempFilePath))
                {
                    File.Delete(TempFilePath);
                }
            }
            catch (Exception cleanupEx)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(cleanupEx, "Error cleaning up temporary play history file after failed save");
            }
        }
    }

    /// <summary>
    /// Adds or updates a play history item based on the game info and play time.
    /// </summary>
    public void AddOrUpdatePlayHistoryItem(string fullPath, string systemName, TimeSpan playTime)
    {
        try
        {
            // Skip if playtime is less than 5 seconds (likely just a quick launch/close)
            if (playTime.TotalSeconds < 5)
                return;

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("UpdatingPlayHistory") ?? "Updating play history...", Application.Current.MainWindow as MainWindow));

            // Get the current date and time in a culture-invariant format
            // This ensures it can be parsed regardless of the UI language
            var now = DateTime.Now;
            var dateStr = now.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
            var timeStr = now.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);

            // Check if the game already exists in play history
            var existingItem = PlayHistoryList.FirstOrDefault(item => item.FileName.Equals(fullPath, StringComparison.OrdinalIgnoreCase) && item.SystemName == systemName);

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
                    FileName = fullPath,
                    SystemName = systemName,
                    TotalPlayTime = (long)playTime.TotalSeconds,
                    TimesPlayed = 1,
                    LastPlayDate = dateStr,
                    LastPlayTime = timeStr
                };
                Application.Current.Dispatcher.Invoke(() => PlayHistoryList.Add(newItem));
            }

            Application.Current.Dispatcher.Invoke(() => SavePlayHistory());
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in AddOrUpdatePlayHistoryItem method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    /// <summary>
    /// Migrates old records that only contain filenames to full absolute paths.
    /// </summary>
    public void MigrateFilenamesToFullPaths(List<SystemManager> systemManagers)
    {
        var needsSave = false;
        foreach (var item in PlayHistoryList)
        {
            // If the path is not rooted, it's an old "filename only" record
            if (!Path.IsPathRooted(item.FileName))
            {
                var system = systemManagers.FirstOrDefault(s => s.SystemName.Equals(item.SystemName, StringComparison.OrdinalIgnoreCase));
                if (system != null)
                {
                    var resolvedPath = PathHelper.FindFileInSystemFolders(system, item.FileName);
                    if (!string.IsNullOrEmpty(resolvedPath))
                    {
                        item.FileName = resolvedPath;
                        needsSave = true;
                    }
                }
            }
        }

        if (needsSave) SavePlayHistory();
    }
}