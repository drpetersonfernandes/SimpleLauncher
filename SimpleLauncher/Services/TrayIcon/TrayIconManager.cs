using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.QuitOrReinstall;

namespace SimpleLauncher.Services.TrayIcon;

public class TrayIconManager : IDisposable
{
    private static TrayIconManager _instance;
    private readonly TaskbarIcon _taskbarIcon;
    private readonly System.Windows.Controls.ContextMenu _trayMenu;
    private readonly Window _mainWindow;
    private readonly ILogErrors _logErrors;

    private readonly RoutedEventHandler _onOpenHandler;
    private readonly RoutedEventHandler _onMinimizeToTrayHandler;
    private readonly RoutedEventHandler _onExitHandler;
    private readonly RoutedEventHandler _onOpenDebugWindowHandler;
    private readonly RoutedEventHandler _trayMouseDoubleClickHandler;

    public TrayIconManager(Window mainWindow, ILogErrors logErrors)
    {
        _instance = this;
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));

        // Initialize delegates with correct types
        _onOpenHandler = OnOpen;
        _onMinimizeToTrayHandler = OnMinimizeToTray;
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

    private System.Windows.Controls.ContextMenu CreateContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();
        var open = (string)Application.Current.TryFindResource("Open") ?? "Open";
        var minimizeToTray = (string)Application.Current.TryFindResource("MinimizeToTray") ?? "Minimize to Tray";
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

        var minimizeToTrayMenuItem = new MenuItem
        {
            Header = minimizeToTray,
            Icon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/shrink.png")),
                Width = 16,
                Height = 16
            }
        };
        minimizeToTrayMenuItem.Click += _onMinimizeToTrayHandler;

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
        menu.Items.Add(minimizeToTrayMenuItem);
        menu.Items.Add(new Separator());
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
        _mainWindow.ShowInTaskbar = true;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void OnMinimizeToTray(object sender, RoutedEventArgs e)
    {
        _mainWindow.Hide();
        _mainWindow.ShowInTaskbar = false;
    }

    private void OnOpenDebugWindow(object sender, RoutedEventArgs e)
    {
        try
        {
            // Initialize debug mode if it wasn't already enabled,
            DebugLogger.Initialize(true);

            // Initialize DebugWindow if it doesn't exist yet
            DebugWindow.Initialize();

            // Show the debug window
            if (DebugWindow.Instance == null) return;

            DebugWindow.Instance.Show();
            DebugWindow.Instance.WindowState = WindowState.Normal;
            DebugWindow.Instance.Activate();

            // Log that the debug window was opened from tray
            DebugLogger.Log("Debug window opened from tray menu");
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Failed to open debug window from tray menu");

            ShowTrayMessage("Failed to open debug window");
        }
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        _taskbarIcon.Visibility = Visibility.Collapsed;
        QuitSimpleLauncher.SimpleQuitApplication();
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
                menuItem.Click -= _onMinimizeToTrayHandler;
                menuItem.Click -= _onExitHandler;
                menuItem.Click -= _onOpenDebugWindowHandler;
            }
        }

        _instance = null;
        GC.SuppressFinalize(this);
    }
}