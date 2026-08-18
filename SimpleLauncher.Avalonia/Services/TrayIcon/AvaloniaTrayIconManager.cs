using Avalonia;
using Avalonia.Controls;
using SimpleLauncher.Core.Interfaces;
using TrayIconControl = Avalonia.Controls.TrayIcon;

namespace SimpleLauncher.Avalonia.Services.TrayIcon;

/// <summary>
/// Manages the system tray icon, its native context menu, and related actions for the application.
/// Cross-platform Avalonia port of the WPF TrayIconManager (Avalonia TrayIcon supports Windows and Linux).
/// </summary>
public class AvaloniaTrayIconManager : IDisposable
{
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly ILogger _logger;

    private Window? _mainWindow;
    private TrayIconControl? _trayIcon;
    private bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="AvaloniaTrayIconManager"/> class.</summary>
    /// <param name="applicationLifetime">The application lifetime service for shutdown control.</param>
    /// <param name="logger">The logger instance.</param>
    public AvaloniaTrayIconManager(IApplicationLifetime applicationLifetime, ILogger logger)
    {
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates the tray icon and registers it on the application.
    /// Must be called from the UI thread after the main window is shown.
    /// </summary>
    /// <param name="mainWindow">The main application window.</param>
    public void Initialize(Window mainWindow)
    {
        if (_isDisposed) return;

        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        _trayIcon = new TrayIconControl
        {
            ToolTipText = "Simple Launcher",
            Icon = LoadIcon(),
            Menu = CreateContextMenu(),
            IsVisible = true
        };
        _trayIcon.Clicked += (_, _) => OnOpen();

        TrayIconControl.SetIcons(Application.Current!, [_trayIcon]);

        _logger.Debug("AvaloniaTrayIconManager was initialized.");
    }

    private static WindowIcon? LoadIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "icon", "icon.ico");
            return File.Exists(iconPath) ? new WindowIcon(iconPath) : null;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to load the tray icon.");
            return null;
        }
    }

    private NativeMenu CreateContextMenu()
    {
        var menu = new NativeMenu();

        var openItem = new NativeMenuItem("Open");
        openItem.Click += (_, _) => OnOpen();

        var minimizeToTrayItem = new NativeMenuItem("Minimize to Tray");
        minimizeToTrayItem.Click += (_, _) => OnMinimizeToTray();

        var debugWindowItem = new NativeMenuItem("Debug Window");
        debugWindowItem.Click += (_, _) => OnOpenDebugWindow();

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) => OnExit();

        menu.Items.Add(openItem);
        menu.Items.Add(minimizeToTrayItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(debugWindowItem);
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnOpen()
    {
        if (_mainWindow is not { } window) return;

        window.ShowInTaskbar = true;
        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private void OnMinimizeToTray()
    {
        if (_mainWindow is not { } window) return;

        window.Hide();
        window.ShowInTaskbar = false;
    }

    private void OnOpenDebugWindow()
    {
        try
        {
            DebugWindow.ShowDebugWindow();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open the debug window from the tray menu.");
        }
    }

    private void OnExit()
    {
        if (_trayIcon is not null)
        {
            _trayIcon.IsVisible = false;
        }

        _applicationLifetime.Shutdown();
    }

    /// <summary>Releases resources used by the tray icon manager.</summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;

        try
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Error disposing the tray icon.");
        }

        GC.SuppressFinalize(this);
    }
}