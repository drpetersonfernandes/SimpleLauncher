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
    private readonly ILogger _logger;
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
        ILogger logger)
    {
        _configuration = configuration;
        _settings = settings;
        _gamePadController = gamePadController;
        _logErrors = logErrors;
        _themeMenuService = themeMenuService;
        _languageMenuService = languageMenuService;
        _messageBoxLibrary = messageBoxLibrary;
        _applicationLifetime = applicationLifetime;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        await CheckWriteAccessAsync();
        InitializePagination();
        InitializeTrayIcon();
        await CheckRequiredFilesAsync();
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

        _logger.Debug("StatusBarTimer was initialized.");
    }

    private void ApplyInitialThemeAndLanguage()
    {
        _languageMenuService.SetLanguageCheckMarks(_settings.Language);
        _logger.Debug("Language and menu was set.");

        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        _themeMenuService.SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
        _logger.Debug("Theme was set.");
    }

    private void InitializeUiState()
    {
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        _host.SelectedSystem = nosystemselected;
        _host.PlayTime = "00:00:00";
        _logger.Debug("SelectedSystem and PlayTime was set.");

        _host.SetViewMode(_settings.ViewMode);
        _logger.Debug("ViewMode was set.");
    }

    private async Task CheckWriteAccessAsync()
    {
        if (!CheckDirWritable.IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory, _logErrors))
        {
            await _messageBoxLibrary.MoveToWritableFolderMessageBoxAsync();
            _logger.Debug("Application does not have write access.");
        }
    }

    private void InitializePagination()
    {
        _host.SetPaginationButtonsDefault();
        _logger.Debug("Pagination was set.");
    }

    private void InitializeTrayIcon()
    {
        _host.SetTrayIconManager(new TrayIconManager(_host.HostWindow, _logErrors, _applicationLifetime, _logger));
        _logger.Debug("TrayIconManager was initialized.");
    }

    private async Task CheckRequiredFilesAsync()
    {
        try
        {
            await _requiredFiles.CheckFilesAsync(_configuration, _logErrors);
            _logger.Debug("Required files were checked.");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CheckRequiredFilesAsync.");
        }
    }

    private void InitializeOverlayButtons()
    {
        _host.RetroAchievementButton.IsChecked = _settings.OverlayRetroAchievementButton;
        _host.VideoLinkButton.IsChecked = _settings.OverlayOpenVideoButton;
        _host.InfoLinkButton.IsChecked = _settings.OverlayOpenInfoButton;
        _logger.Debug("Overlay buttons were set.");
    }

    private void InitializeGamePad()
    {
        _gamePadController.ErrorLogger = (ex, msg) => { _logErrors.LogAndForget(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            _ = _gamePadController.StartAsync();
        }
        else
        {
            _ = _gamePadController.StopAsync();
        }

        _gamePadController.DeadZoneX = _settings.DeadZoneX;
        _gamePadController.DeadZoneY = _settings.DeadZoneY;
        _logger.Debug("GamePadController was initialized.");
    }
}
