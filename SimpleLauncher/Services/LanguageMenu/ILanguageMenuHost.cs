using System.Windows.Controls;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher.Services.LanguageMenu;

public interface ILanguageMenuHost
{
    MenuItem FindMenuItemByName(string name);
    IUpdateStatusBar UpdateStatusBarService { get; }
}
