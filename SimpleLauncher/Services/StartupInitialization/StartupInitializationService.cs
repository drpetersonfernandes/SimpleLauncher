using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.ThemeMenu;
using CheckDirWritable = SimpleLauncher.Services.CheckIfDirectoryIsWritable.CheckIfDirectoryIsWritable;
using RequiredFiles = SimpleLauncher.Services.CheckForRequiredFiles.CheckForRequiredFiles;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;
using TrayIconManager = SimpleLauncher.Services.TrayIcon.TrayIconManager;

namespace SimpleLauncher.Services.StartupInitialization;

/// <summary>
/// Orchestrates application startup initialization, including theme, language, tray icon, gamepad, and required file checks.
/// </summary>
public class StartupInitializationService
{
    private readonly IConfiguration _configuration;
    private readonly Settings _settings;
    private readonly GamePad.GamePadController _gamePadController;
    private readonly ILogErrors _logErrors;
    private readonly ThemeMenuService _themeMenuService;
    private readonly LanguageMenuService _languageMenuService;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly IDebugLogger _debugLogger;
    private readonly RequiredFiles _requiredFiles;
    private IStartupInitializationHost _host;

    /// <summary>
    /// Initializes a new instance of the StartupInitializationService with the specified dependencies.
    /// </summary>
    public StartupInitializationService(
        IConfiguration configuration,
        Settings settings,
        GamePad.GamePadController gamePadController,
        ILogErrors logErrors,
        ThemeMenuService themeMenuService,
        LanguageMenuService languageMenuService,
        IMessageBoxLibraryService messageBoxLibrary,
        IApplicationLifetime applicationLifetime,
        IDebugLogger debugLogger)
    {
        _configuration = configuration;
        _settings = settings;
        _gamePadController = gamePadController;
        _logErrors = logErrors;
        _themeMenuService = themeMenuService;
        _languageMenuService = languageMenuService;
        _messageBoxLibrary = messageBoxLibrary;
        _applicationLifetime = applicationLifetime;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _requiredFiles = new RequiredFiles(messageBoxLibrary);
    }

    /// <summary>
    /// Performs all startup initialization steps using the provided host for UI interaction.
    /// </summary>
    public async Task InitializeAsync(IStartupInitializationHost host)
    {
        _host = host;

        InitializeStatusBarTimer();
        ApplyInitialThemeAndLanguage();
        InitializeUiState();
        await CheckWriteAccess();
        InitializePagination();
        InitializeTrayIcon();
        await CheckRequiredFiles();
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

        _debugLogger.Log("StatusBarTimer was initialized.");
    }

    private void ApplyInitialThemeAndLanguage()
    {
        _languageMenuService.SetLanguageCheckMarks(_settings.Language);
        _debugLogger.Log("Language and menu was set.");

        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        _themeMenuService.SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
        _debugLogger.Log("Theme was set.");
    }

    private void InitializeUiState()
    {
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        _host.SelectedSystem = nosystemselected;
        _host.PlayTime = "00:00:00";
        _debugLogger.Log("SelectedSystem and PlayTime was set.");

        _host.SetViewMode(_settings.ViewMode);
        _debugLogger.Log("ViewMode was set.");
    }

    private async Task CheckWriteAccess()
    {
        if (!CheckDirWritable.IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory, _logErrors))
        {
            await _messageBoxLibrary.MoveToWritableFolderMessageBox();
            _debugLogger.Log("Application does not have write access.");
        }
    }

    private void InitializePagination()
    {
        _host.SetPaginationButtonsDefault();
        _debugLogger.Log("Pagination was set.");
    }

    private void InitializeTrayIcon()
    {
        _host.SetTrayIconManager(new TrayIconManager(_host.HostWindow, _logErrors, _applicationLifetime, _debugLogger));
        _debugLogger.Log("TrayIconManager was initialized.");
    }

    private async Task CheckRequiredFiles()
    {
        try
        {
            await _requiredFiles.CheckFiles(_configuration, _logErrors);
            _debugLogger.Log("Required files were checked.");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CheckRequiredFiles.");
        }
    }

    private void InitializeOverlayButtons()
    {
        _host.RetroAchievementButton.IsChecked = _settings.OverlayRetroAchievementButton;
        _host.VideoLinkButton.IsChecked = _settings.OverlayOpenVideoButton;
        _host.InfoLinkButton.IsChecked = _settings.OverlayOpenInfoButton;
        _debugLogger.Log("Overlay buttons were set.");
    }

    private void InitializeGamePad()
    {
        _gamePadController.ErrorLogger = (ex, msg) => { _logErrors.LogAndForget(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            _ = _gamePadController.Start();
        }
        else
        {
            _ = _gamePadController.Stop();
        }

        _gamePadController.DeadZoneX = _settings.DeadZoneX;
        _gamePadController.DeadZoneY = _settings.DeadZoneY;
        _debugLogger.Log("GamePadController was initialized.");
    }
}
