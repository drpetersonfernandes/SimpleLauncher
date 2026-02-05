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
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindAndLoadImages;
using SimpleLauncher.Services.GameItemFactory;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.GameScan;
using SimpleLauncher.Services.LaunchTools;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.UiHelpers;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.Services.UsageStats;
using SimpleLauncher.SharedModels;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using UpdateChecker = SimpleLauncher.Services.CheckForUpdates.UpdateChecker;

namespace SimpleLauncher;

using ILoadingState = Services.LoadingInterface.ILoadingState;

public partial class MainWindow : INotifyPropertyChanged, IDisposable, ILoadingState
{
    private CancellationTokenSource _cancellationSource = new();
    private bool _isUiUpdating;
    private bool _wasControllerRunningBeforeDeactivation;

    // DispatcherTimer for the status bar timer
    internal DispatcherTimer StatusBarTimer { get; set; }

    // Declare GameListItems
    // Used in ListView Mode
    public ObservableCollection<GameListViewItem> GameListItems { get; } = [];

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
    internal PlayHistoryManager PlayHistoryManager { get; }

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
    private readonly RetroAchievementsService _retroAchievementsService;

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

    public MainWindow(
        SettingsManager settings,
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        UpdateChecker updateChecker,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        ILaunchTools launchTools,
        Stats stats,
        ILogErrors logErrors,
        GameScannerService gameScannerService,
        RetroAchievementsService retroAchievementsService)
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
        _retroAchievementsService = retroAchievementsService ?? throw new ArgumentNullException(nameof(retroAchievementsService));

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
            _ = _logErrors.LogErrorAsync(null, "GameFileGrid not found");
        }

        // Initialize _gameButtonFactory
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _gameFileGrid, this, _gamePadController, _gameLauncher, _playSoundEffects, _logErrors);

        // Initialize _gameListFactory
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, PlayHistoryManager, this, _gamePadController, _gameLauncher, _playSoundEffects);

        // Migrate old play history records to full paths
        PlayHistoryManager.MigrateFilenamesToFullPaths(_systemManagers);

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
                    SetLoadingState(true, (string)Application.Current.TryFindResource("ScanningForWindowsGames") ?? "Scanning for Windows games...");
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
                        SetLoadingState(false);
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

    private void CancelAndRecreateToken()
    {
        _cancellationSource.Cancel();
        _cancellationSource.Dispose();
        _cancellationSource = new CancellationTokenSource();
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
        try
        {
            if (_isLoadingGames)
            {
                CancelAndRecreateToken();
            }

            _playSoundEffects.PlayNotificationSound();

            ResetPaginationButtons(); // Ensure pagination is reset at the beginning
            SearchTextBox.Text = ""; // Clear SearchTextBox
            _currentFilter = selectedLetter; // Update current filter
            _activeSearchQueryOrMode = null; // Reset special search mode

            await LoadGameFilesAsync(selectedLetter, null, _cancellationSource.Token); // searchQuery is null
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in TopLetterNumberMenuClickAsync.");
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
            _settings.Save();
            ApplyShowGamesSetting();

            SearchTextBox.Text = "";
            _currentFilter = null;
            _activeSearchQueryOrMode = "FAVORITES";

            await LoadGameFilesAsync(null, "FAVORITES", _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in ShowSystemFavoriteGamesClickAsync.");
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
            _settings.Save();
            ApplyShowGamesSetting();

            _topLetterNumberMenu.DeselectLetter();
            SearchTextBox.Text = "";
            _currentFilter = null;
            _activeSearchQueryOrMode = "RANDOM_SELECTION";

            await LoadGameFilesAsync(null, "RANDOM_SELECTION", _cancellationSource.Token);

            // If in list view, select the game in the DataGrid
            if (_settings.ViewMode != "ListView" || GameDataGrid.Items.Count <= 0) return;

            GameDataGrid.SelectedIndex = 0;
            GameDataGrid.ScrollIntoView(GameDataGrid.SelectedItem);
            GameDataGrid.Focus();
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in ShowSystemFeelingLuckyClickAsync.");
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    internal void RefreshGameListAfterPlay(string fileName, string systemName)
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
            _ = _logErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        // Update the internal flags used by pagination/search logic
        _isLoadingGames = isLoading;
        IsLoadingGames = isLoading; // Notifies UI via PropertyChanged

        // Visual Updates
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            // Disable the entire content area to prevent clicks/hotkeys during load
            MainContentGrid.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
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

    internal void SetGameButtonsEnabled(bool isEnabled)
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
    internal void InvalidateGameFileCaches()
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