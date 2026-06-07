using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Services.GameListUI;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.LoadingOverlay;
using SimpleLauncher.Services.StartupInitialization;
using SimpleLauncher.Services.ThemeMenu;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher;

public partial class MainWindow
{
    // ILoadingOverlayHost
    Dispatcher ILoadingOverlayHost.Dispatcher => Dispatcher;

    void ILoadingOverlayHost.SetIsLoadingGamesInternal(bool value)
    {
        SetIsLoadingGamesInternal(value);
    }

    void ILoadingOverlayHost.SetLoadingOverlayVisibility(Visibility visibility)
    {
        LoadingOverlay.Visibility = visibility;
    }

    void ILoadingOverlayHost.SetLoadingOverlayContent(object content)
    {
        LoadingOverlay.Content = content;
    }

    void ILoadingOverlayHost.SetMainContentGridEnabled(bool enabled)
    {
        MainContentGrid.IsEnabled = enabled;
    }

    void ILoadingOverlayHost.CancelAndRecreateToken()
    {
        CancelAndRecreateToken();
    }

    async Task ILoadingOverlayHost.ResetUiAsync()
    {
        ResetUiAsync();
    }

    IUpdateStatusBar ILoadingOverlayHost.UpdateStatusBarService => UpdateStatusBarService;

    // IGameListUiHost
    Dispatcher IGameListUiHost.Dispatcher => Dispatcher;
    ScrollViewer IGameListUiHost.Scroller => Scroller;
    Image IGameListUiHost.PreviewImage => PreviewImage;
    WrapPanel IGameListUiHost.GameFileGrid => GameFileGrid;
    Grid IGameListUiHost.ListViewPreviewArea => ListViewPreviewArea;

    void IGameListUiHost.SetGameFileGridVisibility(Visibility visibility)
    {
        GameFileGrid.Visibility = visibility;
    }

    void IGameListUiHost.SetListViewPreviewAreaVisibility(Visibility visibility)
    {
        ListViewPreviewArea.Visibility = visibility;
    }

    void IGameListUiHost.SetPaginationButtonsVisibility(Visibility visibility)
    {
        SetPaginationButtonsVisibility(visibility);
    }

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
