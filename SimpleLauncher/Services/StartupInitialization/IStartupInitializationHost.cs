using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Services.TrayIcon;

namespace SimpleLauncher.Services.StartupInitialization;

public interface IStartupInitializationHost
{
    DispatcherTimer StatusBarTimer { get; set; }
    Label StatusBarText { get; }
    string SelectedSystem { get; set; }
    string PlayTime { get; set; }
    MenuItem RetroAchievementButton { get; }
    MenuItem VideoLinkButton { get; }
    MenuItem InfoLinkButton { get; }
    Window HostWindow { get; }
    void SetViewMode(string viewMode);
    void SetPaginationButtonsDefault();
    void SetTrayIconManager(TrayIconManager manager);
}
