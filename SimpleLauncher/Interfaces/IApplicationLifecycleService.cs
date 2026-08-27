using System.Windows;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Defines the contract for application lifecycle operations including startup initialization,
/// update checking, usage reporting, play history migration, and file system watching.
/// </summary>
public interface IApplicationLifecycleService
{
    /// <summary>
    /// Initializes the application startup sequence.
    /// </summary>
    /// <param name="host">The startup initialization host.</param>
    Task InitializeStartupAsync(IStartupInitializationHost host);

    /// <summary>
    /// Silently checks for available application updates.
    /// </summary>
    /// <param name="mainWindow">The main application window.</param>
    Task SilentCheckForUpdatesAsync(Window mainWindow);

    /// <summary>
    /// Reports anonymous usage telemetry.
    /// </summary>
    Task ReportUsageAsync();

    /// <summary>
    /// Migrates play history data for the specified system managers.
    /// </summary>
    /// <param name="systemManagers">The list of system managers to migrate history for.</param>
    void MigratePlayHistory(IList<SystemManagerService> systemManagers);

    /// <summary>
    /// Occurs when game files change in the watched folders.
    /// </summary>
    event EventHandler<EventArgs<string>> GameFilesChanged;

    /// <summary>
    /// Starts watching the specified folders for game file changes.
    /// </summary>
    /// <param name="folders">The folders to watch.</param>
    /// <param name="systemName">The system name associated with the watched folders.</param>
    /// <param name="fileExtensions">Optional file extension filter.</param>
    void StartWatching(IEnumerable<string> folders, string systemName, IEnumerable<string>? fileExtensions = null);

    /// <summary>
    /// Stops watching all previously watched folders.
    /// </summary>
    void StopWatching();

    /// <summary>
    /// Unsubscribes a handler from the GameFilesChanged event.
    /// </summary>
    /// <param name="handler">The event handler to unsubscribe.</param>
    void UnsubscribeGameFilesChanged(EventHandler<EventArgs<string>> handler);
}