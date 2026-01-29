using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Managers;

public class TrayIconManager : IDisposable
{
    private static TrayIconManager _instance;
    private readonly TaskbarIcon _taskbarIcon;
    private readonly ContextMenu _trayMenu;
    private readonly Window _mainWindow;

    // Updated delegate types
    private readonly RoutedEventHandler _onOpenHandler;
    private readonly RoutedEventHandler _onExitHandler;
    private readonly RoutedEventHandler _onOpenDebugWindowHandler;
    private readonly RoutedEventHandler _trayMouseDoubleClickHandler;

    public TrayIconManager(Window mainWindow)
    {
        _instance = this;
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        // Initialize delegates with correct types
        _onOpenHandler = OnOpen;
        _onExitHandler = OnExit;
        _onOpenDebugWindowHandler = OnOpenDebugWindow;
        _trayMouseDoubleClickHandler = OnOpen;

        // Create context menu
        _trayMenu = CreateContextMenu();

        // Create and setup TaskbarIcon
        _taskbarIcon = CreateTaskbarIcon();

        // Subscribe to events using stored delegates
        _taskbarIcon.TrayMouseDoubleClick += _trayMouseDoubleClickHandler;
    }

    private ContextMenu CreateContextMenu()
    {
        var menu = new ContextMenu();
        var open = (string)Application.Current.TryFindResource("Open") ?? "Open";
        var exit = (string)Application.Current.TryFindResource("Exit") ?? "Exit";
        var debugWindow = (string)Application.Current.TryFindResource("DebugWindow") ?? "Debug Window";

        var openMenuItem = new MenuItem
        {
            Header = open,
            Icon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/play.png")),
                Width = 16,
                Height = 16
            }
        };
        openMenuItem.Click += _onOpenHandler;

        var debugWindowMenuItem = new MenuItem
        {
            Header = debugWindow,
            Icon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/bug.png")),
                Width = 16,
                Height = 16
            }
        };
        debugWindowMenuItem.Click += _onOpenDebugWindowHandler;

        var exitMenuItem = new MenuItem
        {
            Header = exit,
            Icon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/exit.png")),
                Width = 16,
                Height = 16
            }
        };
        exitMenuItem.Click += _onExitHandler;

        menu.Items.Add(openMenuItem);
        menu.Items.Add(debugWindowMenuItem);
        menu.Items.Add(exitMenuItem);

        return menu;
    }

    private TaskbarIcon CreateTaskbarIcon()
    {
        return new TaskbarIcon
        {
            IconSource = new BitmapImage(new Uri("pack://application:,,,/SimpleLauncher;component/icon/icon.ico")),
            ToolTipText = "Simple Launcher",
            ContextMenu = _trayMenu,
            Visibility = Visibility.Visible
        };
    }

    private void OnOpen(object sender, RoutedEventArgs e)
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void OnOpenDebugWindow(object sender, RoutedEventArgs e)
    {
        try
        {
            // Initialize debug mode if it wasn't already enabled,
            DebugLogger.Initialize(true);

            // Initialize LogWindow if it doesn't exist yet
            LogWindow.Initialize();

            // Show the debug window
            if (LogWindow.Instance == null) return;

            LogWindow.Instance.Show();
            LogWindow.Instance.WindowState = WindowState.Normal;
            LogWindow.Instance.Activate();

            // Log that the debug window was opened from tray
            DebugLogger.Log("Debug window opened from tray menu");
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to open debug window from tray menu");

            ShowTrayMessage("Failed to open debug window");
        }
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        _taskbarIcon.Visibility = Visibility.Collapsed;
        QuitApplication.SimpleQuitApplication();
    }

    public static void ShowTrayMessage(string message)
    {
        _instance?._taskbarIcon.ShowBalloonTip("Simple Launcher", message, BalloonIcon.Info);
    }

    public void Dispose()
    {
        if (_taskbarIcon != null)
        {
            _taskbarIcon.TrayMouseDoubleClick -= _trayMouseDoubleClickHandler;
            _taskbarIcon.Dispose();
        }

        if (_trayMenu != null)
        {
            foreach (var item in _trayMenu.Items)
            {
                if (item is not MenuItem menuItem) continue;

                menuItem.Click -= _onOpenHandler;
                menuItem.Click -= _onExitHandler;
                menuItem.Click -= _onOpenDebugWindowHandler;
            }
        }

        _instance = null;
        GC.SuppressFinalize(this);
    }
}