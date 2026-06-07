using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Services.AppDataFile;
using SimpleLauncher.Services.DebugAndBugReport;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.PlayHistory;

[MessagePackObject(AllowPrivate = true)]
public class PlayHistoryManager
{
    [IgnoreMember] private readonly object _historyLock = new();
    [IgnoreMember] private ILogErrors _logErrors;
    [IgnoreMember] private static readonly DataFileLocation FileLocation = new("playhistory.dat");

    // This collection will be serialized.
    [Key(0)] public ObservableCollection<PlayHistoryItem> PlayHistoryList { get; set; } = [];

    [Key(1)] public int Version { get; set; } = 1;

    private static string FilePath => FileLocation.FilePath;
    private static string TempFilePath => FileLocation.TempFilePath;

    public static bool IsPortableMode => FileLocation.IsPortableMode;

    // Constants for date and time formats
    private const string IsoDateFormat = "yyyy-MM-dd";
    private const string IsoTimeFormat = "HH:mm:ss";

    /// <summary>
    /// Sets the error logger for this instance.
    /// </summary>
    internal void SetLogger(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    /// <summary>
    /// Loads play history from the MessagePack file. If the file doesn't exist, creates and saves a new instance.
    /// </summary>
    internal static PlayHistoryManager LoadPlayHistory(ILogErrors logErrors = null)
    {
        if (!File.Exists(FilePath))
        {
            var defaultManager = new PlayHistoryManager { _logErrors = logErrors };
            defaultManager.SavePlayHistoryAsync();
            return defaultManager;
        }

        try
        {
            var bytes = File.ReadAllBytes(FilePath);
            var manager = MessagePackSerializer.Deserialize<PlayHistoryManager>(bytes);
            manager._logErrors = logErrors;

            // Migrate old date formats to new ISO format if needed
            manager.MigrateOldDateFormats();

            return manager;
        }
        catch (Exception ex)
        {
            // Notify developer
            logErrors?.LogAndForget(ex, "Error loading play history");

            return new PlayHistoryManager { _logErrors = logErrors }; // Return default instance if error occurs
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
            SavePlayHistoryAsync();
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
    private bool TryParseAndConvertDate(string dateStr, string timeStr,
        out string newDateStr, out string newTimeStr)
    {
        // Initialize out parameters
        newDateStr = dateStr;
        newTimeStr = timeStr;

        try
        {
            // Try ISO format first (the target format we want to ensure is handled correctly)
            if (DateTime.TryParseExact($"{dateStr} {timeStr}", $"{IsoDateFormat} {IsoTimeFormat}",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
                newTimeStr = dateTime.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
                return true;
            }

            // Try explicit unambiguous formats using InvariantCulture only.
            // We avoid current culture parsing to prevent incorrect interpretation
            // when users switch OS region settings (e.g., US MM/dd/yyyy vs UK dd/MM/yyyy).
            // Note: For ambiguous dates like 01/02/2024, MM/dd/yyyy (US) will be attempted first.
            string[] dateFormats =
            [
                "yyyy/MM/dd", "yyyy.MM.dd", "dd.MM.yyyy",
                "MM/dd/yyyy", "dd/MM/yyyy", "d", "D"
            ];
            foreach (var df in dateFormats)
            {
                if (DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {IsoTimeFormat}", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out dateTime))
                {
                    newDateStr = dateTime.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
                    newTimeStr = dateTime.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
                    return true;
                }
            }

            // Fallback: Try with InvariantCulture (assumes US format for ambiguous dates like 01/02/2024 -> Jan 2)
            if (DateTime.TryParse($"{dateStr} {timeStr}",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
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
            _logErrors?.LogAndForget(null, contextMessage);

            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in date format migration";
            _logErrors?.LogAndForget(ex, contextMessage);

            return false;
        }
    }

    /// <summary>
    /// Saves the provided play history to the MessagePack file asynchronously.
    /// </summary>
    internal Task SavePlayHistoryAsync()
    {
        // Serialize and write on a background thread so Thread.Sleep in the
        // retry loop does not block the UI thread.
        return Task.Run(() =>
        {
            const int maxRetries = 3;
            var retryDelayMs = 500;
            Exception lastException = null;
            var attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    // Notify user
                    Application.Current.Dispatcher.Invoke(static () => (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SavingPlayHistory") ?? "Saving play history...", Application.Current.MainWindow as MainWindow));

                    byte[] bytes;
                    lock (_historyLock)
                    {
                        bytes = MessagePackSerializer.Serialize(this);
                    }

                    // Write to a temporary file first to prevent data loss on crash
                    File.WriteAllBytes(TempFilePath, bytes);

                    // Atomically replace the main file with the temp file
                    File.Move(TempFilePath, FilePath, true);
                    return; // Success
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
                {
                    lastException = ex;
                    attempt++;

                    // If in portable mode, try falling back to LocalAppData and reset retries
                    if (FileLocation.IsPortableMode && attempt >= maxRetries)
                    {
                        try
                        {
                            if (FileLocation.TryFallbackToLocalAppData())
                            {
                                attempt = 0;
                                continue;
                            }
                        }
                        catch
                        {
                            // Fallback failed, continue with normal error handling
                        }
                    }

                    if (attempt < maxRetries)
                    {
                        // Attempt to clean up temp file before retrying
                        try
                        {
                            if (File.Exists(TempFilePath))
                            {
                                File.Delete(TempFilePath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }

                        Thread.Sleep(retryDelayMs);
                        retryDelayMs *= 2; // Exponential backoff
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break; // Don't retry non-transient errors
                }
            }

            // All retries exhausted or non-transient error
            _logErrors?.LogAndForget(lastException, "Error saving playhistory.dat");

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
                _logErrors?.LogAndForget(cleanupEx, "Error cleaning up temporary play history file after failed save");
            }
        });
    }

    /// <summary>
    /// Adds or updates a play history item based on the game info and play time.
    /// </summary>
    internal void AddOrUpdatePlayHistoryItem(string fullPath, string systemName, TimeSpan playTime)
    {
        try
        {
            // Skip if playtime is less than 5 seconds (likely just a quick launch/close)
            if (playTime.TotalSeconds < 5)
                return;

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("UpdatingPlayHistory") ?? "Updating play history...", Application.Current.MainWindow as MainWindow));

            // Get the current date and time in a culture-invariant format
            // This ensures it can be parsed regardless of the UI language
            var now = DateTime.Now;
            var dateStr = now.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
            var timeStr = now.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);

            PlayHistoryItem itemToAdd = null;

            lock (_historyLock)
            {
                // Check if the game already exists in play history
                var existingItem = PlayHistoryList.FirstOrDefault(item => item.FileName.Equals(fullPath, StringComparison.OrdinalIgnoreCase) && item.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

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
                    itemToAdd = new PlayHistoryItem
                    {
                        FileName = fullPath,
                        SystemName = systemName,
                        TotalPlayTime = (long)playTime.TotalSeconds,
                        TimesPlayed = 1,
                        LastPlayDate = dateStr,
                        LastPlayTime = timeStr
                    };
                }
            }

            // Add to the ObservableCollection outside the lock to prevent deadlock
            if (itemToAdd != null)
            {
                Application.Current.Dispatcher.Invoke(() => PlayHistoryList.Add(itemToAdd));
            }

            Application.Current.Dispatcher.Invoke(SavePlayHistoryAsync);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in AddOrUpdatePlayHistoryItem method.";
            _logErrors?.LogAndForget(ex, contextMessage);
        }
    }

    /// <summary>
    /// Migrates old records that only contain filenames to full absolute paths.
    /// </summary>
    internal void MigrateFilenamesToFullPaths(List<SystemManager.SystemManager> systemManagers)
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

        if (needsSave)
        {
            SavePlayHistoryAsync();
        }
    }
}