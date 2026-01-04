using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ControlzEx.Theming;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.Services.GameScanLogic;
using SimpleLauncher.UiHelpers;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;

namespace SimpleLauncher;

public partial class MainWindow : INotifyPropertyChanged, IDisposable
{
    private bool _isUiUpdating;
    private bool _wasControllerRunningBeforeDeactivation;

    // DispatcherTimer for the status bar timer
    public DispatcherTimer StatusBarTimer { get; set; }

    // Declare GameListItems
    // Used in ListView Mode
    public ObservableCollection<GameListViewItem> GameListItems { get; set; } = [];

    // Declare System Name and PlayTime in the Statusbar
    // _selectedSystem is the selected system from ComboBox
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
        set
        {
            _isLoadingGames = value;
            OnPropertyChanged(nameof(IsLoadingGames));
        }
    }

    private void OnPropertyChanged(string propertyName) // Update UI on OnPropertyChanged
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Define Tray Icon
    private TrayIconManager _trayIconManager;

    // Define PlayHistory
    public PlayHistoryManager PlayHistoryManager { get; }

    // Define Pagination Related Variables
    private int _currentPage = 1;
    private int _filesPerPage;
    private int _totalFiles;
    private int _paginationThreshold;
    private Button _nextPageButton;
    private Button _prevPageButton;
    private string _currentFilter;

    // Define and Instantiate variables
    private List<SystemManager> _systemManagers;
    private readonly FilterMenu _topLetterNumberMenu;
    private GameListFactory _gameListFactory;
    private readonly WrapPanel _gameFileGrid;
    private GameButtonFactory _gameButtonFactory;
    private readonly SettingsManager _settings;
    private readonly FavoritesManager _favoritesManager;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private string _selectedImageFolder;
    private List<string> _selectedRomFolders;

    // Define the LogPath
    private readonly string _logPath = GetLogPath.Path();

    private string _activeSearchQueryOrMode;
    private string _mameSortOrder = "FileName";

    // All Games for Current System and Current Search
    private List<string> _allGamesForCurrentSystem = [];
    private List<string> _currentSearchResults = [];
    private readonly SemaphoreSlim _allGamesLock = new(1, 1); // Semaphore for _allGamesForCurrentSystem

    private readonly UpdateChecker _updateChecker;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly ILaunchTools _launchTools;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Stats _stats;
    private readonly ILogErrors _logErrors;
    private readonly GameScannerService _gameScannerService;

    public MainWindow(SettingsManager settings, FavoritesManager favoritesManager, PlayHistoryManager playHistoryManager, UpdateChecker updateChecker, GamePadController gamePadController, GameLauncher gameLauncher, PlaySoundEffects playSoundEffects, ILaunchTools launchTools, Stats stats, ILogErrors logErrors, GameScannerService gameScannerService)
    {
        InitializeComponent();

        _gamePadController = gamePadController ?? throw new ArgumentNullException(nameof(gamePadController));
        _updateChecker = updateChecker ?? throw new ArgumentNullException(nameof(updateChecker));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _favoritesManager = favoritesManager ?? throw new ArgumentNullException(nameof(favoritesManager));
        PlayHistoryManager = playHistoryManager ?? throw new ArgumentNullException(nameof(playHistoryManager));
        _gameLauncher = gameLauncher ?? throw new ArgumentNullException(nameof(gameLauncher));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _launchTools = launchTools ?? throw new ArgumentNullException(nameof(launchTools));
        _gameScannerService = gameScannerService ?? throw new ArgumentNullException(nameof(gameScannerService));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));

        // DataContext set to the MainWindow instance
        DataContext = this;

        // Load and Apply _settings
        ToggleGamepad.IsChecked = _settings.EnableGamePadNavigation;
        UpdateThumbnailSizeCheckMarks(_settings.ThumbnailSize);
        UpdateButtonAspectRatioCheckMarks(_settings.ButtonAspectRatio);
        UpdateNumberOfGamesPerPageCheckMarks(_settings.GamesPerPage);
        UpdateShowGamesCheckMarks(_settings.ShowGames);
        _filesPerPage = _settings.GamesPerPage;
        _paginationThreshold = _settings.GamesPerPage;
        ToggleFuzzyMatching.IsChecked = _settings.EnableFuzzyMatching;

        // Load _machines and _mameLookup
        _machines = MameManager.LoadFromDat();
        _mameLookup = _machines
            .GroupBy(static m => m.MachineName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First().Description, StringComparer.OrdinalIgnoreCase);

        LoadOrReloadSystemManager();

        _topLetterNumberMenu = new FilterMenu(_playSoundEffects);

        // Add _topLetterNumberMenu to the UI
        LetterNumberMenu.Children.Clear();
        LetterNumberMenu.Children.Add(_topLetterNumberMenu.LetterPanel);

        // Create and integrate FilterMenu
        _topLetterNumberMenu.OnLetterSelected += async selectedLetter =>
        {
            await TopLetterNumberMenuClickAsync(selectedLetter);
        };

        // Initialize _gameFileGrid
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;
        if (_gameFileGrid == null)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(new Exception("GameFileGrid not found"), "GameFileGrid not found");
        }

        // Initialize _gameButtonFactory
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _gameFileGrid, this, _gamePadController, _gameLauncher, _playSoundEffects, _logErrors);

        // Initialize _gameListFactory
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, PlayHistoryManager, this, _gamePadController, _gameLauncher, _playSoundEffects);

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        Activated += MainWindow_Activated;
        Deactivated += MainWindow_Deactivated;

        Loaded += async (_, _) =>
        {
            try
            {
                await DisplaySystemSelectionScreenAsync();
                DebugLogger.Log("DisplaySystemSelectionScreenAsync called.");
            }
            catch (Exception ex)
            {
                _ = _logErrors.LogErrorAsync(ex, "Error in the DisplaySystemSelectionScreenAsync method.");
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
                _ = _logErrors.LogErrorAsync(ex, "Error in the Loaded event.");
                DebugLogger.Log($"Error in the Loaded event: {ex.Message}");
            }

            try
            {
                // --- First-run experience: Check if system.xml is empty ---
                if (_systemManagers == null || _systemManagers.Count == 0)
                {
                    // This is the first run. Let's scan for Windows games automatically.
                    SetUiLoadingState(true, (string)Application.Current.TryFindResource("ScanningForWindowsGames") ?? "Scanning for Windows games...");
                    try
                    {
                        await _gameScannerService.ScanForStoreGamesAsync();
                        if (_gameScannerService.WasNewSystemCreated)
                        {
                            UpdateStatusBar.UpdateContent("Found new Microsoft Windows games. Refreshing system list.", this);
                            LoadOrReloadSystemManager(); // Reload to get the new system
                            // After reloading, the system selection screen needs to be updated.
                            await DisplaySystemSelectionScreenAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = _logErrors.LogErrorAsync(ex, "Error during initial Windows games scan.");
                    }
                    finally
                    {
                        SetUiLoadingState(false);
                    }

                    // After the scan, check again if any systems exist.
                    // If still no systems, show the Easy Mode prompt.
                    if (_systemManagers == null || _systemManagers.Count == 0)
                    {
                        var result = MessageBoxLibrary.FirstRunWelcomeMessageBox();
                        if (result == MessageBoxResult.Yes)
                        {
                            var easyModeWindow = new EasyModeWindow();
                            easyModeWindow.Owner = this;
                            easyModeWindow.ShowDialog();

                            LoadOrReloadSystemManager();
                            await DisplaySystemSelectionScreenAsync(); // Await this now
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logErrors.LogErrorAsync(ex, "Error in the Loaded event's first-run logic.");
                DebugLogger.Log($"Error in the Loaded event's first-run logic: {ex.Message}");
            }
        };
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

    private (string startLetter, string searchQuery) GetLoadGameFilesParams()
    {
        var searchQueryToUse = _activeSearchQueryOrMode;
        var startLetterToUse = string.IsNullOrEmpty(searchQueryToUse) ? _currentFilter : null;
        return (startLetterToUse, searchQueryToUse);
    }

    private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Check if the Ctrl key is pressed
        if (Keyboard.Modifiers != ModifierKeys.Control) return;

        switch (e.Delta)
        {
            case > 0:
                // Scroll up, trigger zoom in
                NavZoomInButtonClickAsync(null, null); // Pass null for sender and EventArgs
                break;
            case < 0:
                // Scroll down, trigger zoom out
                NavZoomOutButtonClickAsync(null, null); // Pass null for sender and EventArgs
                break;
        }

        // Mark the event as handled to prevent scrolling the ScrollViewer
        e.Handled = true;
    }

    private async Task TopLetterNumberMenuClickAsync(string selectedLetter)
    {
        if (_isLoadingGames) return;

        try
        {
            _playSoundEffects.PlayNotificationSound();

            ResetPaginationButtons(); // Ensure pagination is reset at the beginning
            SearchTextBox.Text = ""; // Clear SearchTextBox
            _currentFilter = selectedLetter; // Update current filter
            _activeSearchQueryOrMode = null; // Reset special search mode

            await LoadGameFilesAsync(selectedLetter, null); // searchQuery is null
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in TopLetterNumberMenuClickAsync.");
        }
    }

    private async Task ShowSystemFavoriteGamesClickAsync()
    {
        if (_isLoadingGames) return;

        try
        {
            _playSoundEffects.PlayNotificationSound();

            // Change the filter to ShowAll (as favorites might not have covers)
            _settings.ShowGames = "ShowAll";
            _settings.Save();
            ApplyShowGamesSetting(); // Update menu check marks

            ResetPaginationButtons();
            SearchTextBox.Text = ""; // Clear search field
            _currentFilter = null; // Clear any active letter filter
            _activeSearchQueryOrMode = "FAVORITES"; // Set special search mode

            // Filter favorites for the selected system and store them in _currentSearchResults
            var favoriteGames = GetFavoriteGamesForSelectedSystem(_favoritesManager);
            if (favoriteGames.Count != 0)
            {
                _currentSearchResults = favoriteGames.ToList(); // Store only favorite games in _currentSearchResults

                await LoadGameFilesAsync(null, "FAVORITES"); // Call LoadGameFilesAsync
            }
            else
            {
                // Notify user
                AddNoFilesMessage();
                MessageBoxLibrary.NoFavoriteFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in ShowSystemFavoriteGamesClickAsync.");
        }
    }

    private async Task ShowSystemFeelingLuckyClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                // Change the filter to ShowAll (as random might not have covers)
                _settings.ShowGames = "ShowAll";
                _settings.Save();
                ApplyShowGamesSetting(); // Update menu check marks

                // Check if a system is selected
                if (SystemComboBox.SelectedItem == null)
                {
                    // Notify user
                    MessageBoxLibrary.PleaseSelectASystemBeforeMessageBox();
                    return;
                }

                await _allGamesLock.WaitAsync(); // Acquire lock before accessing _allGamesForCurrentSystem
                try
                {
                    var selectedSystem = SystemComboBox.SelectedItem.ToString();
                    var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);
                    if (selectedManager == null)
                    {
                        return;
                    }

                    List<string> gameFilesToPickFrom;

                    if (_allGamesForCurrentSystem != null && _allGamesForCurrentSystem.Count != 0 && _selectedSystem == selectedSystem)
                    {
                        gameFilesToPickFrom = _allGamesForCurrentSystem; // READ
                        DebugLogger.Log($"[Feeling Lucky] Reusing cached _allGamesForCurrentSystem for '{selectedSystem}'. Count: {gameFilesToPickFrom.Count}");
                    }
                    else
                    {
                        // If not, perform the disk scan to get all files for the current system
                        // This scenario should be rare if SystemComboBoxSelectionChangedAsync always populates it.
                        // It acts as a fallback.
                        DebugLogger.Log($"[Feeling Lucky] _allGamesForCurrentSystem not suitable, performing disk scan for '{selectedSystem}'.");
                        var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var folder in selectedManager.SystemFolders)
                        {
                            var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                            if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) ||
                                selectedManager.FileFormatsToSearch == null) continue;

                            var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch);
                            foreach (var file in filesInFolder)
                            {
                                uniqueFiles.TryAdd(Path.GetFileName(file), file);
                            }
                        }

                        gameFilesToPickFrom = uniqueFiles.Values.ToList();
                        _allGamesForCurrentSystem = gameFilesToPickFrom; // WRITE
                    }

                    // Check if we have any games after filtering
                    if (gameFilesToPickFrom.Count == 0)
                    {
                        // Notify user
                        MessageBoxLibrary.NoGameFoundInTheRandomSelectionMessageBox();
                        return;
                    }

                    // Randomly select a game
                    var random = new Random();
                    var randomIndex = random.Next(0, gameFilesToPickFrom.Count);
                    var selectedGame = gameFilesToPickFrom[randomIndex];

                    // Reset letter selection in the UI and current search
                    _topLetterNumberMenu.DeselectLetter();
                    SearchTextBox.Text = "";
                    _currentFilter = null; // Clear any active letter filter
                    _activeSearchQueryOrMode = "RANDOM_SELECTION"; // Set special search mode
                    _currentSearchResults = [selectedGame]; // Store only the selected game

                    await LoadGameFilesAsync(null, "RANDOM_SELECTION");

                    // If in list view, select the game in the DataGrid
                    if (_settings.ViewMode != "ListView" || GameDataGrid.Items.Count <= 0) return;

                    GameDataGrid.SelectedIndex = 0;
                    GameDataGrid.ScrollIntoView(GameDataGrid.SelectedItem);
                    GameDataGrid.Focus();
                }
                finally
                {
                    _allGamesLock.Release(); // Release lock
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error in the Feeling Lucky feature.";
                _ = _logErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in ShowSystemFeelingLuckyClickAsync.");
        }
    }

    public void RefreshGameListAfterPlay(string fileName, string systemName)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("RefreshingGameList") ?? "Refreshing game list...", this);
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
                // ReSharper disable once PossibleNullReferenceException
                Path.GetFileName(item.FilePath).Equals(fileName, StringComparison.OrdinalIgnoreCase));

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
            _ = _logErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    public void SetUiLoadingState(bool isLoading, string message = null)
    {
        _isLoadingGames = isLoading;
        IsLoadingGames = isLoading;

        // Disable/Enable main interaction controls
        SystemComboBox.IsEnabled = !isLoading;
        EmulatorComboBox.IsEnabled = !isLoading;
        SearchTextBox.IsEnabled = !isLoading;
        SearchButton.IsEnabled = !isLoading;
        SelectedSystemFavoriteButton.IsEnabled = !isLoading;
        RandomLuckGameButton.IsEnabled = !isLoading;
        SortOrderToggleButton.IsEnabled = !isLoading;
        ToggleViewMode.IsEnabled = !isLoading;
        ToggleButtonAspectRatio.IsEnabled = !isLoading;
        ZoomInButton.IsEnabled = !isLoading;
        ZoomOutButton.IsEnabled = !isLoading;

        // Disable/Enable Letter/Number/Favorites/Lucky buttons via FilterMenu helper
        _topLetterNumberMenu.SetButtonsEnabled(!isLoading);

        // Disable/Enable pagination buttons (UpdatePaginationButtons already checks _isGameListLoading)
        UpdatePaginationButtons();

        // Update loading message
        if (isLoading)
        {
            LoadingMessage.Text = message ?? ((string)Application.Current.TryFindResource("Loading") ?? "Loading...");
        }
    }

    private void SaveApplicationSettings()
    {
        // Save application's current state
        _settings.ThumbnailSize = _gameButtonFactory.ImageHeight;
        _settings.GamesPerPage = _filesPerPage;
        _settings.ShowGames = _settings.ShowGames;
        _settings.EnableGamePadNavigation = ToggleGamepad.IsChecked;
        _settings.EnableFuzzyMatching = ToggleFuzzyMatching.IsChecked;

        // Save theme settings
        var detectedTheme = ThemeManager.Current.DetectTheme(this);
        if (detectedTheme != null)
        {
            _settings.BaseTheme = detectedTheme.BaseColorScheme;
            _settings.AccentColor = detectedTheme.ColorScheme;
        }

        _settings.Save();
    }

    private List<string> GetFavoriteGamesForSelectedSystem(FavoritesManager favoritesManager)
    {
        // // Reload favorites to ensure we have the latest data
        // _favoritesManager = FavoritesManager.LoadFavorites();

        // Use the injected favoritesManager instance directly (no need to load again)
        var favorites = favoritesManager.FavoriteList;

        var selectedSystem = SystemComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedSystem))
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        // Retrieve the system manager for the selected system
        var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        if (selectedManager == null)
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        // Filter the favorites and build the full file path for each favorite game
        var favoriteGamePaths = favorites
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => PathHelper.FindFileInSystemFolders(selectedManager, fav.FileName))
            .Where(static path => !string.IsNullOrEmpty(path))
            .ToList();

        return favoriteGamePaths;
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
        var gameListViewFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, PlayHistoryManager, this, _gamePadController, _gameLauncher, _playSoundEffects);
        gameListViewFactory.HandleSelectionChangedAsync(selectedItem);
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

            // Delegate the double-click handling to GameListFactory
            await _gameListFactory.HandleDoubleClickAsync(selectedItem);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error while using the method GameListDoubleClickOnSelectedItemAsync.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private async void SystemComboBoxSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            try
            {
                try
                {
                    if (_isUiUpdating)
                    {
                        return; // Prevent re-entrance
                    }

                    if (SystemComboBox.SelectedItem == null)
                    {
                        // Clear the cached list when no system is selected
                        _allGamesForCurrentSystem.Clear();
                        IsPlayTimeVisible = false; // Hide when no system is selected

                        return;
                    }

                    SetUiLoadingState(true, (string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...");
                    _isUiUpdating = true; // Set after UI is frozen
                    try
                    {
                        SearchTextBox.Text = "";
                        EmulatorComboBox.ItemsSource = null;
                        EmulatorComboBox.SelectedIndex = -1;
                        PreviewImage.Source = null;

                        _currentSearchResults.Clear();
                        _currentFilter = null;
                        _activeSearchQueryOrMode = null;

                        GameFileGrid.Visibility = Visibility.Visible;
                        ListViewPreviewArea.Visibility = Visibility.Collapsed;

                        var selectedSystem = SystemComboBox.SelectedItem?.ToString();

                        // // --- "Microsoft Windows" rescan prompt ---
                        // if (await ReScanMicrosoftWindowsSystem(selectedSystem)) return; // Exit after handling rescan, do not proceed with normal load for this selection

                        var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);
                        if (selectedSystem == null || selectedManager == null)
                        {
                            // Notify developer
                            const string errorMessage = "Selected system or its configuration is null.";
                            _ = _logErrors.LogErrorAsync(null, errorMessage);

                            // Notify user
                            MessageBoxLibrary.InvalidSystemConfigMessageBox();
                            SortOrderToggleButton.Visibility = Visibility.Collapsed;

                            SystemComboBox.SelectedItem = null;
                            await DisplaySystemSelectionScreenAsync();

                            // Clear the cached list on error
                            _allGamesForCurrentSystem.Clear();

                            return;
                        }

                        var formats = selectedManager.FileFormatsToSearch;
                        IsPlayTimeVisible = formats == null || !formats.Any(static f =>
                            f.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                            f.Equals("lnk", StringComparison.OrdinalIgnoreCase) ||
                            f.Equals("bat", StringComparison.OrdinalIgnoreCase));

                        _mameSortOrder = "FileName";
                        UpdateSortOrderButtonUi();
                        SortOrderToggleButton.Visibility = selectedManager.SystemIsMame ? Visibility.Visible : Visibility.Collapsed;

                        EmulatorComboBox.ItemsSource = selectedManager.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
                        if (EmulatorComboBox.Items.Count > 0)
                        {
                            EmulatorComboBox.SelectedIndex = 0;
                        }

                        SelectedSystem = selectedSystem;

                        var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
                        PlayTime = systemPlayTime != null ? systemPlayTime.PlayTime : "00:00:00";

                        // Display SystemInfo and get the validation result. Game count is now handled inside this method.
                        var validationResult = await DisplaySystemInformation.DisplaySystemInfoAsync(selectedManager, _gameFileGrid);

                        // If validation failed, show the message box with aggregated errors
                        if (!validationResult.IsValid)
                        {
                            var errorMessages = new System.Text.StringBuilder();
                            foreach (var msg in validationResult.ErrorMessages)
                            {
                                errorMessages.Append(msg);
                            }

                            MessageBoxLibrary.ListOfErrorsMessageBox(errorMessages);
                        }

                        // Resolve the system image folder path using PathHelper
                        var resolvedSystemImageFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedManager.SystemImageFolder);

                        _selectedRomFolders = selectedManager.SystemFolders.Select(PathHelper.ResolveRelativeToAppDirectory).ToList();
                        _selectedImageFolder = string.IsNullOrWhiteSpace(resolvedSystemImageFolderPath)
                            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedManager.SystemName) // Use the default resolved path
                            : resolvedSystemImageFolderPath; // Use resolved configured path

                        await PopulateAllGamesForCurrentSystem(selectedManager, selectedSystem);

                        _topLetterNumberMenu.DeselectLetter();
                        ResetPaginationButtons();
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        const string errorMessage = "Error in the method SystemComboBoxSelectionChangedAsync.";
                        _ = _logErrors.LogErrorAsync(ex, errorMessage);

                        // Notify user
                        MessageBoxLibrary.InvalidSystemConfigMessageBox();

                        // Clear cached list on error
                        _allGamesForCurrentSystem.Clear();
                    }
                    finally
                    {
                        SetUiLoadingState(false);
                        _isUiUpdating = false;
                    }
                }
                catch (Exception ex)
                {
                    _ = _logErrors.LogErrorAsync(ex, "Error in SystemComboBoxSelectionChangedAsync.");
                }

                return;

                async Task PopulateAllGamesForCurrentSystem(SystemManager selectedManager, string currentSelectedSystem)
                {
                    var uniqueFilesForSystem = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var folder in _selectedRomFolders)
                    {
                        // This part can run concurrently with other file system operations, but not modify _allGamesForCurrentSystem
                        var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                        if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) || selectedManager.FileFormatsToSearch == null) continue;

                        var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch);
                        foreach (var file in filesInFolder)
                        {
                            uniqueFilesForSystem.TryAdd(Path.GetFileName(file), file);
                        }
                    }

                    await _allGamesLock.WaitAsync(); // Acquire lock before modifying _allGamesForCurrentSystem
                    try
                    {
                        _allGamesForCurrentSystem = uniqueFilesForSystem.Values.ToList(); // WRITE
                        DebugLogger.Log($"[SystemComboBoxSelectionChangedAsync] Populated _allGamesForCurrentSystem for '{currentSelectedSystem}'. Count: {_allGamesForCurrentSystem.Count}");
                    }
                    finally
                    {
                        _allGamesLock.Release(); // Release lock
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logErrors.LogErrorAsync(ex, "Error in SystemComboBoxSelectionChangedAsync.");
            }

            return;

            // async Task<bool> ReScanMicrosoftWindowsSystem(string selectedSystem)
            // {
            //     if (selectedSystem == "Microsoft Windows")
            //     {
            //         var rescanResult = MessageBoxLibrary.AskToRescanWindowsGamesMessageBox();
            //         if (rescanResult == MessageBoxResult.Yes)
            //         {
            //             SetUiLoadingState(true, (string)Application.Current.TryFindResource("ScanningForWindowsGames") ?? "Scanning for Windows games...");
            //             try
            //             {
            //                 // The GameScannerService.ScanForStoreGamesAsync() already handles creating the system if it doesn't exist.
            //                 // It also updates the system.xml if new games are found, which LoadOrReloadSystemManager() will pick up.
            //                 await _gameScannerService.ScanForStoreGamesAsync();
            //
            //                 // After scan, reload systems and games
            //                 LoadOrReloadSystemManager();
            //                 // Reload the current system's games to reflect any new shortcuts found
            //                 await LoadGameFilesAsync(null, null);
            //                 UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("WindowsGamesScanComplete") ?? "Windows games scan complete.", this);
            //             }
            //             catch (Exception ex)
            //             {
            //                 _ = _logErrors.LogErrorAsync(ex, "Error during Windows games rescan.");
            //                 MessageBoxLibrary.ErrorScanningWindowsGamesMessageBox();
            //             }
            //             finally
            //             {
            //                 SetUiLoadingState(false);
            //             }
            //
            //             return true;
            //         }
            //     }
            //
            //     return false;
            // }
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in SystemComboBoxSelectionChangedAsync.");
        }
    }

    private void AddNoFilesMessage()
    {
        var noGamesMatched = (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button.";

        // Check the current view mode
        if (_settings.ViewMode == "GridView")
        {
            // Clear existing content in Grid view and add the message
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noGamesMatched}",
                Padding = new Thickness(10)
            });
        }
        else
        {
            // For List view, clear GameListItems
            GameListItems.Clear();
            GameListItems.Add(new GameListViewItem
            {
                FileName = noGamesMatched,
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _topLetterNumberMenu.DeselectLetter();
    }

    public void SetGameButtonsEnabled(bool isEnabled)
    {
        if (_gameFileGrid == null) return;

        foreach (var child in _gameFileGrid.Children)
        {
            if (child is Button button)
            {
                button.IsEnabled = isEnabled;
            }
        }
    }

    public async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...", this);
        Dispatcher.Invoke(() => SetUiLoadingState(true, (string)Application.Current.TryFindResource("LoadingGames") ?? "Loading Games..."));
        await SetUiBeforeLoadGameFilesAsync();

        try
        {
            if (SystemComboBox.SelectedItem == null)
            {
                await DisplaySystemSelectionScreenAsync();
                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);
            if (selectedManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedConfig is null.";
                _ = _logErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                await DisplaySystemSelectionScreenAsync();

                return;
            }

            var allFiles = await BuildListOfAllFilesToLoad(selectedManager);

            if (selectedManager.GroupByFolder)
            {
                static string EnsureTrailingSlash(string path)
                {
                    if (string.IsNullOrEmpty(path))
                        return path;

                    return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                }

                var rootFolders = selectedManager.SystemFolders
                    .Select(PathHelper.ResolveRelativeToAppDirectory)
                    .Where(static p => !string.IsNullOrEmpty(p))
                    .Select(EnsureTrailingSlash)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var groupedFiles = allFiles
                    .GroupBy(f =>
                    {
                        var fileDir = Path.GetDirectoryName(f);
                        var normalizedFileDir = EnsureTrailingSlash(fileDir);

                        // If the file's directory is one of the main system folders, it's a "root" file.
                        // Its group key will be its own full path.
                        if (rootFolders.Contains(normalizedFileDir))
                        {
                            return f;
                        }

                        // Otherwise, its group key is its parent directory.
                        return fileDir;
                    })
                    .Select(static g => g.Key) // This gives us a list of unique file paths (for root files) and directory paths (for subfolders)
                    .ToList();
                allFiles = groupedFiles;
            }

            allFiles = ProcessListOfAllFilesWithMachineDescription(selectedManager, allFiles);

            allFiles = await FilterFilesByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);

            allFiles = SetPaginationOfListOfFiles(allFiles);

            foreach (var filePath in allFiles) // 'filePath' is already resolved here
            {
                if (_settings.ViewMode == "GridView") // GridView
                {
                    var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedManager, this);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // ListView
                {
                    var gameListViewItem = await _gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedManager);
                    await Dispatcher.InvokeAsync(() => GameListItems.Add(gameListViewItem));
                }
            }

            switch (_settings.ViewMode)
            {
                case "GridView":
                    Scroller.Focus();
                    break;
                case "ListView":
                    GameDataGrid.Focus();
                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
        finally
        {
            Dispatcher.Invoke(() => SetUiLoadingState(false));
        }

        return;

        async Task<List<string>> BuildListOfAllFilesToLoad(SystemManager selectedManager)
        {
            List<string> allFiles;

            switch (searchQuery)
            {
                case "FAVORITES" when _currentSearchResults != null && _currentSearchResults.Count != 0:
                case "RANDOM_SELECTION" when _currentSearchResults != null && _currentSearchResults.Count != 0:
                    allFiles = new List<string>(_currentSearchResults);
                    break;
                default: // This branch handles initial load, letter filter, and text search
                {
                    // If no specific filter (letter or search query), and _allGamesForCurrentSystem is already populated for this system,
                    // use it directly. Otherwise, perform a full disk scan.
                    // The _selectedSystem field ensures _allGamesForCurrentSystem is for the *currently active* system.
                    await _allGamesLock.WaitAsync(); // Acquire lock before accessing _allGamesForCurrentSystem
                    try
                    {
                        if (string.IsNullOrWhiteSpace(startLetter) && string.IsNullOrWhiteSpace(searchQuery) &&
                            _allGamesForCurrentSystem != null && _allGamesForCurrentSystem.Count != 0 &&
                            _selectedSystem == selectedManager.SystemName)
                        {
                            allFiles = new List<string>(_allGamesForCurrentSystem); // READ
                            DebugLogger.Log($"[BuildListOfAllFilesToLoad] Reusing cached _allGamesForCurrentSystem for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                        else
                        {
                            // If _allGamesForCurrentSystem is not suitable or not yet populated, perform a full disk scan.
                            // This part is outside the lock because it involves I/O and doesn't modify _allGamesForCurrentSystem yet.
                            // The lock is acquired only when _allGamesForCurrentSystem is read or written.
                            _allGamesLock.Release(); // Temporarily release lock for disk scan
                            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var folder in selectedManager.SystemFolders)
                            {
                                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                                if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath)) continue;

                                var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch);
                                foreach (var file in filesInFolder)
                                {
                                    uniqueFiles.TryAdd(Path.GetFileName(file), file);
                                }
                            }

                            allFiles = uniqueFiles.Values.ToList(); // This is the full list from disk for the system

                            await _allGamesLock.WaitAsync(); // Re-acquire lock before potentially writing to _allGamesForCurrentSystem

                            // If no specific filter (letter or search query), this is the "all games" list.
                            // Cache it for future "Feeling Lucky" calls and direct "All" view loads.
                            if (string.IsNullOrWhiteSpace(startLetter) && string.IsNullOrWhiteSpace(searchQuery))
                            {
                                _allGamesForCurrentSystem = new List<string>(allFiles); // WRITE
                                DebugLogger.Log($"[BuildListOfAllFilesToLoad] Populated _allGamesForCurrentSystem for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                            }
                        }
                    }
                    finally
                    {
                        _allGamesLock.Release(); // Ensure lock is released
                    }

                    // ... filtering by startLetter ...
                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        allFiles = await FilterFilesAsync(allFiles, startLetter);
                    }

                    // ... filtering by searchQuery ...
                    if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery != "RANDOM_SELECTION" && searchQuery != "FAVORITES")
                    {
                        var systemIsMame = selectedManager.SystemIsMame;
                        var lowerQuery = searchQuery.ToLowerInvariant();
                        allFiles = await Task.Run(() =>
                            allFiles.FindAll(file =>
                            {
                                var fileName = Path.GetFileNameWithoutExtension(file);
                                var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                if (filenameMatch) return true;

                                if (systemIsMame && _mameLookup != null && _mameLookup.TryGetValue(fileName, out var description))
                                {
                                    return description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                }

                                return false;
                            }));

                        _currentSearchResults = new List<string>(allFiles); // Store search results
                    }

                    break;
                }
            }

            return allFiles;
        }

        List<string> ProcessListOfAllFilesWithMachineDescription(SystemManager selectedManager, List<string> allFiles)
        {
            if (selectedManager.SystemIsMame && _mameSortOrder == "MachineDescription")
            {
                allFiles = allFiles.OrderBy(f =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(f);
                    return _mameLookup.TryGetValue(fileName, out var description) && !string.IsNullOrWhiteSpace(description)
                        ? description
                        : fileName;
                }, StringComparer.OrdinalIgnoreCase).ToList();
            }
            else
            {
                allFiles = allFiles.OrderBy(static f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).ToList();
            }

            return allFiles;
        }
    }

    private List<string> SetPaginationOfListOfFiles(List<string> allFiles)
    {
        // Count the collection of files (this should be the total before pagination)
        // If allFiles is already paginated, _totalFiles needs to be set from the unpaginated list.
        // _totalFiles should be set based on the count of files *before* pagination.
        // For FAV/RANDOM/Search, _currentSearchResults holds the full list.
        // For letter/all, the 'allFiles' passed here (before this method's Skip/Take) is the full list for that filter.
        _totalFiles = allFiles.Count;


        // Calculate the indices of files displayed on the current page
        var startIndex = (_currentPage - 1) * _filesPerPage + 1; // +1 because we are dealing with a 1-based index for displaying
        var endIndex = Math.Min(startIndex + _filesPerPage - 1, _totalFiles); // Actual number of files loaded on this page

        // Pagination related
        if (_totalFiles > _paginationThreshold)
        {
            // Enable pagination and adjust file list based on the current page
            allFiles = allFiles.Skip((_currentPage - 1) * _filesPerPage).Take(_filesPerPage).ToList();

            // Update or create pagination controls
            UpdatePaginationButtons();
        }
        else
        {
            // If total files are not enough for pagination, ensure buttons are disabled.
            _prevPageButton.IsEnabled = false;
            _nextPageButton.IsEnabled = false;
        }


        // Display message if the number of files == 0 (after potential pagination, so check the paginated list)
        if (allFiles.Count == 0 && _totalFiles == 0) // Check if the original list was also empty
        {
            AddNoFilesMessage();
        }

        // Update the UI to reflect the current pagination status and the indices of files being displayed
        var displayingfiles0To = (string)Application.Current.TryFindResource("Displayingfiles0to") ?? "Displaying files 0 to";
        var outOf = (string)Application.Current.TryFindResource("outof") ?? "out of";
        var total = (string)Application.Current.TryFindResource("total") ?? "total";
        var displayingfiles = (string)Application.Current.TryFindResource("Displayingfiles") ?? "Displaying files";
        var to = (string)Application.Current.TryFindResource("to") ?? "to";

        TotalFilesLabel.Dispatcher.Invoke(() =>
            TotalFilesLabel.Content = _totalFiles == 0 ? $"{displayingfiles0To} 0 {outOf} 0 {total}" : $"{displayingfiles} {(_totalFiles > 0 ? startIndex : 0)} {to} {endIndex} {outOf} {_totalFiles} {total}"
        );
        return allFiles;
    }

    private async Task SetUiBeforeLoadGameFilesAsync()
    {
        // Move scroller to top
        Scroller.Dispatcher.Invoke(() => Scroller.ScrollToTop());

        // Clear PreviewImage
        PreviewImage.Source = null;

        // Clear Game Grid
        GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Clear());

        // Clear the Game List
        await Dispatcher.InvokeAsync(() => GameListItems.Clear());

        // Set ViewMode based on user preference
        if (_settings.ViewMode == "GridView")
        {
            // Allow GridView
            GameFileGrid.Visibility = Visibility.Visible;
            ListViewPreviewArea.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Allow ListView
            GameFileGrid.Visibility = Visibility.Collapsed;
            ListViewPreviewArea.Visibility = Visibility.Visible;
        }
    }

    private Task<List<string>> FilterFilesByShowGamesSettingAsync(List<string> files, string selectedSystem, SystemManager selectedConfig)
    {
        if (files.Count == 0 || _settings.ShowGames == "ShowAll")
            return Task.FromResult(files);

        var filteredFiles = new List<string>();

        foreach (var filePath in files) // 'filePath' is already resolved here
        {
            var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);

            var imagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, selectedConfig, _settings);

            bool isDefaultImage;
            if (string.IsNullOrEmpty(imagePath) || imagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
            {
                isDefaultImage = true;
            }
            else
            {
                // Resolve the found image path before checking existence
                var resolvedImagePath = PathHelper.ResolveRelativeToAppDirectory(imagePath);
                isDefaultImage = string.IsNullOrEmpty(resolvedImagePath) || !File.Exists(resolvedImagePath) || resolvedImagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase);
            }

            switch (_settings.ShowGames)
            {
                case "ShowWithCover" when !isDefaultImage:
                case "ShowWithoutCover" when isDefaultImage:
                    filteredFiles.Add(filePath);
                    break;
            }
        }

        return Task.FromResult(filteredFiles);
    }

    private static Task<List<string>> FilterFilesAsync(List<string> files, string startLetter)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(startLetter))
                return files;

            if (startLetter == "#")
            {
                return files.Where(static file => !string.IsNullOrEmpty(file) &&
                                                  file.Length > 0 &&
                                                  char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }

            return files.Where(file => !string.IsNullOrEmpty(file) &&
                                       Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
        });
    }

    private async void SortOrderToggleButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            _playSoundEffects.PlayNotificationSound();
            _mameSortOrder = _mameSortOrder == "FileName" ? "MachineDescription" : "FileName";
            UpdateSortOrderButtonUi();

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq);
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in SortOrderToggleButtonClickAsync.");
            DebugLogger.Log("Error in SortOrderToggleButtonClickAsync.");
        }
    }

    private void UpdateSortOrderButtonUi()
    {
        if (SortOrderToggleButton == null) return;

        if (_mameSortOrder == "FileName")
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

    /// <summary>
    /// Invalidates the in-memory caches of game file paths, forcing a reload from disk
    /// or re-evaluation of search results on the next LoadGameFilesAsync call.
    /// </summary>
    public void InvalidateGameFileCaches()
    {
        // Ensure cache clearing happens on the UI thread for thread safety,
        // although LoadGameFilesAsync will also be invoked on the dispatcher.
        Dispatcher.Invoke(() =>
        {
            _allGamesForCurrentSystem.Clear();
            _currentSearchResults.Clear();
            DebugLogger.Log("[MainWindow.InvalidateGameFileCaches] All game file caches invalidated.");
        });
    }
}