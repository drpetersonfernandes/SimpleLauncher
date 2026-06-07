using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.StartupInitialization;
using SimpleLauncher.Services.ThemeMenu;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher;

public partial class MainWindow
{
    // IStartupInitializationHost
    DispatcherTimer IStartupInitializationHost.StatusBarTimer
    {
        get => StatusBarTimer;
        set => StatusBarTimer = value;
    }

    Label IStartupInitializationHost.StatusBarText => StatusBarText;
    MenuItem IStartupInitializationHost.RetroAchievementButton => RetroAchievementButton;
    MenuItem IStartupInitializationHost.VideoLinkButton => VideoLinkButton;
    MenuItem IStartupInitializationHost.InfoLinkButton => InfoLinkButton;
    Window IStartupInitializationHost.HostWindow => this;

    void IStartupInitializationHost.SetViewMode(string viewMode)
    {
        SetViewMode(viewMode);
    }

    void IStartupInitializationHost.SetPaginationButtonsDefault()
    {
        SetPaginationButtonsDefault();
    }

    void IStartupInitializationHost.SetTrayIconManager(TrayIconManager manager)
    {
        SetTrayIconManager(manager);
    }

    // IThemeMenuHost
    MenuItem IThemeMenuHost.FindMenuItemByName(string name)
    {
        return FindName(name) as MenuItem;
    }

    // ILanguageMenuHost
    MenuItem ILanguageMenuHost.FindMenuItemByName(string name)
    {
        return FindName(name) as MenuItem;
    }

    IUpdateStatusBar ILanguageMenuHost.UpdateStatusBarService => UpdateStatusBarService;

    // IStatusBarHost
    Dispatcher IStatusBarHost.Dispatcher => Dispatcher;
    Label IStatusBarHost.StatusBarText => StatusBarText;
    DispatcherTimer IStatusBarHost.StatusBarTimer => StatusBarTimer;
}
