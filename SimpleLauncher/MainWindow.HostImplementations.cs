using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.TrayIcon;

namespace SimpleLauncher;

/// <summary>
///     Partial MainWindow implementing host interfaces for startup initialization, theming, language, status bar, and tray
///     icon.
/// </summary>
public partial class MainWindow
{
    // ILanguageMenuHost
    MenuItem? ILanguageMenuHost.FindMenuItemByName(string name)
    {
        return FindName(name) as MenuItem;
    }

    IUpdateStatusBar ILanguageMenuHost.UpdateStatusBarService => UpdateStatusBarService;

    // IStartupInitializationHost
    DispatcherTimer? IStartupInitializationHost.StatusBarTimer
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

    // IStatusBarHost
    Dispatcher IStatusBarHost.Dispatcher => Dispatcher;
    Label IStatusBarHost.StatusBarText => StatusBarText;
    DispatcherTimer? IStatusBarHost.StatusBarTimer => StatusBarTimer;

    // IThemeMenuHost
    MenuItem? IThemeMenuHost.FindMenuItemByName(string name)
    {
        return FindName(name) as MenuItem;
    }
}