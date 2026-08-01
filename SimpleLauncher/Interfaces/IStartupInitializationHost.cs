using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Services.TrayIcon;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides access to host UI elements and state used during startup initialization.
/// </summary>
public interface IStartupInitializationHost
{
    /// <summary>
    /// Gets or sets the status bar timer used to clear status messages.
    /// </summary>
    DispatcherTimer? StatusBarTimer { get; set; }

    /// <summary>
    /// Gets the status bar text label.
    /// </summary>
    Label StatusBarText { get; }

    /// <summary>
    /// Gets or sets the name of the currently selected system.
    /// </summary>
    string? SelectedSystem { get; set; }

    /// <summary>
    /// Gets or sets the current play time string displayed by the status bar.
    /// </summary>
    string PlayTime { get; set; }

    /// <summary>
    /// Gets the RetroAchievements overlay button menu item.
    /// </summary>
    MenuItem RetroAchievementButton { get; }

    /// <summary>
    /// Gets the video link overlay button menu item.
    /// </summary>
    MenuItem VideoLinkButton { get; }

    /// <summary>
    /// Gets the info link overlay button menu item.
    /// </summary>
    MenuItem InfoLinkButton { get; }

    /// <summary>
    /// Gets the host window used to create UI elements such as the tray icon.
    /// </summary>
    Window HostWindow { get; }

    /// <summary>
    /// Sets the current view mode (e.g., grid or list).
    /// </summary>
    /// <param name="viewMode">The view mode identifier.</param>
    void SetViewMode(string viewMode);

    /// <summary>
    /// Resets the pagination buttons to their default state.
    /// </summary>
    void SetPaginationButtonsDefault();

    /// <summary>
    /// Sets the tray icon manager for the host window.
    /// </summary>
    /// <param name="manager">The tray icon manager to set.</param>
    void SetTrayIconManager(TrayIconManager manager);
}
