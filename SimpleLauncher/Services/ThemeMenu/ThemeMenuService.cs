using System.Windows.Controls;
using SimpleLauncher.Services.PlaySound;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.ThemeMenu;

using Interfaces;

/// <summary>
/// Manages theme and accent color selection in the UI, updating menu check marks and applying theme changes.
/// </summary>
public class ThemeMenuService
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Settings _settings;
    private IThemeMenuHost _host;

    private static readonly string[] BaseThemeNames = ["Light", "Dark", "Adaptive", "HighContrast", "Midnight"];

    private static readonly string[] AccentColorNames =
    [
        "Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald",
        "Green", "Indigo", "Lime", "Magenta", "Maroon", "Mauve", "Olive",
        "OliveDrab", "Orange", "Pink", "Plum", "Purple", "Red", "Sienna",
        "SkyBlue", "Steel", "Taupe", "Teal", "Violet", "Yellow"
    ];

    /// <summary>Initializes a new instance of the ThemeMenuService with the specified dependencies.</summary>
    public ThemeMenuService(PlaySoundEffects playSoundEffects, Settings settings)
    {
        _playSoundEffects = playSoundEffects;
        _settings = settings;
    }

    /// <summary>Initializes the service with the specified UI host.</summary>
    public void Initialize(IThemeMenuHost host)
    {
        _host = host;
    }

    /// <summary>Changes the base theme and updates the corresponding menu check marks.</summary>
    public void ChangeBaseTheme(MenuItem menuItem)
    {
        var baseTheme = menuItem.Name;
        App.ChangeTheme(baseTheme, _settings.AccentColor);
        _playSoundEffects.PlayNotificationSound();
        UncheckAllBaseThemes();
        menuItem.IsChecked = true;
    }

    /// <summary>Changes the accent color and updates the corresponding menu check marks.</summary>
    public void ChangeAccentColor(MenuItem menuItem)
    {
        var accentColor = menuItem.Name;
        App.ChangeTheme(_settings.BaseTheme, accentColor);
        _playSoundEffects.PlayNotificationSound();
        UncheckAllAccentColors();
        menuItem.IsChecked = true;
    }

    /// <summary>Sets the checked state for the specified base theme and accent color menu items.</summary>
    public void SetCheckedTheme(string baseTheme, string accentColor)
    {
        UncheckAllBaseThemes();
        UncheckAllAccentColors();

        if (_host.FindMenuItemByName(baseTheme) is { } baseItem)
        {
            baseItem.IsChecked = true;
        }

        if (_host.FindMenuItemByName(accentColor) is { } accentItem)
        {
            accentItem.IsChecked = true;
        }
    }

    private void UncheckAllBaseThemes()
    {
        foreach (var name in BaseThemeNames)
        {
            if (_host.FindMenuItemByName(name) is { } item)
            {
                item.IsChecked = false;
            }
        }
    }

    private void UncheckAllAccentColors()
    {
        foreach (var name in AccentColorNames)
        {
            if (_host.FindMenuItemByName(name) is { } item)
            {
                item.IsChecked = false;
            }
        }
    }
}
