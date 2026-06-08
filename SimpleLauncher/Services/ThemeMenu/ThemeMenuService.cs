using System.Windows.Controls;
using SimpleLauncher.Services.PlaySound;
using Settings = SimpleLauncher.Core.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.ThemeMenu;

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

    public ThemeMenuService(PlaySoundEffects playSoundEffects, Settings settings)
    {
        _playSoundEffects = playSoundEffects;
        _settings = settings;
    }

    public void Initialize(IThemeMenuHost host)
    {
        _host = host;
    }

    public void ChangeBaseTheme(MenuItem menuItem)
    {
        var baseTheme = menuItem.Name;
        App.ChangeTheme(baseTheme, _settings.AccentColor);
        _playSoundEffects.PlayNotificationSound();
        UncheckAllBaseThemes();
        menuItem.IsChecked = true;
    }

    public void ChangeAccentColor(MenuItem menuItem)
    {
        var accentColor = menuItem.Name;
        App.ChangeTheme(_settings.BaseTheme, accentColor);
        _playSoundEffects.PlayNotificationSound();
        UncheckAllAccentColors();
        menuItem.IsChecked = true;
    }

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
