using System.Collections.ObjectModel;
using System.Globalization;
using MessagePack;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services;
using SimpleLauncher.Core.Services.CheckPaths;
using ILogger = Serilog.ILogger;

namespace SimpleLauncher.Avalonia.Services.PlayHistory;

/// <summary>
///     Manages play history tracking, persistence, and date format migration using MessagePack serialization.
///     Compatible with the existing playhistory.dat format from SimpleLauncher.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public class PlayHistoryManager
{
    private const string IsoDateFormat = "yyyy-MM-dd";
    private const string IsoTimeFormat = "HH:mm:ss";
    [IgnoreMember] private static readonly DataFileLocation FileLocation = new("playhistory.dat");
    [IgnoreMember] private readonly Lock _historyLock = new();
    [IgnoreMember] private ILogger? _logger;

    [Key(0)] public ObservableCollection<PlayHistoryItem> PlayHistoryList { get; set; } = [];

    [Key(1)] public int Version { get; set; } = 1;

    private static string FilePath => FileLocation.FilePath;
    private static string TempFilePath => FileLocation.TempFilePath;
    public static bool IsPortableMode => FileLocation.IsPortableMode;

    /// <summary>
    ///     Loads play history from the MessagePack file. Creates new if doesn't exist.
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
    ///     Synchronous initial save (startup/recovery paths only). Mirrors
    ///     <see cref="SavePlayHistoryAsync" /> with retry logic and AppData fallback,
    ///     but never awaits — safe to call on the UI thread.
    /// </summary>
    private void SavePlayHistorySync()
    {
        const int maxRetries = 3;
        var retryDelayMs = 100;
        var attempt = 0;

        while (attempt < maxRetries)
            try
            {
                var bytes = MessagePackSerializer.Serialize(this);
                File.WriteAllBytes(TempFilePath, bytes);
                AtomicReplace(TempFilePath, FilePath);
                return;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                _logger?.Warning(ex, "Error saving playhistory.dat (attempt {Attempt})", attempt + 1);
                attempt++;

                // If in portable mode and retries exhausted, fall back to LocalAppData
                if (IsPortableMode && attempt >= maxRetries)
                    try
                    {
                        if (FileLocation.TryFallbackToLocalAppData())
                        {
                            attempt = 0;
                            continue;
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger?.Debug("FallbackToLocalAppData failed: {Message}", fallbackEx.Message);
                    }

                if (attempt < maxRetries)
                {
                    try
                    {
                        if (File.Exists(TempFilePath)) File.Delete(TempFilePath);
                    }
                    catch
                    {
                        // Ignore cleanup failures
                    }

                    Thread.Sleep(retryDelayMs);
                    retryDelayMs *= 2;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error saving playhistory.dat (attempt {Attempt})", attempt + 1);
                attempt++;
                if (attempt < maxRetries) Thread.Sleep(retryDelayMs);
            }
    }

    /// <summary>
    ///     Replaces the destination file atomically. Falls back to delete-then-move
    ///     when the OS refuses an in-place overwrite (e.g. file locked by another handle).
    /// </summary>
    private static void AtomicReplace(string tempPath, string destPath)
    {
        try
        {
            File.Move(tempPath, destPath, true);
        }
        catch (IOException)
        {
            // Destination may be locked — delete first, then move.
            if (File.Exists(destPath)) File.Delete(destPath);
            File.Move(tempPath, destPath, false);
        }
    }

    /// <summary>
    ///     Saves play history atomically with retry logic and AppData fallback.
    /// </summary>
    public async Task SavePlayHistoryAsync()
    {
        const int maxRetries = 3;
        var retryDelayMs = 100;
        var attempt = 0;

        while (attempt < maxRetries)
            try
            {
                var bytes = MessagePackSerializer.Serialize(this);
                await File.WriteAllBytesAsync(TempFilePath, bytes);
                AtomicReplace(TempFilePath, FilePath);
                return;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                _logger?.Warning(ex, "Error saving playhistory.dat (attempt {Attempt})", attempt + 1);
                attempt++;

                // If in portable mode and retries exhausted, fall back to LocalAppData
                if (IsPortableMode && attempt >= maxRetries)
                    try
                    {
                        if (FileLocation.TryFallbackToLocalAppData())
                        {
                            attempt = 0;
                            continue;
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger?.Debug("FallbackToLocalAppData failed: {Message}", fallbackEx.Message);
                    }

                if (attempt < maxRetries)
                {
                    try
                    {
                        if (File.Exists(TempFilePath)) File.Delete(TempFilePath);
                    }
                    catch
                    {
                        // Ignore cleanup failures
                    }

                    await Task.Delay(retryDelayMs);
                    retryDelayMs *= 2;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error saving playhistory.dat (attempt {Attempt})", attempt + 1);
                attempt++;
                if (attempt < maxRetries) await Task.Delay(retryDelayMs);
            }
    }

    /// <summary>
    ///     Renames the system in all play history entries (used when a system is renamed in Edit System).
    /// </summary>
    /// <param name="oldSystemName">The previous system name.</param>
    /// <param name="newSystemName">The new system name.</param>
    /// <returns>A task representing the save operation (no-op when nothing changed).</returns>
    public async Task RenameSystemAsync(string oldSystemName, string newSystemName)
    {
        var changed = false;
        lock (_historyLock)
        {
            foreach (var item in PlayHistoryList)
                if (item.SystemName.Equals(oldSystemName, StringComparison.OrdinalIgnoreCase))
                {
                    item.SystemName = newSystemName;
                    changed = true;
                }
        }

        if (changed) await SavePlayHistoryAsync();
    }

    /// <summary>
    ///     Records a play event for the given game. Increments play count and updates timestamps.
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
    ///     Migrates old records that only contain filenames to full absolute paths
    ///     (legacy WPF history entries written before full-path recording). Runs once
    ///     at startup with the current system configuration.
    /// </summary>
    /// <param name="systemManagers">The configured systems used to resolve missing files.</param>
    /// <returns>A task representing the save operation (no-op when nothing changed).</returns>
    public async Task MigrateFilenamesToFullPathsAsync(List<SystemManagerConfig> systemManagers)
    {
        var needsSave = false;
        lock (_historyLock)
        {
            foreach (var item in PlayHistoryList)
            {
                // If the path is not rooted, it's an old "filename only" record
                if (Path.IsPathRooted(item.FileName)) continue;

                var system = systemManagers.FirstOrDefault(s =>
                    s.SystemName.Equals(item.SystemName, StringComparison.OrdinalIgnoreCase));
                if (system is null) continue;

                var resolvedPath = PathHelper.FindFileInSystemFolders(system.SystemFolders, item.FileName);
                if (!string.IsNullOrEmpty(resolvedPath))
                {
                    item.FileName = resolvedPath;
                    needsSave = true;
                }
            }
        }

        if (needsSave) await SavePlayHistoryAsync();
    }

    /// <summary>
    ///     Gets a dictionary of file path → PlayHistoryItem for quick lookup.
    ///     Safe against duplicate file names (e.g. corrupted playhistory.dat) — first entry wins.
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