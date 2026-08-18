using Avalonia.Controls;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
/// Manages the checked state of option menu items (thumbnail size, games per page,
/// aspect ratio). The Avalonia menu items carry their value in <c>Tag</c> (sizes /
/// page counts) or <c>Name</c> (aspect ratios), so the service can drive any menu
/// generically — the same contract the WPF <c>MenuCheckMarkService</c> provides
/// through its named host properties.
/// </summary>
public class AvaloniaMenuCheckMarkService
{
    /// <summary>
    /// Checks exactly the menu item whose <c>Tag</c> parses to <paramref name="selectedValue"/>
    /// (used by the Button Size and Games Per Page menus).
    /// </summary>
    /// <param name="menuItems">The menu items of the submenu.</param>
    /// <param name="selectedValue">The selected numeric value.</param>
    public void UpdateCheckedByTag(IEnumerable<MenuItem> menuItems, int selectedValue)
    {
        foreach (var item in menuItems)
        {
            item.IsChecked = item.Tag is string tag && int.TryParse(tag, out var tagValue) && tagValue == selectedValue;
        }
    }

    /// <summary>
    /// Checks exactly the menu item whose <c>Name</c> equals <paramref name="selectedName"/>
    /// (used by the Button Aspect Ratio menu).
    /// </summary>
    /// <param name="menuItems">The menu items of the submenu.</param>
    /// <param name="selectedName">The selected item name.</param>
    public void UpdateCheckedByName(IEnumerable<MenuItem> menuItems, string? selectedName)
    {
        foreach (var item in menuItems)
        {
            item.IsChecked = string.Equals(item.Name, selectedName, StringComparison.Ordinal);
        }
    }
}