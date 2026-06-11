using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.ContextMenu;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.GameScan;
using SimpleLauncher.Services.MenuCheckMark;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.QuitOrReinstall;
using SimpleLauncher.Services.UpdateStatusBar;
using MessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.MenuActionHandler;

public class MenuActionHandlerService
{
    private readonly Settings _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;

    // ReSharper disable once NotAccessedField.Local
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher.GameLauncher _gameLauncher;
    private readonly GameScannerService _gameScannerService;
    private readonly FavoritesManager _favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHelpUserService _helpUserService;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly IMenuCheckMarkService _menuCheckMarkService;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;
    private readonly IDebugLogger _debugLogger;

    private IMenuActionHost _host;
    private readonly IUpdateStatusBar _updateStatusBar;

    private readonly Dictionary<string, Action> _emulatorConfigWindowFactory;

    public MenuActionHandlerService(
        Settings settings,
        PlaySoundEffects playSoundEffects,
        ILogErrors logErrors,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        GamePadController gamePadController,
        GameLauncher.GameLauncher gameLauncher,
        GameScannerService gameScannerService,
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        IHelpUserService helpUserService,
        IGetListOfFilesService getListOfFiles,
        IServiceProvider serviceProvider,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IMenuCheckMarkService menuCheckMarkService,
        IMessageBoxLibraryService messageBoxLibrary,
        IUpdateStatusBar updateStatusBar,
        QuitSimpleLauncher quitSimpleLauncher,
        IDebugLogger debugLogger)
    {
        _settings = settings;
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _gamePadController = gamePadController;
        _gameLauncher = gameLauncher;
        _gameScannerService = gameScannerService;
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _helpUserService = helpUserService;
        _getListOfFiles = getListOfFiles;
        _serviceProvider = serviceProvider;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _menuCheckMarkService = menuCheckMarkService;
        _messageBoxLibrary = messageBoxLibrary;
        _updateStatusBar = updateStatusBar;
        _quitSimpleLauncher = quitSimpleLauncher;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));

        _emulatorConfigWindowFactory = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            ["Xenia"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectXeniaConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Mame"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectMameConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["RetroArch"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectRetroArchConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Supermodel"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectSupermodelConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Mednafen"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectMednafenConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["SegaModel2"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectSegaModel2ConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Ares"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectAresConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Daphne"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectDaphneConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(false);
                w.ShowDialog();
            },
            ["Blastem"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectBlastemConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Mesen"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectMesenConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["DuckStation"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectDuckStationConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["RPCS3"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectRpcs3ConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Flycast"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectFlycastConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Stella"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectStellaConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Dolphin"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectDolphinConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Cemu"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectCemuConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["PCSX2"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectPcsx2ConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Azahar"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectAzaharConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Yumir"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectYumirConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Raine"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectRaineConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            },
            ["Redream"] = () =>
            {
                var w = serviceProvider.GetRequiredService<InjectRedreamConfigWindow>();
                w.Owner = Application.Current.MainWindow;
                w.Initialize(null, false);
                w.ShowDialog();
            }
        };
    }

    public void Initialize(IMenuActionHost host)
    {
        _host = host;
    }

    // ---- Emulator Config Windows ----

    public void ShowEmulatorConfigWindow(string emulatorName)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent($"Opening {emulatorName} configuration...");

            if (!_emulatorConfigWindowFactory.TryGetValue(emulatorName, out var showWindow))
            {
                _logErrors.LogAndForget(null, $"Unknown emulator config: {emulatorName}");
                return;
            }

            showWindow();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error opening {emulatorName} configuration window.");
        }
    }

    // ---- Easy Mode / Expert Mode ----

    public void HandleEasyMode()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningEasyMode") ?? "Opening Easy Mode...");

            var easyModeWindow = _serviceProvider.GetRequiredService<EasyModeWindow>();
            easyModeWindow.Owner = Application.Current.MainWindow;
            easyModeWindow.ShowDialog();

            _host.LoadOrReloadSystemManager();
            _ = _host.ResetUiAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method EasyMode_Click.");
        }
    }

    public void HandleExpertMode()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningExpertMode") ?? "Opening Expert Mode...");

            var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
            var selectedSystem = _host.GetSelectedSystem();
            var systemToPreselect = !string.IsNullOrEmpty(selectedSystem) && selectedSystem != nosystemselected
                ? selectedSystem
                : null;

            var editSystemWindow = new EditSystemWindow(_settings, _playSoundEffects, _configuration, _logErrors, _helpUserService, _imageLoader, _messageBoxLibrary, _quitSimpleLauncher, _debugLogger, systemToPreselect)
            {
                Owner = Application.Current.MainWindow
            };
            editSystemWindow.ShowDialog();

            _host.LoadOrReloadSystemManager();
            _ = _host.ResetUiAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ExpertMode_Click.");
        }
    }

    // ---- Download Image Pack ----

    public void HandleDownloadImagePack()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningImagePackDownloader") ?? "Opening Image Pack Downloader...");

            _ = _host.ResetUiAsync();

            var downloadImagePack = _serviceProvider.GetRequiredService<DownloadImagePackWindow>();
            downloadImagePack.Owner = Application.Current.MainWindow;
            downloadImagePack.ShowDialog();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method DownloadImagePack_Click.");
        }
    }

    // ---- Scan for Windows Games ----

    public async Task HandleScanForWindowsGames()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ScanningForWindowsGames") ?? "Scanning for Windows games...");
            await Task.Yield();
            try
            {
                await _gameScannerService.ScanForStoreGamesAsync();
                await Task.Delay(2000, _host.CurrentCancellationToken);
                _host.LoadOrReloadSystemManager();
                await _host.ResetUiAsync();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in method ScanForMicrosoftWindowsGames_Click.");
            }
            finally
            {
                _host.SetLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ScanForMicrosoftWindowsGames_Click.");
        }
    }

    // ---- Edit Links ----

    public async Task HandleEditLinks()
    {
        try
        {
            _host.CancelAndRecreateToken();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningLinkSettings") ?? "Opening link settings...");
            _playSoundEffects.PlayNotificationSound();

            var setLinksWindow = _serviceProvider.GetRequiredService<SetLinksWindow>();
            setLinksWindow.Owner = Application.Current.MainWindow;
            setLinksWindow.ShowDialog();

            var (sl, sq) = _host.GetLoadGameFilesParams();
            _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
            await Task.Yield();
            await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method EditLinksClickAsync.");
        }
    }

    // ---- Toggle Gamepad ----

    public async Task HandleToggleGamepad(bool isChecked)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.EnableGamePadNavigation = isChecked;
            await _settings.SaveAsync();

            if (isChecked)
                await _gamePadController.Start();
            else
                await _gamePadController.Stop();

            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingGamepadNavigation") ?? "Toggling gamepad navigation...");
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to toggle gamepad.";
            _logErrors.LogAndForget(ex, contextMessage);
            await _messageBoxLibrary.ToggleGamepadFailureMessageBox();
        }
    }

    // ---- Set Gamepad Dead Zone ----

    public void HandleSetGamepadDeadZone()
    {
        _playSoundEffects.PlayNotificationSound();
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGamepadDeadZoneSettings") ?? "Opening Gamepad Dead Zone settings...");

        var setGamepadDeadZoneWindow = _serviceProvider.GetRequiredService<SetGamepadDeadZoneWindow>();
        setGamepadDeadZoneWindow.ShowDialog();

        _gamePadController.DeadZoneX = _settings.DeadZoneX;
        _gamePadController.DeadZoneY = _settings.DeadZoneY;

        if (_settings.EnableGamePadNavigation)
        {
            _ = _gamePadController.Stop();
            _ = _gamePadController.Start();
        }
        else
        {
            _ = _gamePadController.Stop();
        }
    }

    // ---- Toggle Fuzzy Matching ----

    public async Task HandleToggleFuzzyMatching(bool isChecked)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilityFilter") ?? "Applying game visibility filter...");
                _playSoundEffects.PlayNotificationSound();

                _settings.EnableFuzzyMatching = isChecked;
                await _settings.SaveAsync();

                var (sl, sq) = _host.GetLoadGameFilesParams();
                _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                await Task.Yield();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingFuzzyMatching") ?? "Toggling fuzzy matching...");
            }
            catch (Exception ex)
            {
                const string contextMessage = "Failed to toggle fuzzy matching.";
                _logErrors.LogAndForget(ex, contextMessage);
                await _messageBoxLibrary.ToggleFuzzyMatchingFailureMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ToggleFuzzyMatchingClickAsync.");
        }
    }

    // ---- Set Fuzzy Matching Threshold ----

    public async Task HandleSetFuzzyMatchingThreshold()
    {
        try
        {
            _host.CancelAndRecreateToken();
            _playSoundEffects.PlayNotificationSound();

            var setThresholdWindow = _serviceProvider.GetRequiredService<SetFuzzyMatchingWindow>();
            setThresholdWindow.ShowDialog();

            if (!_settings.EnableFuzzyMatching) return;

            var (sl, sq) = _host.GetLoadGameFilesParams();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningFuzzyMatchingSettings") ?? "Opening fuzzy matching settings...");
            _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
            await Task.Yield();
            await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method SetFuzzyMatchingThresholdClickAsync");
        }
    }

    // ---- Support / Donate / About / Exit ----

    public void HandleSupport()
    {
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningSupportWindow") ?? "Opening support window...");
        _playSoundEffects.PlayNotificationSound();

        var supportRequestWindow = _serviceProvider.GetRequiredService<SupportWindow>();
        supportRequestWindow.Owner = Application.Current.MainWindow;
        supportRequestWindow.ShowDialog();
    }

    public async Task HandleDonate()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningDonationPage") ?? "Opening donation page...");

            var psi = new ProcessStartInfo
            {
                FileName = _configuration.GetValue<string>("Urls:DonationPage") ?? "https://www.purelogiccode.com/Donate/",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            const string contextMessage = "Unable to open the Donation Link from the menu.";
            _logErrors.LogAndForget(ex, contextMessage);
            await _messageBoxLibrary.ErrorOpeningDonationLinkMessageBox();
        }
    }

    public void HandleAbout()
    {
        _playSoundEffects.PlayNotificationSound();
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningAboutWindow") ?? "Opening About window...");

        var aboutWindow = _serviceProvider.GetRequiredService<AboutWindow>();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }

    public void HandleExit()
    {
        _playSoundEffects.PlayNotificationSound();
        if (Application.Current.MainWindow != null) Application.Current.MainWindow.Close();
    }

    // ---- Show Games Settings ----

    public async Task HandleShowGames(string showGamesMode)
    {
        try
        {
            _host.CancelAndRecreateToken();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilityFilter") ?? "Applying game visibility filter...");

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.ShowGames = showGamesMode;
                await _settings.SaveAsync();
                _menuCheckMarkService.UpdateShowGamesCheckMarks(showGamesMode);

                var (sl, sq) = _host.GetLoadGameFilesParams();
                _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ApplyingGameVisibilityFilter") ?? "Applying game visibility filter...");
                await Task.Yield();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, $"Error in the method ShowGames ({showGamesMode}).");
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error in the method ShowGames ({showGamesMode}).");
        }
    }

    // ---- Button Size ----

    public async Task HandleButtonSize(int newSize)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _host.SetGameButtonImageHeight(newSize);
                _settings.ThumbnailSize = newSize;
                await _settings.SaveAsync();

                _menuCheckMarkService.UpdateThumbnailSizeCheckMarks(newSize);
                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("AdjustingButtonSize") ?? "Adjusting button size...");

                var (sl, sq) = _host.GetLoadGameFilesParams();
                _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                await Task.Yield();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Error in method ButtonSizeClickAsync.";
                _logErrors.LogAndForget(ex, errorMessage);
                await _messageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ButtonSizeClickAsync.");
        }
    }

    // ---- Button Aspect Ratio ----

    public async Task HandleButtonAspectRatio(string aspectRatio)
    {
        try
        {
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("AdjustingButtonAspectRatio") ?? "Adjusting button aspect ratio...");
            _host.CancelAndRecreateToken();

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.ButtonAspectRatio = aspectRatio;
                await _settings.SaveAsync();

                _menuCheckMarkService.UpdateButtonAspectRatioCheckMarks(aspectRatio);

                var (sl, sq) = _host.GetLoadGameFilesParams();
                _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                await Task.Yield();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            catch (Exception ex)
            {
                const string contextMessage = "Error in method ButtonAspectRatioClickAsync";
                _logErrors.LogAndForget(ex, contextMessage);
                await _messageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ButtonAspectRatioClickAsync.");
        }
    }

    // ---- Games Per Page ----

    public async Task HandleGamesPerPage(int newPage)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                if (newPage is 1000 or 10000 or 1000000)
                {
                    if (await _messageBoxLibrary.WarnUserAboutMemoryConsumptionMessageBox() == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                _playSoundEffects.PlayNotificationSound();

                _host.SetFilesPerPage(newPage);
                _host.SetPaginationThreshold(newPage);
                _settings.GamesPerPage = newPage;
                await _settings.SaveAsync();

                _menuCheckMarkService.UpdateNumberOfGamesPerPageCheckMarks(newPage);
                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("AdjustingGamesPerPage") ?? "Adjusting games per page...");

                var (sl, sq) = _host.GetLoadGameFilesParams();
                _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                await Task.Yield();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in the method GamesPerPageClickAsync.");
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method GamesPerPageClickAsync.");
        }
    }

    // ---- Navigation: Global Search ----

    public void HandleShowGlobalSearch()
    {
        _playSoundEffects.PlayNotificationSound();
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGlobalSearch") ?? "Opening Global Search...");

        if (Application.Current.MainWindow is not MainWindow mainWindow) return;

        var contextMenuFunctions = _serviceProvider.GetRequiredService<IContextMenuFunctions>();
        var contextMenuService = _serviceProvider.GetRequiredService<IContextMenuService>();
        var globalSearchPage = new Pages.GlobalSearchPage(
            _host.GetSystemManagers(), _host.GetMachines(), _host.GetMameLookup(),
            _favoritesManager, _settings, mainWindow,
            _gamePadController, _gameLauncher, _playSoundEffects,
            _logErrors, _configuration, _getListOfFiles, _findCoverImage, _imageLoader, contextMenuFunctions, _debugLogger, contextMenuService);

        _host.NavigateToPage(globalSearchPage);
    }

    // ---- Navigation: Global Stats ----

    public void HandleShowGlobalStats()
    {
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGlobalStatistics") ?? "Opening Global Statistics...");
        _playSoundEffects.PlayNotificationSound();

        var globalStatsWindow = _serviceProvider.GetRequiredService<GlobalStatsWindow>();
        globalStatsWindow.Owner = Application.Current.MainWindow;
        globalStatsWindow.Initialize(_host.GetSystemManagers());
        globalStatsWindow.Show();
    }

    // ---- Navigation: Favorites ----

    public void HandleShowFavorites()
    {
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningFavorites") ?? "Opening Favorites...");
        _playSoundEffects.PlayNotificationSound();

        var contextMenuFunctions = _serviceProvider.GetRequiredService<IContextMenuFunctions>();
        var contextMenuService = _serviceProvider.GetRequiredService<IContextMenuService>();
        var favoritesPage = new Pages.FavoritesPage(
            _settings, _host.GetSystemManagers(), _host.GetMachines(), _favoritesManager,
            (MainWindow)Application.Current.MainWindow, _gamePadController, _gameLauncher, _playSoundEffects, _configuration, _logErrors, _findCoverImage, _imageLoader, contextMenuFunctions, _debugLogger, contextMenuService);

        _host.NavigateToPage(favoritesPage);
    }

    // ---- Navigation: Play History ----

    public void HandleShowPlayHistory()
    {
        _playSoundEffects.PlayNotificationSound();
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningPlayHistory") ?? "Opening Play History...");

        if (Application.Current.MainWindow is not MainWindow mainWindow) return;

        var contextMenuFunctions = _serviceProvider.GetRequiredService<IContextMenuFunctions>();
        var contextMenuService = _serviceProvider.GetRequiredService<IContextMenuService>();
        var playHistoryPage = new Pages.PlayHistoryPage(
            _host.GetSystemManagers(), _host.GetMachines(), _settings,
            _favoritesManager, _playHistoryManager, mainWindow,
            _gamePadController, _gameLauncher, _playSoundEffects, _configuration, _logErrors, _findCoverImage, _imageLoader, contextMenuFunctions, _debugLogger, contextMenuService);

        _host.NavigateToPage(playHistoryPage);
    }

    // ---- Navigation: Retro Achievements ----

    public void HandleShowRetroAchievements()
    {
        _playSoundEffects.PlayNotificationSound();
        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...");

        var retroAchievementsWindow = _serviceProvider.GetRequiredService<RetroAchievementsWindow>();
        retroAchievementsWindow.Owner = Application.Current.MainWindow;
        retroAchievementsWindow.Show();
    }

    // ---- Navigation: Restart ----

    public void HandleRestart()
    {
        _playSoundEffects.PlayNotificationSound();
        _host.NavigateBackToMainContent();
        _ = _host.ResetUiAsync();
    }

    // ---- System Favorites ----

    public async Task HandleShowSystemFavorites()
    {
        try
        {
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingFavoriteGamesForSystem") ?? "Loading favorite games for system...");
            _playSoundEffects.PlayNotificationSound();
            await _host.ShowSystemFavoriteGamesAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method NavSelectedSystemFavoriteButtonClickAsync.");
        }
    }

    // ---- Random / Feeling Lucky ----

    public async Task HandleFeelingLucky()
    {
        try
        {
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("PickingARandomGame") ?? "Picking a random game...");
            _playSoundEffects.PlayNotificationSound();
            await _host.ShowSystemFeelingLuckyAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavRandomLuckGameButtonClickAsync.");
        }
    }

    // ---- Retro Achievements Filter ----

    public async Task HandleShowGamesWithRetroAchievements()
    {
        try
        {
            if (_host.IsLoadingGames)
            {
                _host.CancelAndRecreateToken();
            }

            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FilteringRetroAchievements") ?? "Filtering games with achievements...");

            _host.DeselectTopLetterNumberMenu();
            _host.SetSearchTextBoxText("");
            _host.SetCurrentFilter(null);
            _host.SetActiveSearchQueryOrMode("RETRO_ACHIEVEMENTS");

            await _host.LoadGameFilesAsync(null, "RETRO_ACHIEVEMENTS", _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavShowGamesWithRetroAchievementsButtonClickAsync.");
        }
    }

    // ---- Zoom ----

    private const int MaxThumbnailSizeForSystem = 150;
    private const int MaxThumbnailSize = 800;
    private const int MinThumbnailSize = 50;
    private const int ZoomStep = 50;

    public async Task HandleZoomIn()
    {
        try
        {
            _host.CancelAndRecreateToken();
            _playSoundEffects.PlayNotificationSound();

            var isSystemSelectionMode = !_host.IsTopSystemSelectionVisible();

            if (isSystemSelectionMode)
            {
                var newSize = Math.Min(MaxThumbnailSizeForSystem, _settings.ThumbnailSizeForSystem + ZoomStep);
                if (newSize != _settings.ThumbnailSizeForSystem)
                {
                    _settings.ThumbnailSizeForSystem = newSize;
                    await _settings.SaveAsync();
                    _menuCheckMarkService.UpdateThumbnailSizeCheckMarks(newSize);
                }

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ZoomingIn") ?? "Zooming in...");
                var (sl, sq) = _host.GetLoadGameFilesParams();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            else
            {
                var newSize = Math.Min(MaxThumbnailSize, _settings.ThumbnailSize + ZoomStep);
                if (newSize != _settings.ThumbnailSize)
                {
                    _settings.ThumbnailSize = newSize;
                    _host.SetGameButtonImageHeight(newSize);
                    await _settings.SaveAsync();
                    _menuCheckMarkService.UpdateThumbnailSizeCheckMarks(newSize);
                }

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ZoomingIn") ?? "Zooming in...");
                var (sl, sq) = _host.GetLoadGameFilesParams();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method HandleZoomIn.");
        }
    }

    public async Task HandleZoomOut()
    {
        try
        {
            _host.CancelAndRecreateToken();
            _playSoundEffects.PlayNotificationSound();

            var isSystemSelectionMode = !_host.IsTopSystemSelectionVisible();

            if (isSystemSelectionMode)
            {
                var newSize = Math.Max(MinThumbnailSize, _settings.ThumbnailSizeForSystem - ZoomStep);
                if (newSize != _settings.ThumbnailSizeForSystem)
                {
                    _settings.ThumbnailSizeForSystem = newSize;
                    await _settings.SaveAsync();
                    _menuCheckMarkService.UpdateThumbnailSizeCheckMarks(newSize);
                }

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ZoomingOut") ?? "Zooming out...");
                var (sl, sq) = _host.GetLoadGameFilesParams();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            else
            {
                var newSize = Math.Max(MinThumbnailSize, _settings.ThumbnailSize - ZoomStep);
                if (newSize != _settings.ThumbnailSize)
                {
                    _settings.ThumbnailSize = newSize;
                    _host.SetGameButtonImageHeight(newSize);
                    await _settings.SaveAsync();
                    _menuCheckMarkService.UpdateThumbnailSizeCheckMarks(newSize);
                }

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ZoomingOut") ?? "Zooming out...");
                var (sl, sq) = _host.GetLoadGameFilesParams();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method HandleZoomOut.");
        }
    }

    // ---- View Mode ----

    public async Task HandleToggleViewMode()
    {
        try
        {
            _host.CancelAndRecreateToken();

            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingViewMode") ?? "Toggling view mode...");
            _playSoundEffects.PlayNotificationSound();

            if (_host.GetViewMode() == "GridView")
            {
                _host.SetGridViewChecked(false);
                _host.SetListViewChecked(true);
                _host.SetGameFileGridVisible(false);
                _host.SetListViewPreviewAreaVisible(true);
                _settings.ViewMode = "ListView";
            }
            else
            {
                _host.SetGridViewChecked(true);
                _host.SetListViewChecked(false);
                _host.SetGameFileGridVisible(true);
                _host.SetListViewPreviewAreaVisible(false);
                _settings.ViewMode = "GridView";
            }

            await _settings.SaveAsync();

            var (sl, sq) = _host.GetLoadGameFilesParams();
            await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavToggleViewModeClickAsync.");
        }
    }

    public async Task HandleChangeViewMode(object sender)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            switch (sender)
            {
                case MenuItem mi when mi.Name == _host.GridViewMenuItemId:
                    _host.SetGridViewChecked(true);
                    _host.SetListViewChecked(false);
                    _settings.ViewMode = "GridView";

                    _host.SetGameFileGridVisible(true);
                    _host.SetListViewPreviewAreaVisible(false);

                    _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingViewMode") ?? "Changing view mode...");
                    break;
                case MenuItem mi2 when mi2.Name == _host.ListViewMenuItemId:
                    _host.SetGridViewChecked(false);
                    _host.SetListViewChecked(true);
                    _settings.ViewMode = "ListView";

                    _host.SetGameFileGridVisible(false);
                    _host.SetListViewPreviewAreaVisible(true);

                    _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingViewMode") ?? "Changing view mode...");
                    break;
            }

            await _settings.SaveAsync();
        }
        catch (Exception ex)
        {
            const string errorMessage = "Error while using the method ChangeViewMode_Click.";
            _logErrors.LogAndForget(ex, errorMessage);
            await _messageBoxLibrary.ErrorChangingViewModeMessageBox();
        }
    }

    // ---- Filename Display Mode ----

    public async Task HandleFilenameDisplayMode(string mode)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.FilenameDisplayMode = mode;
                await _settings.SaveAsync();

                _menuCheckMarkService.UpdateFilenameDisplayModeCheckMarks(mode);

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingFilenameDisplayMode") ?? "Changing filename display mode...");

                if (_host.GetViewMode() == "GridView")
                {
                    var (sl, sq) = _host.GetLoadGameFilesParams();
                    _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                    await Task.Yield();
                    await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in method FilenameDisplayMode_Click.");
                await _messageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method FilenameDisplayMode_Click.");
        }
    }

    // ---- Display Machine Name ----

    public async Task HandleDisplayMachineName(bool isChecked)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.DisplayMachineName = isChecked;
                await _settings.SaveAsync();

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingDisplayMachineName") ?? "Changing machine name display...");

                if (_host.GetViewMode() == "GridView")
                {
                    var (sl, sq) = _host.GetLoadGameFilesParams();
                    _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                    await Task.Yield();
                    await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in method DisplayMachineName_Click.");
                await _messageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method DisplayMachineName_Click.");
        }
    }

    // ---- Filename Font Size ----

    public async Task HandleFilenameFontSize(string size)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.FilenameFontSize = size;
                await _settings.SaveAsync();

                _menuCheckMarkService.UpdateFilenameFontSizeCheckMarks(size);

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingFilenameFontSize") ?? "Changing filename font size...");

                if (_host.GetViewMode() == "GridView")
                {
                    var (sl, sq) = _host.GetLoadGameFilesParams();
                    _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                    await Task.Yield();
                    await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in method FilenameFontSize_Click.");
                await _messageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method FilenameFontSize_Click.");
        }
    }

    // ---- Machine Name Font Size ----

    public async Task HandleMachineNameFontSize(string size)
    {
        try
        {
            _host.CancelAndRecreateToken();

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.MachineNameFontSize = size;
                await _settings.SaveAsync();

                _menuCheckMarkService.UpdateMachineNameFontSizeCheckMarks(size);

                _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingMachineNameFontSize") ?? "Changing machine name font size...");

                if (_host.GetViewMode() == "GridView")
                {
                    var (sl, sq) = _host.GetLoadGameFilesParams();
                    _host.SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
                    await Task.Yield();
                    await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in method MachineNameFontSize_Click.");
                await _messageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method MachineNameFontSize_Click.");
        }
    }

    // ---- Sound Configuration ----

    public async Task HandleSoundConfiguration()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningSoundConfigurationSettings") ?? "Opening Sound Configuration settings...");

            var soundConfigWindow = _serviceProvider.GetRequiredService<SoundConfigurationWindow>();
            soundConfigWindow.Owner = Application.Current.MainWindow;
            soundConfigWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error opening Sound Configuration window.");
            await _messageBoxLibrary.CouldNotOpenSoundConfigurationWindowMessageBox();
        }
    }

    // ---- RetroAchievements Settings ----

    public async Task HandleShowRetroAchievementsSettings()
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievementsSettings") ?? "Opening RetroAchievements settings...");

            var raSettingsWindow = _serviceProvider.GetRequiredService<RetroAchievementsSettingsWindow>();
            raSettingsWindow.Owner = Application.Current.MainWindow;
            raSettingsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error opening RetroAchievements settings window.");
            await _messageBoxLibrary.ErrorMessageBox();
        }
    }

    // ---- Overlay Button Toggles ----

    public async Task HandleToggleRetroAchievementButton(bool isChecked)
    {
        _host.CancelAndRecreateToken();

        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingRetroAchievementsOverlayButton") ?? "Toggling RetroAchievements overlay button...");
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.OverlayRetroAchievementButton = isChecked;
            await _settings.SaveAsync();

            var (sl, sq) = _host.GetLoadGameFilesParams();
            _ = _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error toggling RetroAchievements overlay button.");
            await _messageBoxLibrary.ErrorMessageBox();
        }
    }

    public async Task HandleToggleVideoLinkButton(bool isChecked)
    {
        _host.CancelAndRecreateToken();

        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingVideoLinkOverlayButton") ?? "Toggling video link overlay button...");
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.OverlayOpenVideoButton = isChecked;
            await _settings.SaveAsync();

            var (sl, sq) = _host.GetLoadGameFilesParams();
            _ = _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error toggling video link overlay button.");
            await _messageBoxLibrary.ErrorMessageBox();
        }
    }

    public async Task HandleToggleInfoLinkButton(bool isChecked)
    {
        _host.CancelAndRecreateToken();

        _updateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingInfoLinkOverlayButton") ?? "Toggling info link overlay button...");
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.OverlayOpenInfoButton = isChecked;
            await _settings.SaveAsync();

            var (sl, sq) = _host.GetLoadGameFilesParams();
            _ = _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error toggling info link overlay button.");
            await _messageBoxLibrary.ErrorMessageBox();
        }
    }

    // ---- Language ----

    public void HandleChangeLanguage(string languageCode)
    {
        _host.ChangeLanguage(languageCode);
    }

    // ---- Top Letter/Number Menu ----

    public async Task HandleTopLetterNumberMenuClick(string selectedLetter)
    {
        try
        {
            if (_host.IsLoadingGames)
            {
                _host.CancelAndRecreateToken();
            }

            _playSoundEffects.PlayNotificationSound();

            _host.ResetPaginationButtons();
            _host.SetSearchTextBoxText("");
            _host.SetCurrentFilter(selectedLetter);
            _host.SetActiveSearchQueryOrMode(null);

            _host.SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingGames") ?? "Loading Games...");
            await Task.Yield();

            await _host.LoadGameFilesAsync(selectedLetter, null, _host.CurrentCancellationToken);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in TopLetterNumberMenuClickAsync.");
        }
    }

    // ---- Sort Order Toggle ----

    public async Task HandleSortOrderToggle()
    {
        try
        {
            if (_host.IsLoadingGames)
            {
                return;
            }

            _host.CancelAndRecreateToken();

            _playSoundEffects.PlayNotificationSound();
            var currentSort = _host.GetMameSortOrder();
            var newSort = currentSort == "FileName" ? "MachineDescription" : "FileName";
            _host.SetMameSortOrder(newSort);
            _host.UpdateSortOrderButtonUi();

            _host.SetIsResortOperation(true);
            try
            {
                var (sl, sq) = _host.GetLoadGameFilesParams();
                await _host.LoadGameFilesAsync(sl, sq, _host.CurrentCancellationToken);
            }
            finally
            {
                _host.SetIsResortOperation(false);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in SortOrderToggleButtonClickAsync.");
            _debugLogger.Log("Error in SortOrderToggleButtonClickAsync.");
        }
    }
}