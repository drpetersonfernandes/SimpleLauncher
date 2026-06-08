using System.IO;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Core.Services.GameFileWatcher;

/// <summary>
/// Monitors ROM system folders for file changes (create, delete, rename, change)
/// and raises an event when changes are detected. Uses debouncing to avoid
/// rapid re-scans during batch file operations.
/// </summary>
public sealed class GameFileWatcherService : IDisposable
{
    private readonly Dictionary<FileSystemWatcher, WatcherTag> _watchers = new();
    private readonly object _lock = new();
    private readonly IDebugLogger _debugLogger;
    private CancellationTokenSource _debounceCts;
    private bool _disposed;

    public GameFileWatcherService(IDebugLogger debugLogger)
    {
        _debugLogger = debugLogger;
    }

    /// <summary>
    /// Raised when a file change is detected in any monitored folder.
    /// The string parameter is the system name that was being monitored.
    /// </summary>
    public event Action<string> GameFilesChanged;

    /// <summary>
    /// The debounce delay before raising the GameFilesChanged event.
    /// Prevents rapid re-scans during batch file operations (e.g., extracting archives).
    /// </summary>
    public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Starts monitoring the specified folders for file changes.
    /// Stops monitoring any previously monitored folders first.
    /// </summary>
    /// <param name="folders">The folder paths to monitor (can be relative or contain %BASEFOLDER%).</param>
    /// <param name="systemName">The system name associated with these folders.</param>
    /// <param name="fileExtensions">Optional list of file extensions to filter (e.g., ["zip", "tap"]). If null, all files are monitored.</param>
    public void StartWatching(IEnumerable<string> folders, string systemName, IEnumerable<string> fileExtensions = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        StopWatching();

        var extensionFilter = fileExtensions?.Select(static e => e.TrimStart('.').ToLowerInvariant()).ToHashSet();
        var resolvedFolders = folders
            .Select(static f => PathHelper.TryGetExistingDirectory(f))
            .Where(static f => f != null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (resolvedFolders.Count == 0)
        {
            _debugLogger.Log($"[GameFileWatcherService] No valid folders to watch for system '{systemName}'.");
            return;
        }

        var tag = new WatcherTag(systemName, extensionFilter);

        lock (_lock)
        {
            foreach (var folder in resolvedFolders)
            {
                try
                {
                    if (folder != null)
                    {
                        var watcher = new FileSystemWatcher(folder)
                        {
                            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                            IncludeSubdirectories = true,
                            EnableRaisingEvents = false
                        };

                        watcher.Created += OnFileChanged;
                        watcher.Deleted += OnFileChanged;
                        watcher.Renamed += OnFileChanged;
                        watcher.Error += OnWatcherError;

                        _watchers[watcher] = tag;
                        watcher.EnableRaisingEvents = true;
                    }

                    _debugLogger.Log($"[GameFileWatcherService] Watching '{folder}' for system '{systemName}'.");
                }
                catch (Exception ex)
                {
                    _debugLogger.Log($"[GameFileWatcherService] Failed to watch '{folder}': {ex.Message}");
                }
            }
        }

        lock (_lock)
        {
            _debugLogger.Log($"[GameFileWatcherService] Started watching {_watchers.Count} folder(s) for system '{systemName}'.");
        }
    }

    /// <summary>
    /// Stops monitoring all currently monitored folders.
    /// </summary>
    public void StopWatching()
    {
        lock (_lock)
        {
            foreach (var (watcher, _) in _watchers)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Created -= OnFileChanged;
                    watcher.Deleted -= OnFileChanged;
                    watcher.Renamed -= OnFileChanged;
                    watcher.Error -= OnWatcherError;
                    watcher.Dispose();
                }
                catch (Exception ex)
                {
                    _debugLogger.Log($"[GameFileWatcherService] Error disposing watcher: {ex.Message}");
                }
            }

            _watchers.Clear();
        }

        CancelPendingDebounce();
        _debugLogger.Log("[GameFileWatcherService] Stopped all watchers.");
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (_disposed) return;

        if (sender is not FileSystemWatcher watcher) return;

        // Look up the tag for this watcher
        WatcherTag tag;
        lock (_lock)
        {
            if (!_watchers.TryGetValue(watcher, out tag)) return;
        }

        // Check extension filter if applicable
        if (tag.Extensions is { Count: > 0 })
        {
            var ext = Path.GetExtension(e.Name)?.TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrEmpty(ext) && !tag.Extensions.Contains(ext))
            {
                return; // Ignore files with non-matching extensions
            }
        }

        _debugLogger.Log($"[GameFileWatcherService] File change detected: {e.ChangeType} - {e.FullPath} (System: {tag.SystemName})");

        DebounceAndRaiseEvent(tag.SystemName);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _debugLogger.Log($"[GameFileWatcherService] Watcher error: {e.GetException().Message}");
    }

    private void DebounceAndRaiseEvent(string systemName)
    {
        lock (_lock)
        {
            CancelPendingDebounce();

            var cts = new CancellationTokenSource();
            _debounceCts = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(DebounceDelay, cts.Token);

                    if (!cts.IsCancellationRequested)
                    {
                        _debugLogger.Log($"[GameFileWatcherService] Debounce complete. Raising GameFilesChanged for system '{systemName}'.");
                        GameFilesChanged?.Invoke(systemName);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected when debounce is reset by another file change
                }
            }, cts.Token);
        }
    }

    private void CancelPendingDebounce()
    {
        var oldCts = Interlocked.Exchange(ref _debounceCts, null);
        // ReSharper disable once ConstantConditionalAccessQualifier
        oldCts?.Cancel();
        oldCts?.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        StopWatching();
    }

    /// <summary>
    /// Stores the system name and optional extension filter for a FileSystemWatcher.
    /// </summary>
    private sealed record WatcherTag(string SystemName, HashSet<string> Extensions);
}