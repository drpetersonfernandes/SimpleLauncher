using System.Windows;
using SimpleLauncher.Models;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IApplicationLifecycleService
{
    Task InitializeStartupAsync(IStartupInitializationHost host);
    Task SilentCheckForUpdatesAsync(Window mainWindow);
    Task ReportUsageAsync();
    void MigratePlayHistory(IList<SystemManagerService> systemManagers);

    event EventHandler<EventArgs<string>> GameFilesChanged;
    void StartWatching(IEnumerable<string> folders, string systemName, IEnumerable<string>? fileExtensions = null);
    void StopWatching();
    void UnsubscribeGameFilesChanged(EventHandler<EventArgs<string>> handler);
}
