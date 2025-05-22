using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

public class TrayIconManager : IDisposable
{
    private readonly TaskbarIcon _taskbarIcon;
    private readonly ContextMenu _trayMenu;
    private readonly Window _mainWindow;
    private readonly SettingsManager _settings;

    // Updated delegate types
    private readonly RoutedEventHandler _onOpenHandler;
    private readonly RoutedEventHandler _onExitHandler;
    private readonly EventHandler _mainWindowStateChangedHandler;
    private readonly RoutedEventHandler _trayMouseDoubleClickHandler;

    public TrayIconManager(Window mainWindow, SettingsManager settings)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        // Initialize delegates with correct types
        _onOpenHandler = OnOpen;
        _onExitHandler = OnExit;
        _mainWindowStateChangedHandler = MainWindow_StateChanged;
        _trayMouseDoubleClickHandler = OnOpen;

        // Create context menu
        _trayMenu = CreateContextMenu();

        // Create and setup TaskbarIcon
        _taskbarIcon = CreateTaskbarIcon();

        // Subscribe to events using stored delegates
        _taskbarIcon.TrayMouseDoubleClick += _trayMouseDoubleClickHandler;
        _mainWindow.StateChanged += _mainWindowStateChangedHandler;
    }

    private ContextMenu CreateContextMenu()
    {
        var menu = new ContextMenu();
        var open = (string)Application.Current.TryFindResource("Open") ?? "Open";
        var exit = (string)Application.Current.TryFindResource("Exit") ?? "Exit";

        var openMenuItem = new MenuItem { Header = open };
        openMenuItem.Click += _onOpenHandler;

        var exitMenuItem = new MenuItem { Header = exit };
        exitMenuItem.Click += _onExitHandler;

        menu.Items.Add(openMenuItem);
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

    private void MainWindow_StateChanged(object sender, EventArgs e)
    {
        if (_mainWindow.WindowState != WindowState.Minimized) return;

        _mainWindow.Hide();
        var isminimizedtothetray = (string)Application.Current.TryFindResource("isminimizedtothetray") ?? "is minimized to the tray.";
        ShowTrayMessage($"Simple Launcher {isminimizedtothetray}");

        // <<< ADDED: Stop GamePadController if it's running
        if (GamePadController.Instance2.IsRunning)
        {
            GamePadController.Instance2.Stop();
        }
        // No 'else' needed here for restoring, OnOpen handles explicit restore from tray.
    }

    // Handle "Open" context menu item or tray icon double-click
    private void OnOpen(object sender, RoutedEventArgs e)
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();

        // <<< ADDED: Start GamePadController if allowed by settings and not already running
        if (_settings.EnableGamePadNavigation && !GamePadController.Instance2.IsRunning)
        {
            GamePadController.Instance2.Start();
        }
    }

    // Handle "Exit" context menu item
    private void OnExit(object sender, RoutedEventArgs e)
    {
        _taskbarIcon.Visibility = Visibility.Collapsed;
        QuitApplication.SimpleQuitApplication();
    }

    // Display a balloon message
    private void ShowTrayMessage(string message)
    {
        _taskbarIcon.ShowBalloonTip("Simple Launcher", message, BalloonIcon.Info);
    }

    public void Dispose()
    {
        if (_taskbarIcon != null)
        {
            _taskbarIcon.TrayMouseDoubleClick -= _trayMouseDoubleClickHandler;
            _taskbarIcon.Dispose();
        }

        if (_mainWindow != null)
        {
            _mainWindow.StateChanged -= _mainWindowStateChangedHandler;
        }

        if (_trayMenu != null)
        {
            foreach (var item in _trayMenu.Items)
            {
                if (item is not MenuItem menuItem) continue;

                menuItem.Click -= _onOpenHandler;
                menuItem.Click -= _onExitHandler;
            }
        }

        GC.SuppressFinalize(this);
    }
}