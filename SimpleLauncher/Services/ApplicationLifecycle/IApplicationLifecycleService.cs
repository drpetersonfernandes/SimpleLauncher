using System.Windows;
using SimpleLauncher.Services.StartupInitialization;

namespace SimpleLauncher.Services.ApplicationLifecycle;

public interface IApplicationLifecycleService
{
    void InitializeStartup(IStartupInitializationHost host);
    Task SilentCheckForUpdatesAsync(Window mainWindow);
    Task ReportUsageAsync();
    void MigratePlayHistory(List<SystemManager.SystemManager> systemManagers);

    event Action<string> GameFilesChanged;
    void StartWatching(IEnumerable<string> folders, string systemName, IEnumerable<string> fileExtensions = null);
    void StopWatching();
    void UnsubscribeGameFilesChanged(Action<string> handler);
}
