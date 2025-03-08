using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace SimpleLauncher;

public class TrayIconManager : IDisposable
{
    private readonly TaskbarIcon _taskbarIcon;
    private readonly ContextMenu _trayMenu;
    private readonly Window _mainWindow;

    public TrayIconManager(Window mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        // Create a context menu for the tray icon
        _trayMenu = new ContextMenu();

        var open = (string)Application.Current.TryFindResource("Open") ?? "Open";
        var exit = (string)Application.Current.TryFindResource("Exit") ?? "Exit";

        // Create menu items
        var openMenuItem = new MenuItem { Header = open };
        openMenuItem.Click += OnOpen;
        var exitMenuItem = new MenuItem { Header = exit };
        exitMenuItem.Click += OnExit;

        // Add items to the menu
        _trayMenu.Items.Add(openMenuItem);
        _trayMenu.Items.Add(exitMenuItem);

        // Create the TaskbarIcon
        _taskbarIcon = new TaskbarIcon
        {
            IconSource = new BitmapImage(new Uri("pack://application:,,,/SimpleLauncher;component/icon/icon.ico")),
            ToolTipText = "Simple Launcher",
            ContextMenu = _trayMenu,
            Visibility = Visibility.Visible
        };

        // Handle tray icon events
        _taskbarIcon.TrayMouseDoubleClick += OnOpen;

        // Subscribe to the main window's state changed event
        _mainWindow.StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object sender, EventArgs e)
    {
        if (_mainWindow.WindowState == WindowState.Minimized)
        {
            _mainWindow.Hide();
            // Retrieve the dynamic resource string
            var isminimizedtothetray = (string)Application.Current.TryFindResource("isminimizedtothetray") ?? "is minimized to the tray.";
            ShowTrayMessage($"Simple Launcher {isminimizedtothetray}");
        }
    }

    // Handle "Open" context menu item or tray icon double-click
    private void OnOpen(object sender, RoutedEventArgs e)
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    // Handle "Exit" context menu item
    private void OnExit(object sender, RoutedEventArgs e)
    {
        _taskbarIcon.Visibility = Visibility.Collapsed;
        Application.Current.Shutdown();
    }

    // Display a balloon message
    private void ShowTrayMessage(string message)
    {
        _taskbarIcon.ShowBalloonTip("Simple Launcher", message, BalloonIcon.Info);
    }

    public void Dispose()
    {
        // Unsubscribe from events
        if (_taskbarIcon != null)
        {
            _taskbarIcon.TrayMouseDoubleClick -= OnOpen;
            _taskbarIcon.Dispose();
        }

        if (_mainWindow != null)
        {
            _mainWindow.StateChanged -= MainWindow_StateChanged;
        }

        // Remove menu item event handlers
        if (_trayMenu != null)
        {
            foreach (var item in _trayMenu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Click -= OnOpen;
                    menuItem.Click -= OnExit;
                }
            }
        }
    }
}