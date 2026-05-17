using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _themeMenuService.ChangeBaseTheme(menuItem);
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _themeMenuService.ChangeAccentColor(menuItem);
    }

    private void SetCheckedTheme(string baseTheme, string accentColor)
    {
        _themeMenuService.SetCheckedTheme(baseTheme, accentColor);
    }
}