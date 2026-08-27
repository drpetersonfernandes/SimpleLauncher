using System.Windows.Controls;
using System.Windows.Threading;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides access to the host window's status bar elements and dispatcher.
/// </summary>
public interface IStatusBarHost
{
    /// <summary>
    /// Gets the dispatcher for the host window, used to marshal calls to the UI thread.
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the status bar text label.
    /// </summary>
    Label StatusBarText { get; }

    /// <summary>
    /// Gets the status bar timer used to clear status messages.
    /// </summary>
    DispatcherTimer? StatusBarTimer { get; }
}