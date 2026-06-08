using System.Windows;
using SimpleLauncher.Services.CheckForUpdates;
using SimpleLauncher.Core.Services.GameFileWatcher;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.StartupInitialization;
using SimpleLauncher.Services.UsageStats;

namespace SimpleLauncher.Services.ApplicationLifecycle;

public class ApplicationLifecycleService : IApplicationLifecycleService
{
    private readonly UpdateChecker _updateChecker;
    private readonly Stats _stats;
    private readonly StartupInitializationService _startupInitializationService;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly GameFileWatcherService _gameFileWatcherService;

    public ApplicationLifecycleService(
        UpdateChecker updateChecker,
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

    public Task InitializeStartupAsync(IStartupInitializationHost host)
    {
        return _startupInitializationService.InitializeAsync(host);
    }

    public Task SilentCheckForUpdatesAsync(Window mainWindow)
    {
        return _updateChecker.SilentCheckForUpdatesAsync(mainWindow);
    }

    public Task ReportUsageAsync()
    {
        return _stats.CallApiAsync();
    }

    public void MigratePlayHistory(List<SystemManager.SystemManager> systemManagers)
    {
        _playHistoryManager.MigrateFilenamesToFullPaths(systemManagers);
    }

    public event Action<string> GameFilesChanged
    {
        add => _gameFileWatcherService.GameFilesChanged += value;
        remove => _gameFileWatcherService.GameFilesChanged -= value;
    }

    public void StartWatching(IEnumerable<string> folders, string systemName, IEnumerable<string> fileExtensions = null)
    {
        _gameFileWatcherService.StartWatching(folders, systemName, fileExtensions);
    }

    public void StopWatching()
    {
        _gameFileWatcherService.StopWatching();
    }

    public void UnsubscribeGameFilesChanged(Action<string> handler)
    {
        _gameFileWatcherService.GameFilesChanged -= handler;
    }
}
