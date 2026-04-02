using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        var baseTheme = menuItem.Name;
        App.ChangeTheme(baseTheme, _settings.AccentColor);

        _playSoundEffects.PlayNotificationSound();

        UncheckBaseThemes();
        menuItem.IsChecked = true;
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        var accentColor = menuItem.Name;
        App.ChangeTheme(_settings.BaseTheme, accentColor);

        _playSoundEffects.PlayNotificationSound();

        UncheckAccentColors();
        menuItem.IsChecked = true;
    }

    private void UncheckBaseThemes()
    {
        Light.IsChecked = false;
        Dark.IsChecked = false;
        Adaptive.IsChecked = false;
        HighContrast.IsChecked = false;
        Midnight.IsChecked = false;
    }

    private void UncheckAccentColors()
    {
        Red.IsChecked = false;
        Green.IsChecked = false;
        Blue.IsChecked = false;
        Purple.IsChecked = false;
        Orange.IsChecked = false;
        Lime.IsChecked = false;
        Emerald.IsChecked = false;
        Teal.IsChecked = false;
        Cyan.IsChecked = false;
        Cobalt.IsChecked = false;
        Indigo.IsChecked = false;
        Violet.IsChecked = false;
        Pink.IsChecked = false;
        Magenta.IsChecked = false;
        Crimson.IsChecked = false;
        Amber.IsChecked = false;
        Yellow.IsChecked = false;
        Brown.IsChecked = false;
        Olive.IsChecked = false;
        Steel.IsChecked = false;
        Mauve.IsChecked = false;
        Taupe.IsChecked = false;
        Sienna.IsChecked = false;
        Maroon.IsChecked = false;
        OliveDrab.IsChecked = false;
        Plum.IsChecked = false;
        SkyBlue.IsChecked = false;
    }

    private void SetCheckedTheme(string baseTheme, string accentColor)
    {
        switch (baseTheme)
        {
            case "Light":
                Light.IsChecked = true;
                break;
            case "Dark":
                Dark.IsChecked = true;
                break;
            case "Adaptive":
                Adaptive.IsChecked = true;
                break;
            case "HighContrast":
                HighContrast.IsChecked = true;
                break;
            case "Midnight":
                Midnight.IsChecked = true;
                break;
        }

        switch (accentColor)
        {
            case "Red":
                Red.IsChecked = true;
                break;
            case "Green":
                Green.IsChecked = true;
                break;
            case "Blue":
                Blue.IsChecked = true;
                break;
            case "Purple":
                Purple.IsChecked = true;
                break;
            case "Orange":
                Orange.IsChecked = true;
                break;
            case "Lime":
                Lime.IsChecked = true;
                break;
            case "Emerald":
                Emerald.IsChecked = true;
                break;
            case "Teal":
                Teal.IsChecked = true;
                break;
            case "Cyan":
                Cyan.IsChecked = true;
                break;
            case "Cobalt":
                Cobalt.IsChecked = true;
                break;
            case "Indigo":
                Indigo.IsChecked = true;
                break;
            case "Violet":
                Violet.IsChecked = true;
                break;
            case "Pink":
                Pink.IsChecked = true;
                break;
            case "Magenta":
                Magenta.IsChecked = true;
                break;
            case "Crimson":
                Crimson.IsChecked = true;
                break;
            case "Amber":
                Amber.IsChecked = true;
                break;
            case "Yellow":
                Yellow.IsChecked = true;
                break;
            case "Brown":
                Brown.IsChecked = true;
                break;
            case "Olive":
                Olive.IsChecked = true;
                break;
            case "Steel":
                Steel.IsChecked = true;
                break;
            case "Mauve":
                Mauve.IsChecked = true;
                break;
            case "Taupe":
                Taupe.IsChecked = true;
                break;
            case "Sienna":
                Sienna.IsChecked = true;
                break;
            case "Maroon":
                Maroon.IsChecked = true;
                break;
            case "OliveDrab":
                OliveDrab.IsChecked = true;
                break;
            case "Plum":
                Plum.IsChecked = true;
                break;
            case "SkyBlue":
                SkyBlue.IsChecked = true;
                break;
        }
    }
}