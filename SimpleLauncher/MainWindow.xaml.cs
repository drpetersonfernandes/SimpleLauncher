using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameItemRender;
using SimpleLauncher.Services.GameFileWatcher;
using SimpleLauncher.Services.GameListUI;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.GameScan;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.LaunchTools;
using SimpleLauncher.Services.MameData;
using SimpleLauncher.Services.MenuActionHandler;
using SimpleLauncher.Services.MenuCheckMark;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SearchOrchestrator;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.StartupInitialization;
using SimpleLauncher.Services.SystemConfiguration;
using SimpleLauncher.Services.ThemeMenu;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.UiHelpers;
using SimpleLauncher.Services.UIReset;
using SimpleLauncher.Services.UiOrchestrator;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.Services.UsageStats;
using SimpleLauncher.Services.GameFileLoadingOrchestrator;
using SimpleLauncher.Services.SystemSelectionOrchestrator;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using UpdateChecker = SimpleLauncher.Services.CheckForUpdates.UpdateChecker;

namespace SimpleLauncher;

using ILoadingState = Services.LoadingInterface.ILoadingState;

public partial class MainWindow : INotifyPropertyChanged, IDisposable, ILoadingState, IMenuCheckMarkHost, IUiResetHost, IUiOrchestratorHost, IStartupInitializationHost, IThemeMenuHost, ILanguageMenuHost, IStatusBarHost
{
    private CancellationTokenSource _cancellationSource = new();
    private bool _isResortOperation;
    private bool _wasControllerRunningBeforeDeactivation;

    private bool _isDisposed;
    internal DispatcherTimer StatusBarTimer { get; set; }
    public ObservableCollection<GameListViewItem> GameListItems { get; } = [];

    // Event handler references for proper unsubscription to prevent memory leaks
    private RoutedEventHandler _emergencyButtonClickHandler;
    private readonly RoutedEventHandler _asyncLoadedHandler;

    public event PropertyChangedEventHandler PropertyChanged;
    private string _selectedSystem;

    public string SelectedSystem
    {
        get => _selectedSystem;
        set
        {
            _selectedSystem = value;
            OnPropertyChanged(nameof(SelectedSystem));
        }
    }

