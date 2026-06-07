using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.ThemeMenu;
using CheckDirWritable = SimpleLauncher.Services.CheckIfDirectoryIsWritable.CheckIfDirectoryIsWritable;
using RequiredFiles = SimpleLauncher.Services.CheckForRequiredFiles.CheckForRequiredFiles;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;
using TrayIconManager = SimpleLauncher.Services.TrayIcon.TrayIconManager;

namespace SimpleLauncher.Services.StartupInitialization;

public class StartupInitializationService
{
    private readonly IConfiguration _configuration;
    private readonly Settings _settings;
    private readonly GamePad.GamePadController _gamePadController;
    private readonly ILogErrors _logErrors;
    private readonly ThemeMenuService _themeMenuService;
    private readonly LanguageMenuService _languageMenuService;
    private IStartupInitializationHost _host;

    public StartupInitializationService(
        IConfiguration configuration,
        Settings settings,
        GamePad.GamePadController gamePadController,
        ILogErrors logErrors,
        ThemeMenuService themeMenuService,
        LanguageMenuService languageMenuService)
    {
        _configuration = configuration;
        _settings = settings;
        _gamePadController = gamePadController;
        _logErrors = logErrors;
        _themeMenuService = themeMenuService;
        _languageMenuService = languageMenuService;
    }

    public void Initialize(IStartupInitializationHost host)
    {
        _host = host;

        InitializeStatusBarTimer();
        ApplyInitialThemeAndLanguage();
        InitializeUiState();
        CheckWriteAccess();
        InitializePagination();
        InitializeTrayIcon();
        CheckRequiredFiles();
        InitializeOverlayButtons();
        InitializeGamePad();
    }

    private void InitializeStatusBarTimer()
    {
        var statusBarTimeoutSeconds = _configuration.GetValue("StatusBarTimeoutSeconds", 3);
        _host.StatusBarTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(statusBarTimeoutSeconds)
        };
        _host.StatusBarTimer.Tick += (_, _) =>
        {
            _host.StatusBarText.Content = "";
            _host.StatusBarTimer.Stop();
        };

        DebugLogger.Log("StatusBarTimer was initialized.");
    }

    private void ApplyInitialThemeAndLanguage()
    {
        _languageMenuService.SetLanguageCheckMarks(_settings.Language);
        DebugLogger.Log("Language and menu was set.");

        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        _themeMenuService.SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
        DebugLogger.Log("Theme was set.");
    }

    private void InitializeUiState()
    {
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        _host.SelectedSystem = nosystemselected;
        _host.PlayTime = "00:00:00";
        DebugLogger.Log("SelectedSystem and PlayTime was set.");

        _host.SetViewMode(_settings.ViewMode);
        DebugLogger.Log("ViewMode was set.");
    }

    private void CheckWriteAccess()
    {
        if (!CheckDirWritable.IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory, _logErrors))
        {
            MessageBoxLibrary.MoveToWritableFolderMessageBox();
            DebugLogger.Log("Application does not have write access.");
        }
    }

    private void InitializePagination()
    {
        _host.SetPaginationButtonsDefault();
        DebugLogger.Log("Pagination was set.");
    }

    private void InitializeTrayIcon()
    {
        _host.SetTrayIconManager(new TrayIconManager(_host.HostWindow, _logErrors));
        DebugLogger.Log("TrayIconManager was initialized.");
    }

    private void CheckRequiredFiles()
    {
        RequiredFiles.CheckFiles(_configuration, _logErrors);
        DebugLogger.Log("Required files were checked.");
    }

    private void InitializeOverlayButtons()
    {
        _host.RetroAchievementButton.IsChecked = _settings.OverlayRetroAchievementButton;
        _host.VideoLinkButton.IsChecked = _settings.OverlayOpenVideoButton;
        _host.InfoLinkButton.IsChecked = _settings.OverlayOpenInfoButton;
        DebugLogger.Log("Overlay buttons were set.");
    }

    private void InitializeGamePad()
    {
        _gamePadController.ErrorLogger = (ex, msg) => { _logErrors.LogAndForget(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            _gamePadController.Start();
        }
        else
        {
            _gamePadController.Stop();
        }

        _gamePadController.DeadZoneX = _settings.DeadZoneX;
        _gamePadController.DeadZoneY = _settings.DeadZoneY;
        DebugLogger.Log("GamePadController was initialized.");
    }
}
