using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using SimpleLauncher.Services; // Assuming SettingsManager is in SimpleLauncher.Managers

namespace SimpleLauncher.Managers;

public class TrayIconManager : IDisposable
{
    private readonly TaskbarIcon _taskbarIcon;
    private readonly ContextMenu _trayMenu;
    private readonly Window _mainWindow;
    private readonly SettingsManager _settings; // <<< ADDED: To access gamepad settings

    // Modify constructor to accept SettingsManager
    public TrayIconManager(Window mainWindow, SettingsManager settings) // <<< MODIFIED
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings)); // <<< ADDED

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
                if (item is not MenuItem menuItem) continue;

                // Check specific handlers before removing
                // This assumes OnOpen and OnExit are the only handlers attached here.
                // A more robust way would be to store delegates if more complex.
                if (menuItem.Header.ToString() == ((string)Application.Current.TryFindResource("Open") ?? "Open"))
                {
                    menuItem.Click -= OnOpen;
                }
                else if (menuItem.Header.ToString() == ((string)Application.Current.TryFindResource("Exit") ?? "Exit"))
                {
                    menuItem.Click -= OnExit;
                }
            }
        }

        GC.SuppressFinalize(this); // Moved from original code to be standard last line
    }
}
