using System.Windows;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameFileWatcher;
using SimpleLauncher.Core.Services.UsageStats;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.StartupInitialization;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Services;

/// <summary>
///     Manages the application lifecycle including startup initialization, update checks, usage reporting, and game file
///     watching.
/// </summary>
public class ApplicationLifecycleService : IApplicationLifecycleService
{
    private readonly GameFileWatcherService _gameFileWatcherService;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly StartupInitializationService _startupInitializationService;
    private readonly Stats _stats;
    private readonly CheckForUpdatesService _updateChecker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationLifecycleService" /> class.
    /// </summary>
    /// <param name="updateChecker">The service for checking application updates.</param>
    /// <param name="stats">The usage statistics reporting service.</param>
    /// <param name="startupInitializationService">The service for performing startup initialization tasks.</param>
    /// <param name="playHistoryManager">The manager for play history entries.</param>
    /// <param name="gameFileWatcherService">The service for monitoring game file changes.</param>
    public ApplicationLifecycleService(
        CheckForUpdatesService updateChecker,
        Stats stats,
        StartupInitializationService startupInitializationService,
        PlayHistoryManager playHistoryManager,
        GameFileWatcherService gameFileWatcherService)
    {
        _updateChecker = updateChecker;
        _stats = stats;
        _startupInitializationService = startupInitializationService;
        _playHistoryManager = playHistoryManager;
        _gameFileWatcherService = gameFileWatcherService;
    }

    /// <summary>
    ///     Initializes the application startup by delegating to the startup initialization service.
    /// </summary>
    /// <param name="host">The startup initialization host that coordinates the initialization process.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public Task InitializeStartupAsync(IStartupInitializationHost host)
    {
        return _startupInitializationService.InitializeAsync(host);
    }

    /// <summary>
    ///     Silently checks for application updates without notifying the user.
    /// </summary>
    /// <param name="mainWindow">The main application window used for the check.</param>
    /// <returns>A task representing the asynchronous update check operation.</returns>
    public Task SilentCheckForUpdatesAsync(Window mainWindow)
    {
        return _updateChecker.SilentCheckForUpdatesAsync(mainWindow);
    }

    /// <summary>
    ///     Reports anonymous usage statistics to the statistics service.
    /// </summary>
    /// <returns>A task representing the asynchronous usage reporting operation.</returns>
    public Task ReportUsageAsync()
    {
        return _stats.CallApiAsync();
    }

    /// <summary>
    ///     Migrates play history entries from file names to full paths for the given systems.
    /// </summary>
    /// <param name="systemManagers">The list of system managers to migrate history for.</param>
    public void MigratePlayHistory(IList<SystemManagerService> systemManagers)
    {
        _playHistoryManager.MigrateFilenamesToFullPaths(systemManagers.ToList());
    }

    /// <summary>
    ///     Occurs when the set of game files being watched changes.
    /// </summary>
    public event EventHandler<EventArgs<string>> GameFilesChanged
    {
        add => _gameFileWatcherService.GameFilesChanged += value;
        remove => _gameFileWatcherService.GameFilesChanged -= value;
    }

    /// <summary>
    ///     Starts watching the given folders for game file changes for the specified system.
    /// </summary>
    /// <param name="folders">The folders to watch for game file changes.</param>
    /// <param name="systemName">The name of the system the folders belong to.</param>
    /// <param name="fileExtensions">The optional file extensions to filter the watched files by.</param>
    public void StartWatching(IEnumerable<string> folders, string systemName,
        IEnumerable<string>? fileExtensions = null)
    {
        _gameFileWatcherService.StartWatching(folders, systemName, fileExtensions);
    }

    /// <summary>
    ///     Stops watching for game file changes.
    /// </summary>
    public void StopWatching()
    {
        _gameFileWatcherService.StopWatching();
    }

    /// <summary>
    ///     Unsubscribes the given handler from the game files changed event.
    /// </summary>
    /// <param name="handler">The event handler to unsubscribe.</param>
    public void UnsubscribeGameFilesChanged(EventHandler<EventArgs<string>> handler)
    {
        _gameFileWatcherService.GameFilesChanged -= handler;
    }
}