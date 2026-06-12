using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

public interface IThemeMenuHost
{
    MenuItem FindMenuItemByName(string name);
}
