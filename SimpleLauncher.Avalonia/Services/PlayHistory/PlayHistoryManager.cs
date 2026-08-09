using System.Collections.ObjectModel;
using System.Globalization;
using MessagePack;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services;
using ILogger = Serilog.ILogger;

namespace SimpleLauncher.Avalonia.Services.PlayHistory;

/// <summary>
/// Manages play history tracking, persistence, and date format migration using MessagePack serialization.
/// Compatible with the existing playhistory.dat format from SimpleLauncher.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public class PlayHistoryManager
{
    [IgnoreMember] private readonly Lock _historyLock = new();
    [IgnoreMember] private ILogger? _logger;
    [IgnoreMember] private static readonly DataFileLocation FileLocation = new("playhistory.dat");

    [Key(0)]
    public ObservableCollection<PlayHistoryItem> PlayHistoryList { get; set; } = [];

    [Key(1)]
    public int Version { get; set; } = 1;

    private static string FilePath => FileLocation.FilePath;
    private static string TempFilePath => FileLocation.TempFilePath;
    public static bool IsPortableMode => FileLocation.IsPortableMode;

    private const string IsoDateFormat = "yyyy-MM-dd";
    private const string IsoTimeFormat = "HH:mm:ss";

    /// <summary>
    /// Loads play history from the MessagePack file. Creates new if doesn't exist.
    /// </summary>
    public static PlayHistoryManager LoadPlayHistory(ILogger? logErrors = null)
    {
        if (!File.Exists(FilePath))
        {
            var defaultManager = new PlayHistoryManager { _logger = logErrors };
            // Write the initial file synchronously. This runs on the UI thread at startup —
            // awaiting the async save via GetAwaiter().GetResult() would deadlock.
            defaultManager.SavePlayHistorySync();
            return defaultManager;
        }

        try
        {
            var bytes = File.ReadAllBytes(FilePath);
            var manager = MessagePackSerializer.Deserialize<PlayHistoryManager>(bytes);
            manager._logger = logErrors;
            return manager;
        }
        catch (Exception ex)
        {
            logErrors?.Error(ex, "Error loading playhistory.dat");
        }

        var newManager = new PlayHistoryManager { _logger = logErrors };
        // Synchronous recovery save (same UI-thread deadlock protection as above)
        newManager.SavePlayHistorySync();
        return newManager;
    }

    /// <summary>
    /// Synchronous initial save (startup/recovery paths only). Mirrors
    /// <see cref="SavePlayHistoryAsync"/> with retry logic, but never awaits —
    /// safe to call on the UI thread.
    /// </summary>
    private void SavePlayHistorySync()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var bytes = MessagePackSerializer.Serialize(this);
                File.WriteAllBytes(TempFilePath, bytes);
                File.Move(TempFilePath, FilePath, true);
                return;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error saving playhistory.dat (attempt {Attempt})", attempt + 1);
                if (attempt < 2) Thread.Sleep(100);
            }
        }
    }

    /// <summary>
    /// Saves play history atomically with retry logic.
    /// </summary>
    public async Task SavePlayHistoryAsync()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var bytes = MessagePackSerializer.Serialize(this);
                await File.WriteAllBytesAsync(TempFilePath, bytes);
                File.Move(TempFilePath, FilePath, true);
                return;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error saving playhistory.dat (attempt {Attempt})", attempt + 1);
                if (attempt < 2) await Task.Delay(100);
            }
        }
    }

    /// <summary>
    /// Records a play event for the given game. Increments play count and updates timestamps.
    /// </summary>
    public Task RecordPlayAsync(string filePath, string systemName, long playTimeSeconds = 0)
    {
        lock (_historyLock)
        {
            var entry = PlayHistoryList.FirstOrDefault(h =>
                string.Equals(h.FileName, filePath, StringComparison.OrdinalIgnoreCase));

            if (entry is null)
            {
                entry = new PlayHistoryItem
                {
                    FileName = filePath,
                    SystemName = systemName
                };
                PlayHistoryList.Add(entry);
            }

            entry.TimesPlayed++;
            entry.TotalPlayTime += playTimeSeconds;
            entry.LastPlayDate = DateTime.Now.ToString(IsoDateFormat, CultureInfo.InvariantCulture);
            entry.LastPlayTime = DateTime.Now.ToString(IsoTimeFormat, CultureInfo.InvariantCulture);
        }

        return SavePlayHistoryAsync();
    }

    /// <summary>
    /// Gets a dictionary of file path → PlayHistoryItem for quick lookup.
    /// Safe against duplicate file names (e.g. corrupted playhistory.dat) — first entry wins.
    /// </summary>
    public Dictionary<string, PlayHistoryItem> GetHistoryLookup()
    {
        lock (_historyLock)
        {
            try
            {
                return PlayHistoryList
                    .GroupBy(h => h.FileName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error building play history lookup; returning empty dictionary.");
                return new Dictionary<string, PlayHistoryItem>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
