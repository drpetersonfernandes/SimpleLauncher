using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides access to theme menu items for managing base theme and accent color selections.
/// </summary>
public interface IThemeMenuHost
{
    /// <summary>
    /// Finds a menu item by its name.
    /// </summary>
    /// <param name="name">The name of the menu item to find.</param>
    /// <returns>The matching menu item, or null if not found.</returns>
    MenuItem? FindMenuItemByName(string name);
}
