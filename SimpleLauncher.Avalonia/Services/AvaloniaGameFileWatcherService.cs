using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameFileWatcher;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
/// Avalonia-friendly wrapper around the Core <see cref="GameFileWatcherService"/>.
/// Starts watching every configured system's ROM folders and re-raises the
/// <see cref="GameFilesChanged"/> event (with the affected system name) so the
/// main window can refresh the library live — no WPF dependencies.
/// </summary>
public class AvaloniaGameFileWatcherService : IDisposable
{
    private readonly GameFileWatcherService _watcher;
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaGameFileWatcherService"/> class.
    /// </summary>
    /// <param name="watcher">The Core watcher service.</param>
    /// <param name="logger">The Serilog logger.</param>
    public AvaloniaGameFileWatcherService(GameFileWatcherService watcher, ILogger logger)
    {
        _watcher = watcher;
        _logger = logger;
        _watcher.GameFilesChanged += OnGameFilesChanged;
    }

    /// <summary>
    /// Raised (on a thread-pool thread) when a file change is detected in any watched
    /// folder. The string parameter is the affected system name.
    /// </summary>
    public event EventHandler<EventArgs<string>>? GameFilesChanged;

    /// <summary>
    /// The debounce delay before <see cref="GameFilesChanged"/> is raised.
    /// </summary>
    public TimeSpan DebounceDelay
    {
        get => _watcher.DebounceDelay;
        set => _watcher.DebounceDelay = value;
    }

    /// <summary>
    /// Starts watching the ROM folders of every configured system. Stops any
    /// previously monitored folders first.
    /// </summary>
    /// <param name="systems">The system configurations to watch.</param>
    public void StartWatchingForSystems(IEnumerable<SystemManagerConfig> systems)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var systemManagerConfigs = systems.ToList();
        foreach (var system in systemManagerConfigs)
        {
            _watcher.StartWatching(system.SystemFolders, system.SystemName, system.FileFormatsToSearch);
        }

        _logger.Debug("[AvaloniaGameFileWatcherService] Started watching {Count} system(s).", systemManagerConfigs.Count());
    }

    /// <summary>
    /// Stops watching all currently monitored folders.
    /// </summary>
    public void StopWatching()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _watcher.StopWatching();
    }

    private void OnGameFilesChanged(object? sender, EventArgs<string> e)
    {
        GameFilesChanged?.Invoke(this, e);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _watcher.GameFilesChanged -= OnGameFilesChanged;
        _watcher.Dispose();
    }
}