    public string PlayTime
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(PlayTime));
        }
    }

    public bool IsPlayTimeVisible
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged(nameof(IsPlayTimeVisible));
        }
    } = true;

    private bool _isLoadingGames;

    public bool IsLoadingGames
    {
        get => _isLoadingGames;
        private set
        {
            _isLoadingGames = value;
            OnPropertyChanged(nameof(IsLoadingGames));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Pagination
    internal Button NextPageButton2;
    internal Button PrevPageButton2;

    internal TrayIconManager TrayIconManager;
    internal PlayHistoryManager PlayHistoryManager { get; }
    private List<SystemManager> _systemManagers;
    private readonly FilterMenu _topLetterNumberMenu;
    private readonly WrapPanel _gameFileGrid;
    private readonly SettingsManager _settings;
    private readonly IGameItemRenderService _gameItemRenderService;
    private readonly IMameDataService _mameDataService;
    private string _selectedImageFolder;
    private List<string> _selectedRomFolders;

    // Game cache and filtering are now handled by dedicated services
    private readonly Services.GameCache.IGameCacheService _gameCacheService;

    private readonly UpdateChecker _updateChecker;
    private readonly GamePadController _gamePadController;
    private readonly ILaunchTools _launchTools;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Stats _stats;
    private readonly ILogErrors _logErrors;
    private readonly GameScannerService _gameScannerService;
    private readonly ThemeMenuService _themeMenuService;
    private readonly LanguageMenuService _languageMenuService;
    private readonly StartupInitializationService _startupInitializationService;
    private readonly GameFileWatcherService _gameFileWatcherService;
    private readonly ISearchOrchestratorService _searchOrchestratorService;
    internal readonly MenuActionHandlerService MenuActionHandlerService;
    internal readonly IUpdateStatusBar UpdateStatusBarService;
    internal readonly IMenuCheckMarkService MenuCheckMarkService;
    internal readonly IUiResetService UiResetService;
    internal readonly ISystemConfigurationService SystemConfigurationService;
    internal readonly IUiOrchestrator UiOrchestrator;
    private readonly ISystemSelectionOrchestrator _systemSelectionOrchestrator;
    private readonly IGameFileLoadingOrchestrator _gameFileLoadingOrchestrator;

    public MainWindow(
        SettingsManager settings,
        PlayHistoryManager playHistoryManager,
        UpdateChecker updateChecker,
        GamePadController gamePadController,
        PlaySoundEffects playSoundEffects,
        ILaunchTools launchTools,
        Stats stats,
        ILogErrors logErrors,
        GameScannerService gameScannerService,
        ThemeMenuService themeMenuService,
        LanguageMenuService languageMenuService,
        StartupInitializationService startupInitializationService,
        GameFileWatcherService gameFileWatcherService,
        MenuActionHandlerService menuActionHandlerService,
        IUpdateStatusBar updateStatusBarService,
        IMenuCheckMarkService menuCheckMarkService,
        IUiResetService uiResetService,
        ISystemConfigurationService systemConfigurationService,
        Services.GameCache.IGameCacheService gameCacheService,
        IMameDataService mameDataService,
        ISearchOrchestratorService searchOrchestratorService,
        IUiOrchestrator uiOrchestrator,
        IGameItemRenderService gameItemRenderService,
        ISystemSelectionOrchestrator systemSelectionOrchestrator,
        IGameFileLoadingOrchestrator gameFileLoadingOrchestrator)
    {
        InitializeComponent();

        _gamePadController = gamePadController;
        _updateChecker = updateChecker;
        _settings = settings;
        PlayHistoryManager = playHistoryManager;
        _playSoundEffects = playSoundEffects;
        _launchTools = launchTools;
        _gameScannerService = gameScannerService;
        _themeMenuService = themeMenuService;
        _languageMenuService = languageMenuService;
        _startupInitializationService = startupInitializationService;
        _gameFileWatcherService = gameFileWatcherService;
        MenuActionHandlerService = menuActionHandlerService;
        UpdateStatusBarService = updateStatusBarService;
        _stats = stats;
        _logErrors = logErrors;

        MenuCheckMarkService = menuCheckMarkService;
        UiResetService = uiResetService;
        SystemConfigurationService = systemConfigurationService;
        _gameCacheService = gameCacheService;
        _mameDataService = mameDataService;
        _searchOrchestratorService = searchOrchestratorService;
        UiOrchestrator = uiOrchestrator;
        _gameItemRenderService = gameItemRenderService;
        _systemSelectionOrchestrator = systemSelectionOrchestrator;
        _gameFileLoadingOrchestrator = gameFileLoadingOrchestrator;

        UiOrchestrator.Initialize(this);
        _systemSelectionOrchestrator.Initialize(this);
        _gameFileLoadingOrchestrator.Initialize(this);
        _themeMenuService.Initialize((IThemeMenuHost)this);
        _languageMenuService.Initialize((ILanguageMenuHost)this);
        MenuActionHandlerService.Initialize(this);
        MenuCheckMarkService.Initialize(this);
        UiResetService.Initialize(this);
        UpdateStatusBarService.Initialize((IStatusBarHost)this);

        DataContext = this;

        // Load and Apply _settings
        ToggleGamepad.IsChecked = _settings.EnableGamePadNavigation;
        MenuCheckMarkService.UpdateThumbnailSizeCheckMarks(_settings.ThumbnailSize);
        MenuCheckMarkService.UpdateButtonAspectRatioCheckMarks(_settings.ButtonAspectRatio);
        MenuCheckMarkService.UpdateNumberOfGamesPerPageCheckMarks(_settings.GamesPerPage);
        MenuCheckMarkService.UpdateShowGamesCheckMarks(_settings.ShowGames);
        MenuCheckMarkService.UpdateFilenameDisplayModeCheckMarks(_settings.FilenameDisplayMode);
        MenuCheckMarkService.UpdateFilenameFontSizeCheckMarks(_settings.FilenameFontSize);
        MenuCheckMarkService.UpdateMachineNameFontSizeCheckMarks(_settings.MachineNameFontSize);
        UiOrchestrator.PaginationFilesPerPage = _settings.GamesPerPage;
        UiOrchestrator.PaginationThreshold = _settings.GamesPerPage;
        ToggleFuzzyMatching.IsChecked = _settings.EnableFuzzyMatching;

        // Initialize _gameFileGrid before LoadOrReloadSystemManager uses it
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;
        if (_gameFileGrid == null)
        {
            _logErrors.LogAndForget(null, "GameFileGrid not found");
            throw new InvalidOperationException("GameFileGrid not found");
        }

        _gameItemRenderService.Initialize(this);
        _systemSelectionOrchestrator.LoadOrReloadSystemManager();

        _topLetterNumberMenu = new FilterMenu(_playSoundEffects);

        // Add _topLetterNumberMenu to the UI
        LetterNumberMenu.Children.Clear();
        LetterNumberMenu.Children.Add(_topLetterNumberMenu.LetterPanel);

        // Create and integrate FilterMenu
        _topLetterNumberMenu.OnLetterSelected += TopLetterNumberMenu_OnLetterSelected;

        // Migrate old play history records to full paths
        PlayHistoryManager.MigrateFilenamesToFullPaths(_systemManagers);

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        Activated += MainWindow_Activated;
        Deactivated += MainWindow_Deactivated;

        // Wire up game file watcher to detect external file changes
        _gameFileWatcherService.GameFilesChanged += _gameFileLoadingOrchestrator.OnGameFilesChanged;

        // Store the async Loaded handler reference so it can be unsubscribed later
        _asyncLoadedHandler = async void (_, _) =>
        {
            try
            {
                // Wire Emergency Return Button from Template
                LoadingOverlay.ApplyTemplate();
                if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
                {
                    _emergencyButtonClickHandler = EmergencyOverlayRelease_Click;
                    emergencyBtn.Click += _emergencyButtonClickHandler;
                }

                await HandleLoadedAsync();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in the HandleLoadedAsync method.");
            }
        };
        Loaded += _asyncLoadedHandler;
    }

    private async Task HandleLoadedAsync()
    {
        try
        {
            await _systemSelectionOrchestrator.DisplaySystemSelectionScreenAsync(((IMenuActionHost)this).CurrentCancellationToken);
            DebugLogger.Log("DisplaySystemSelectionScreenAsync called.");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the DisplaySystemSelectionScreenAsync method.");
            DebugLogger.Log($"Error in the DisplaySystemSelectionScreenAsync method: {ex.Message}");
        }

        try
        {
            await _updateChecker.SilentCheckForUpdatesAsync(this);
            DebugLogger.Log("Silent check for updates was done.");
            await _stats.CallApiAsync();
            DebugLogger.Log("Stats API call was done.");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the Loaded event.");
            DebugLogger.Log($"Error in the Loaded event: {ex.Message}");
        }

        try
        {
            // --- First-run experience: Check if system.xml is empty ---
            if (_systemManagers == null || _systemManagers.Count == 0)
            {
                // This is the first run. Let's scan for Windows games automatically.
                SetLoadingState(true, (string)Application.Current.TryFindResource("ScanningForWindowsGames") ?? "Scanning for Windows games...");
                try
                {
                    await _gameScannerService.ScanForStoreGamesAsync();
                    if (_gameScannerService.WasNewSystemCreated)
                    {
                        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("FoundNewMicrosoftWindowsGames") ?? "Found new Microsoft Windows games. Refreshing system list.");

                        // Reload to get the new system
                        _systemSelectionOrchestrator.LoadOrReloadSystemManager();

                        // After reloading, the system selection screen needs to be updated.
                        await _systemSelectionOrchestrator.DisplaySystemSelectionScreenAsync(((IMenuActionHost)this).CurrentCancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logErrors.LogAndForget(ex, "Error during initial Windows games scan.");
                }
                finally
                {
                    SetLoadingState(false);
                }

                // After the scan, check again if any systems exist.
                // If still no systems, show the Easy Mode prompt.
                if (_systemManagers == null || _systemManagers.Count == 0)
                {
                    var result = MessageBoxLibrary.FirstRunWelcomeMessageBox();
                    if (result == MessageBoxResult.Yes)
                    {
                        var easyModeWindow = App.ServiceProvider.GetRequiredService<EasyModeWindow>();
                        easyModeWindow.Owner = this;
                        easyModeWindow.ShowDialog();

                        _systemSelectionOrchestrator.LoadOrReloadSystemManager();
                        await _systemSelectionOrchestrator.DisplaySystemSelectionScreenAsync(((IMenuActionHost)this).CurrentCancellationToken); // Await this now
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the Loaded event's first-run logic.");
            DebugLogger.Log($"Error in the Loaded event's first-run logic: {ex.Message}");
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _startupInitializationService.Initialize((IStartupInitializationHost)this);
    }

    private void MainWindow_Activated(object sender, EventArgs e)
    {
        if (_wasControllerRunningBeforeDeactivation)
        {
            _gamePadController.Start();
            DebugLogger.Log("Gamepad controller restarted on window activation.");
        }

        _wasControllerRunningBeforeDeactivation = false; // Reset flag
    }

    private void MainWindow_Deactivated(object sender, EventArgs e)
    {
        if (_gamePadController.IsRunning)
        {
            _wasControllerRunningBeforeDeactivation = true;
            _gamePadController.Stop();
            DebugLogger.Log("Gamepad controller temporarily stopped on window deactivation.");
        }
        else
        {
            _wasControllerRunningBeforeDeactivation = false;
        }
    }

    private async void TopLetterNumberMenu_OnLetterSelected(string selectedLetter)
    {
        try
        {
            await TopLetterNumberMenuClickAsync(selectedLetter);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method TopLetterNumberMenu_OnLetterSelected");
        }
    }

    private async void SystemComboBoxSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            await _systemSelectionOrchestrator.SystemComboBoxSelectionChangedAsync(_cancellationSource.Token);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in SystemComboBoxSelectionChangedAsync.");
        }
    }

    internal void CancelAndRecreateToken()
    {
        // Atomically exchange the old CancellationTokenSource with a new one
        // This prevents race conditions when multiple threads try to recreate the token
        var oldCts = Interlocked.Exchange(ref _cancellationSource, new CancellationTokenSource());

        // Dispose the old instance after the exchange is complete
        try
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Token was already disposed, ignore
        }
    }

    /// <summary>
    /// Navigates to a Page within the MainWindow, hiding the main game content.
    /// </summary>
    internal void NavigateToPage(Page page)
    {
        // Hide main game content and show the page frame
        MainGameContent.Visibility = Visibility.Collapsed;
        PageContentFrame.Visibility = Visibility.Visible;

        // Navigate to the page
        PageContentFrame.Content = page;
    }

    /// <summary>
    /// Returns to the main game content from a page.
    /// </summary>
    internal void NavigateBackToMainContent()
    {
        // Clear the frame content
        PageContentFrame.Content = null;

        // Show main game content and hide the page frame
        MainGameContent.Visibility = Visibility.Visible;
        PageContentFrame.Visibility = Visibility.Collapsed;

        // Refresh the game list to ensure it's up to date
        _playSoundEffects.PlayNotificationSound();
    }

    private (string startLetter, string searchQuery) GetLoadGameFilesParams()
    {
        var searchQueryToUse = ((IUiResetHost)this).ActiveSearchQueryOrMode;
        var startLetterToUse = string.IsNullOrEmpty(searchQueryToUse) ? ((IUiResetHost)this).CurrentFilter : null;
        return (startLetterToUse, searchQueryToUse);
    }

    private async void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        try
        {
            // Check if the Ctrl key is pressed
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            try
            {
                switch (e.Delta)
                {
                    case > 0:
                        await MenuActionHandlerService.HandleZoomIn();
                        break;
                    case < 0:
                        await MenuActionHandlerService.HandleZoomOut();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in the method MainWindow_MouseWheel.");
            }

            // Mark the event as handled to prevent scrolling the ScrollViewer
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method MainWindow_MouseWheel.");
        }
    }

    private async Task TopLetterNumberMenuClickAsync(string selectedLetter)
    {
        try
        {
            if (_isLoadingGames)
            {
                CancelAndRecreateToken();
            }

            _playSoundEffects.PlayNotificationSound();

            ResetPaginationButtons(); // Ensure pagination is reset at the beginning
            SearchTextBox.Text = ""; // Clear SearchTextBox
            ((IUiResetHost)this).CurrentFilter = selectedLetter; // Update current filter
            ((IUiResetHost)this).ActiveSearchQueryOrMode = null; // Reset special search mode

            // Show loading overlay immediately with proper message
            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingGames") ?? "Loading Games...");
            await Task.Yield(); // Allow UI to render the loading overlay

            await _gameFileLoadingOrchestrator.LoadGameFilesAsync(selectedLetter, null, _cancellationSource.Token); // searchQuery is null
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error in TopLetterNumberMenuClickAsync.");
        }
    }

    private async Task ShowSystemFavoriteGamesClickAsync()
    {
        try
        {
            if (_isLoadingGames)
            {
                CancelAndRecreateToken();
            }

            _playSoundEffects.PlayNotificationSound();

            // Change the filter to ShowAll (as favorites might not have covers)
            _settings.ShowGames = "ShowAll";
            await _settings.SaveAsync();
            ApplyShowGamesSetting();

            SearchTextBox.Text = "";
            ((IUiResetHost)this).CurrentFilter = null;
            ((IUiResetHost)this).ActiveSearchQueryOrMode = "FAVORITES";

            // Show loading overlay immediately with proper message
            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingFavoriteGamesForSystem") ?? "Loading favorite games for system...");
            await Task.Yield(); // Allow UI to render the loading overlay

            await _gameFileLoadingOrchestrator.LoadGameFilesAsync(null, "FAVORITES", _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error in ShowSystemFavoriteGamesClickAsync.");
        }
    }

    private async Task ShowSystemFeelingLuckyClickAsync()
    {
        try
        {
            CancelAndRecreateToken();

            _playSoundEffects.PlayNotificationSound();

            // Change the filter to ShowAll
            _settings.ShowGames = "ShowAll";
            await _settings.SaveAsync();
            ApplyShowGamesSetting();

            _topLetterNumberMenu.DeselectLetter();
            SearchTextBox.Text = "";
            ((IUiResetHost)this).CurrentFilter = null;
            ((IUiResetHost)this).ActiveSearchQueryOrMode = "RANDOM_SELECTION";

            // Show loading overlay immediately with proper message
            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingGames") ?? "Loading Games...");
            await Task.Yield(); // Allow UI to render the loading overlay

            await _gameFileLoadingOrchestrator.LoadGameFilesAsync(null, "RANDOM_SELECTION", _cancellationSource.Token);

            // If in list view, select the game in the DataGrid
            if (_settings.ViewMode != "ListView" || GameDataGrid.Items.Count <= 0) return;

            GameDataGrid.SelectedIndex = 0;
            GameDataGrid.ScrollIntoView(GameDataGrid.SelectedItem);
            GameDataGrid.Focus();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in ShowSystemFeelingLuckyClickAsync.");
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    internal void RefreshGameListAfterPlay(string fileName, string systemName)
    {
        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("RefreshingGameList") ?? "Refreshing game list...");
        try
        {
            // Only update if in ListView mode
            if (_settings.ViewMode != "ListView" || GameListItems.Count == 0)
                return;

            // Get the current playtime from history
            var historyItem = PlayHistoryManager.PlayHistoryList
                .FirstOrDefault(h => h.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) && h.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (historyItem == null)
            {
                return;
            }

            // Find and update the specific item
            var gameItem = GameListItems.FirstOrDefault(item =>
                item.FilePath.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (gameItem != null)
            {
                // Update in the UI thread to ensure UI refreshes
                Dispatcher.Invoke(() =>
                {
                    // Update playtime
                    var timeSpan = TimeSpan.FromSeconds(historyItem.TotalPlayTime);
                    gameItem.PlayTime = timeSpan.TotalHours >= 1
                        ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                        : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";

                    // Update times played
                    gameItem.TimesPlayed = historyItem.TimesPlayed.ToString(CultureInfo.InvariantCulture);

                    // Force refresh of DataGrid
                    GameDataGrid.Items.Refresh();
                });
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error refreshing game list play time";
            _logErrors.LogAndForget(ex, contextMessage);
        }
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        UiOrchestrator.SetLoadingState(isLoading, message);
    }

    internal void SetIsLoadingGamesInternal(bool value)
    {
        _isLoadingGames = value;
        IsLoadingGames = value;
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        UiOrchestrator.EmergencyRelease();
    }

    internal void SetPaginationButtonsDefault()
    {
        PrevPageButton2 = PrevPageButton;
        NextPageButton2 = NextPageButton;
        PrevPageButton2.IsEnabled = false;
        NextPageButton2.IsEnabled = false;
    }

    internal void SetPaginationButtonsVisibility(Visibility visibility)
    {
        PrevPageButton2.Visibility = visibility;
        NextPageButton2.Visibility = visibility;
    }

    internal void SetTrayIconManager(TrayIconManager manager)
    {
        TrayIconManager = manager;
    }

    private void SaveApplicationSettings()
    {
        // Save application's current state
        _settings.ThumbnailSize = _gameItemRenderService.ImageHeight;
        _settings.GamesPerPage = UiOrchestrator.PaginationFilesPerPage;
        _settings.EnableGamePadNavigation = ToggleGamepad.IsChecked;
        _settings.EnableFuzzyMatching = ToggleFuzzyMatching.IsChecked;

        // Note: Theme settings (BaseTheme and AccentColor) are already saved
        // when the user changes them via App.ChangeTheme().
        // We do NOT detect and overwrite them here because DetectTheme()
        // cannot properly identify custom themes like "Adaptive", "HighContrast",
        // or "Midnight" - it would incorrectly save them as "Dark" or "Light".

        _settings.SaveAsync();
    }

    private void GameListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GameDataGrid.SelectedItem is not GameListViewItem selectedItem)
        {
            PreviewImage.Source = null; // Clear preview if selection is cleared
            return;
        }

        // If the selected item is the "No results" message, its FilePath will be null.
        // In this case, just clear the preview and do nothing else.
        if (string.IsNullOrEmpty(selectedItem.FilePath))
        {
            PreviewImage.Source = null;
            return;
        }

        // If it's a real game item, proceed with loading the preview.
        _gameItemRenderService.HandleSelectionChangedAsync(selectedItem);
    }

    private async void GameListDoubleClickOnSelectedItemAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (GameDataGrid.SelectedItem is not GameListViewItem selectedItem)
            {
                return;
            }

            if (string.IsNullOrEmpty(selectedItem.FilePath))
            {
                // This is likely the "No results found" placeholder item.
                return;
            }

            // Delegate the double-click handling to the render service
            await _gameItemRenderService.HandleDoubleClickAsync(selectedItem);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error while using the method GameListDoubleClickOnSelectedItemAsync.";
            _logErrors.LogAndForget(ex, contextMessage);
        }
    }

    internal void SetGameButtonsEnabled(bool isEnabled)
    {
        UiOrchestrator.SetGameButtonsEnabled(isEnabled);
    }

    internal static void ClearGameButtonImages(Panel panel)
    {
        GameListUiService.ClearGameButtonImages(panel);
    }

    private Task SetUiBeforeLoadGameFilesAsync()
    {
        return UiOrchestrator.SetUiBeforeLoadGameFilesAsync();
    }

    private List<string> SetPaginationOfListOfFiles(List<string> allFiles)
    {
        return UiOrchestrator.ApplyPagination(allFiles);
    }

    private async void SortOrderToggleButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames)
            {
                return;
            }

            CancelAndRecreateToken();

            _playSoundEffects.PlayNotificationSound();
            ((IUiResetHost)this).MameSortOrder = ((IUiResetHost)this).MameSortOrder == "FileName" ? "MachineDescription" : "FileName";
            UpdateSortOrderButtonUi();

            _isResortOperation = true; // Set flag before loading
            try
            {
                var (sl, sq) = GetLoadGameFilesParams();
                await _gameFileLoadingOrchestrator.LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
            }
            finally
            {
                _isResortOperation = false; // Reset flag after loading
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in SortOrderToggleButtonClickAsync.");
            DebugLogger.Log("Error in SortOrderToggleButtonClickAsync.");
        }
    }

    private void UpdateSortOrderButtonUi()
    {
        if (SortOrderToggleButton == null)
        {
            return;
        }

        if (((IUiResetHost)this).MameSortOrder == "FileName")
        {
            var tooltipText = (string)Application.Current.TryFindResource("SortByMachineDescriptionTooltip") ?? "Sort by Machine Description";
            SortOrderToggleButton.ToolTip = tooltipText;
        }
        else
        {
            var tooltipText = (string)Application.Current.TryFindResource("SortByFileNameTooltip") ?? "Sort by File Name";
            SortOrderToggleButton.ToolTip = tooltipText;
        }
    }

    internal Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default)
    {
        return _gameFileLoadingOrchestrator.LoadGameFilesAsync(startLetter, searchQuery, cancellationToken);
    }

    internal Task InvalidateGameFileCachesAsync(CancellationToken cancellationToken = default)
    {
        return _gameFileLoadingOrchestrator.InvalidateGameFileCachesAsync(cancellationToken);
    }

    // IUiOrchestratorHost implementation
    ScrollViewer IUiOrchestratorHost.Scroller => Scroller;
    Image IUiOrchestratorHost.PreviewImage => PreviewImage;
    WrapPanel IUiOrchestratorHost.GameFileGrid => GameFileGrid;
    Grid IUiOrchestratorHost.ListViewPreviewArea => ListViewPreviewArea;
    Frame IUiOrchestratorHost.PageContentFrame => PageContentFrame;
    Grid IUiOrchestratorHost.MainGameContent => MainGameContent;
    Grid IUiOrchestratorHost.MainContentGrid => MainContentGrid;
    Label IUiOrchestratorHost.TotalFilesLabel => TotalFilesLabel;
    Button IUiOrchestratorHost.PrevPageButton2 => PrevPageButton2;
    Button IUiOrchestratorHost.NextPageButton2 => NextPageButton2;
    UIElement IUiOrchestratorHost.LoadingOverlay => LoadingOverlay;
    Button IUiOrchestratorHost.SortOrderToggleButton => SortOrderToggleButton;
    TextBox IUiOrchestratorHost.SearchTextBox => SearchTextBox;
    ComboBox IUiOrchestratorHost.SystemComboBox => SystemComboBox;
    ComboBox IUiOrchestratorHost.EmulatorComboBox => EmulatorComboBox;
    ObservableCollection<GameListViewItem> IUiOrchestratorHost.GameListItems => GameListItems;

    bool IUiOrchestratorHost.IsLoadingGames => _isLoadingGames;

    void IUiOrchestratorHost.SetIsLoadingGamesInternal(bool value)
    {
        SetIsLoadingGamesInternal(value);
    }

    void IUiOrchestratorHost.CancelAndRecreateToken()
    {
        CancelAndRecreateToken();
    }

    Task IUiOrchestratorHost.ResetUiAsync()
    {
        try
        {
            ResetUiAsync();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
}