using System.Windows.Controls;

namespace SimpleLauncher.Services.ThemeMenu;

public interface IThemeMenuHost
{
    MenuItem FindMenuItemByName(string name);
}
