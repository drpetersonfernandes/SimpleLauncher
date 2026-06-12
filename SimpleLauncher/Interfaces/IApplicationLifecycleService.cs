using System.Windows;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IApplicationLifecycleService
{
    Task InitializeStartupAsync(IStartupInitializationHost host);
    Task SilentCheckForUpdatesAsync(Window mainWindow);
    Task ReportUsageAsync();
    void MigratePlayHistory(List<SystemManager> systemManagers);

    event Action<string> GameFilesChanged;
    void StartWatching(IEnumerable<string> folders, string systemName, IEnumerable<string> fileExtensions = null);
    void StopWatching();
    void UnsubscribeGameFilesChanged(Action<string> handler);
}
