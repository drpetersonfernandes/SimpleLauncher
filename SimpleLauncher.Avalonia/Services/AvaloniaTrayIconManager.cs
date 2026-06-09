using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using SimpleLauncher.Avalonia.Views;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using IApplicationLifetime = SimpleLauncher.Core.Interfaces.IApplicationLifetime;

namespace SimpleLauncher.Avalonia.Services;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public class AvaloniaTrayIconManager : IDisposable
{
    private static AvaloniaTrayIconManager? _instance;
    private TrayIcon? _trayIcon;
    private readonly Window _mainWindow;
    private readonly ILogErrors _logErrors;
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly IServiceProvider _serviceProvider;

    public AvaloniaTrayIconManager(Window mainWindow, ILogErrors logErrors, IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
    {
        _instance = this;
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _trayIcon = CreateTrayIcon();
    }

    private TrayIcon CreateTrayIcon()
    {
        var trayIcon = new TrayIcon
        {
            ToolTipText = "Simple Launcher",
            IsVisible = true
        };

        // Set the icon
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon", "icon.ico");
            if (File.Exists(iconPath))
            {
                trayIcon.Icon = new WindowIcon(iconPath);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to load tray icon");
        }

        // Create context menu
        var menu = new NativeMenu();

        var openItem = new NativeMenuItem("Open");
        openItem.Click += (_, _) => OnOpen();
        menu.Add(openItem);

        var minimizeItem = new NativeMenuItem("Minimize to Tray");
        minimizeItem.Click += (_, _) => OnMinimizeToTray();
        menu.Add(minimizeItem);

        menu.Add(new NativeMenuItemSeparator());

        var debugItem = new NativeMenuItem("Debug Window");
        debugItem.Click += (_, _) => OnOpenDebugWindow();
        menu.Add(debugItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) => OnExit();
        menu.Add(exitItem);

        trayIcon.Menu = menu;

        // Handle double-click on tray icon
        trayIcon.Clicked += (_, _) => OnOpen();

        return trayIcon;
    }

    private void OnOpen()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void OnMinimizeToTray()
    {
        _mainWindow.Hide();
    }

    private void OnOpenDebugWindow()
    {
        try
        {
            if (_serviceProvider.GetService(typeof(DebugWindow)) is DebugWindow debugWindow)
            {
                debugWindow.Show();
                debugWindow.WindowState = WindowState.Normal;
                debugWindow.Activate();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to open debug window from tray menu");
            ShowTrayMessage("Failed to open debug window");
        }
    }

    private void OnExit()
    {
        _trayIcon?.IsVisible = false;
        _applicationLifetime.Shutdown();
    }

    public static void ShowTrayMessage(string message)
    {
        // Avalonia 12 doesn't have built-in balloon tip support
        // Log the message instead
        System.Diagnostics.Debug.WriteLine($"[Tray] {message}");
    }

    public void Dispose()
    {
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        _instance = null;
        GC.SuppressFinalize(this);
    }
}
