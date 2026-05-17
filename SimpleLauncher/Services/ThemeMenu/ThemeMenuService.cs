using System.Windows.Controls;
using SimpleLauncher.Services.PlaySound;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.ThemeMenu;

public class ThemeMenuService
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Settings _settings;
    private MainWindow _mainWindow;

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

    public void Initialize(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
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

        if (_mainWindow.FindName(baseTheme) is MenuItem baseItem)
        {
            baseItem.IsChecked = true;
        }

        if (_mainWindow.FindName(accentColor) is MenuItem accentItem)
        {
            accentItem.IsChecked = true;
        }
    }

    private void UncheckAllBaseThemes()
    {
        foreach (var name in BaseThemeNames)
        {
            if (_mainWindow.FindName(name) is MenuItem item)
            {
                item.IsChecked = false;
            }
        }
    }

    private void UncheckAllAccentColors()
    {
        foreach (var name in AccentColorNames)
        {
            if (_mainWindow.FindName(name) is MenuItem item)
            {
                item.IsChecked = false;
            }
        }
    }
}