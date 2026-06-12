using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

public interface ILanguageMenuHost
{
    MenuItem FindMenuItemByName(string name);
    IUpdateStatusBar UpdateStatusBarService { get; }
}
