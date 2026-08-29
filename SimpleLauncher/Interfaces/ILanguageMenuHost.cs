using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides UI elements required by the language menu service to manage the language selection menu.
/// </summary>
public interface ILanguageMenuHost
{
    /// <summary>
    ///     Gets the service used to update the status bar.
    /// </summary>
    IUpdateStatusBar UpdateStatusBarService { get; }

    /// <summary>
    ///     Finds a menu item by its name.
    /// </summary>
    /// <param name="name">The name of the menu item to find.</param>
    /// <returns>The matching menu item, or null if not found.</returns>
    MenuItem? FindMenuItemByName(string name);
}