using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ControlzEx.Theming;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SimpleLauncher;

public partial class MainWindow : INotifyPropertyChanged, IDisposable
{
    // Declare Controller Detection
    private DispatcherTimer _controllerCheckTimer;

    // Declare CacheManager and CacheFiles
    private readonly CacheManager _cacheManager = new();
    private List<string> _cachedFiles;

    // Declare GameListItems
    // Used in ListView Mode
    public ObservableCollection<GameListFactory.GameListViewItem> GameListItems { get; set; } = [];

    // Declare System Name and PlayTime in the Statusbar
    // _selectedSystem is the selected system from ComboBox
    public event PropertyChangedEventHandler PropertyChanged;
    private string _selectedSystem;
    private string _playTime;

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
        get => _playTime;
        set
        {
            _playTime = value;
            OnPropertyChanged(nameof(PlayTime));
        }
    }

    private void OnPropertyChanged(string propertyName) // Update UI on OnPropertyChanged
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Define Tray Icon
    private TrayIconManager _trayIconManager;

    // Define PlayHistory
    private PlayHistoryManager _playHistoryManager;

    // Define Pagination Related Variables
    private int _currentPage = 1;
    private int _filesPerPage;
    private int _totalFiles;
    private int _paginationThreshold;
    private Button _nextPageButton;
    private Button _prevPageButton;
    private string _currentFilter;

    // Define _currentSearchResults
    private List<string> _currentSearchResults = [];

    // Define and Instantiate variables
    private List<SystemManager> _systemConfigs;
    private readonly FilterMenu _filterMenu = new();
    private readonly GameListFactory _gameListFactory;
    private readonly WrapPanel _gameFileGrid;
    private GameButtonFactory _gameButtonFactory;
    private readonly SettingsManager _settings;
    private FavoritesManager _favoritesManager;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup; // Used for faster lookup of MAME machine names
    private string _selectedImageFolder;
    private string _selectedRomFolder;

    // Define the LogPath
    private readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

    public MainWindow()
    {
        InitializeComponent();

        // Initialize settings from App
        _settings = App.Settings;

        // Check for Command-line Args
        // Show UpdateHistory after the MainWindow is fully loaded
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("whatsnew"))
        {
            Loaded += static (_, _) => OpenUpdateHistory();
        }

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
        // Set initial state of Fuzzy Matching menu item
        ToggleFuzzyMatching.IsChecked = _settings.EnableFuzzyMatching;

        // Load _machines and _mameLookup
        _machines = MameManager.LoadFromDat();
        _mameLookup = _machines
            .GroupBy(static m => m.MachineName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First().Description, StringComparer.OrdinalIgnoreCase);

        LoadOrReloadSystemConfig();

        // Initialize the GamePadController
        GamePadController.Instance2.ErrorLogger = (ex, msg) => { _ = LogErrors.LogErrorAsync(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }

        // Add _filterMenu to the UI
        LetterNumberMenu.Children.Clear();
        LetterNumberMenu.Children.Add(_filterMenu.LetterPanel);

        // Create and integrate FilterMenu
        _filterMenu.OnLetterSelected += async selectedLetter =>
        {
            await Letter_Click(selectedLetter);
        };
        _filterMenu.OnFavoritesSelected += async () =>
        {
            await Favorites_Click();
        };
        _filterMenu.OnFeelingLuckySelected += async () =>
        {
            await FeelingLucky_Click(null, null);
        };

        // Initialize _favoritesManager
        _favoritesManager = FavoritesManager.LoadFavorites();

        // Initialize _gameFileGrid
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;

        // Initialize _gameButtonFactory
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _gameFileGrid, this);

        // Initialize _gameListFactory
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);

        // Check for Updates
        Loaded += async (_, _) => await UpdateChecker.CheckForUpdatesAsync(this);

        // Call Stats API
        Loaded += static (_, _) =>
        {
            _ = Stats.CallApiAsync();
        };

        // Attach the Load and Close events
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply language
        SetLanguageAndCheckMenu(_settings.Language);

        // Apply Theme
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);

        // Load previous windows state
        Width = _settings.MainWindowWidth;
        Height = _settings.MainWindowHeight;
        Top = _settings.MainWindowTop;
        Left = _settings.MainWindowLeft;
        WindowState = Enum.Parse<WindowState>(_settings.MainWindowState);

        // Set the initial SelectedSystem and PlayTime
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        SelectedSystem = nosystemselected;
        PlayTime = "00:00:00";

        // Set the initial ViewMode based on the _settings
        SetViewMode(_settings.ViewMode);

        // Check if a system is already selected, otherwise show the message
        if (SystemComboBox.SelectedItem == null)
        {
            AddNoSystemMessage();
        }

        // Check if the application has write access
        if (!CheckIfDirectoryIsWritable.IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory))
        {
            MessageBoxLibrary.MoveToWritableFolderMessageBox();
        }

        // Set initial pagination state
        PrevPageButton.IsEnabled = false;
        NextPageButton.IsEnabled = false;
        _prevPageButton = PrevPageButton;
        _nextPageButton = NextPageButton;

        // Update the GamePadController dead zone settings from SettingsManager
        GamePadController.Instance2.DeadZoneX = _settings.DeadZoneX;
        GamePadController.Instance2.DeadZoneY = _settings.DeadZoneY;

        InitializeControllerDetection();

        // Initialize TrayIconManager
        _trayIconManager = new TrayIconManager(this);

        // Initialize PlayHistory
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();

        // Check for required files
        CheckForRequiredFiles.CheckFiles();
    }

    private Task Letter_Click(string selectedLetter)
    {
        ResetPaginationButtons(); // Ensure pagination is reset at the beginning
        SearchTextBox.Text = ""; // Clear SearchTextBox
        _currentFilter = selectedLetter; // Update current filter
        return LoadGameFilesAsync(selectedLetter); // Load games
    }

    private Task Favorites_Click()
    {
        // Change filter to ShowAll
        _settings.ShowGames = "ShowAll";
        _settings.Save();
        ApplyShowGamesSetting();

        ResetPaginationButtons();
        SearchTextBox.Text = ""; // Clear search field
        _currentFilter = null; // Clear any active filter

        // Filter favorites for the selected system and store them in _currentSearchResults
        var favoriteGames = GetFavoriteGamesForSelectedSystem();
        if (favoriteGames.Count != 0)
        {
            _currentSearchResults = favoriteGames.ToList(); // Store only favorite games in _currentSearchResults
            return LoadGameFilesAsync(null, "FAVORITES"); // Call LoadGameFilesAsync
        }
        else
        {
            AddNoFilesMessage();
            MessageBoxLibrary.NoFavoriteFoundMessageBox();
        }

        return Task.CompletedTask;
    }

    private async Task FeelingLucky_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Change filter to ShowAll
            _settings.ShowGames = "ShowAll";
            _settings.Save();
            ApplyShowGamesSetting();

            // Check if a system is selected
            if (SystemComboBox.SelectedItem == null)
            {
                MessageBoxLibrary.PleaseSelectASystemBeforeMessageBox();
                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig == null)
            {
                return;
            }

            try
            {
                // Determine which game list to use
                List<string> gameFiles;

                // Otherwise, use the cached files for the selected system
                if (_cachedFiles is { Count: > 0 })
                {
                    gameFiles = _cachedFiles;
                }
                // If needed, rescan the system folder
                else
                {
                    var systemFolderPath = selectedConfig.SystemFolder;
                    var fileExtensions = selectedConfig.FileFormatsToSearch.Select(static ext => $"*.{ext}").ToList();
                    gameFiles = await GetFilePaths.GetFilesAsync(systemFolderPath, fileExtensions);
                }

                // Check if we have any games after filtering
                if (gameFiles.Count == 0)
                {
                    MessageBoxLibrary.NoGameFoundInTheRandomSelectionMessageBox();
                    return;
                }

                // Randomly select a game
                var random = new Random();
                var randomIndex = random.Next(0, gameFiles.Count);
                var selectedGame = gameFiles[randomIndex];

                // Reset letter selection in the UI and current search
                _filterMenu.DeselectLetter();
                SearchTextBox.Text = "";
                _currentSearchResults = [selectedGame];

                // Load just this game to display it
                await LoadGameFilesAsync(null, "RANDOM_SELECTION");

                // If in list view, select the game in the DataGrid
                if (_settings.ViewMode != "ListView" || GameDataGrid.Items.Count <= 0) return;

                GameDataGrid.SelectedIndex = 0;
                GameDataGrid.ScrollIntoView(GameDataGrid.SelectedItem);
                GameDataGrid.Focus();
            }
            catch (Exception)
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the Feeling Lucky feature.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    public void RefreshGameListAfterPlay(string fileName, string systemName)
    {
        try
        {
            // Only update if in ListView mode
            if (_settings.ViewMode != "ListView" || GameListItems.Count == 0)
                return;

            // Re-load the latest play history data
            _playHistoryManager = PlayHistoryManager.LoadPlayHistory();

            // Get the current playtime from history
            var historyItem = _playHistoryManager.PlayHistoryList
                .FirstOrDefault(h => h.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                     h.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (historyItem == null)
                return;

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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private void InitializeControllerDetection()
    {
        _controllerCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // Check every 5 seconds
        };
        _controllerCheckTimer.Tick += GamePadControllerCheckTimer_Tick;
        _controllerCheckTimer.Start();
    }

    private static void GamePadControllerCheckTimer_Tick(object sender, EventArgs e)
    {
        GamePadController.Instance2.CheckAndReconnectControllers();
    }

    private static void OpenUpdateHistory()
    {
        var updateHistoryWindow = new UpdateHistoryWindow();
        updateHistoryWindow.Show();
    }

    private void SaveApplicationSettings()
    {
        // Save application's window state
        _settings.MainWindowWidth = (int)Width;
        _settings.MainWindowHeight = (int)Height;
        _settings.MainWindowTop = (int)Top;
        _settings.MainWindowLeft = (int)Left;
        _settings.MainWindowState = WindowState.ToString();

        // Save application's current state
        _settings.ThumbnailSize = _gameButtonFactory.ImageHeight;
        _settings.GamesPerPage = _filesPerPage;
        _settings.ShowGames = _settings.ShowGames;
        _settings.EnableGamePadNavigation = ToggleGamepad.IsChecked;
        _settings.EnableFuzzyMatching = ToggleFuzzyMatching.IsChecked; // Save fuzzy matching state

        // Save theme settings
        var detectedTheme = ThemeManager.Current.DetectTheme(this);
        if (detectedTheme != null)
        {
            _settings.BaseTheme = detectedTheme.BaseColorScheme;
            _settings.AccentColor = detectedTheme.ColorScheme;
        }

        _settings.Save();
    }

    private List<string> GetFavoriteGamesForSelectedSystem()
    {
        // Reload favorites to ensure we have the latest data
        _favoritesManager = FavoritesManager.LoadFavorites();

        var selectedSystem = SystemComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedSystem))
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        // Retrieve the system configuration for the selected system
        var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        if (selectedConfig == null)
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        var systemFolderPath = selectedConfig.SystemFolder;

        // Filter the favorites and build the full file path for each favorite game
        var favoriteGamePaths = _favoritesManager.FavoriteList
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => Path.Combine(systemFolderPath, fav.FileName))
            .ToList();

        return favoriteGamePaths;
    }

    private static Task ShowPleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Show);
        });
    }

    private static Task ClosePleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Close);
        });
    }

    private void GameListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GameDataGrid.SelectedItem is not GameListFactory.GameListViewItem selectedItem) return;

        var gameListViewFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);
        gameListViewFactory.HandleSelectionChanged(selectedItem);
    }

    // Used on the Game List Mode
    private async void GameListDoubleClickOnSelectedItem(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (GameDataGrid.SelectedItem is GameListFactory.GameListViewItem selectedItem)
            {
                // Delegate the double-click handling to GameListFactory
                await _gameListFactory.HandleDoubleClick(selectedItem);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error while using the method GameListDoubleClickOnSelectedItem.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    private async void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            SearchTextBox.Text = ""; // Empty search field
            EmulatorComboBox.ItemsSource = null; // Null selected emulator
            EmulatorComboBox.SelectedIndex = -1; // No emulator selected
            PreviewImage.Source = null; // Empty PreviewImage

            // Clear search results
            _currentSearchResults.Clear();

            // Hide ListView
            GameFileGrid.Visibility = Visibility.Visible;
            ListViewPreviewArea.Visibility = Visibility.Collapsed;

            if (SystemComboBox.SelectedItem != null)
            {
                var selectedSystem = SystemComboBox.SelectedItem?.ToString();
                var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

                if (selectedSystem == null)
                {
                    // Notify developer
                    const string errorMessage = "Selected system is null.";
                    var ex = new Exception(errorMessage);
                    _ = LogErrors.LogErrorAsync(ex, errorMessage);

                    // Notify user
                    MessageBoxLibrary.InvalidSystemConfigMessageBox();

                    return;
                }

                if (selectedConfig != null)
                {
                    // Populate EmulatorComboBox with the emulators for the selected system
                    EmulatorComboBox.ItemsSource = selectedConfig.Emulators.Select(static emulator => emulator.EmulatorName).ToList();

                    // Select the first emulator
                    if (EmulatorComboBox.Items.Count > 0)
                    {
                        EmulatorComboBox.SelectedIndex = 0;
                    }

                    // Update the selected system property
                    SelectedSystem = selectedSystem;

                    // Retrieve the playtime for the selected system
                    var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
                    PlayTime = systemPlayTime != null ? systemPlayTime.PlayTime : "00:00:00";

                    // Display the system info
                    var systemFolderPath = selectedConfig.SystemFolder;
                    var fileExtensions = selectedConfig.FileFormatsToSearch.Select(static ext => $"{ext}").ToList();
                    var gameCount = CountFiles.CountFilesAsync(systemFolderPath, fileExtensions);

                    // Display SystemInfo for that system
                    await DisplaySystemInformation.DisplaySystemInfo(systemFolderPath, await gameCount, selectedConfig, _gameFileGrid);

                    // Update Image Folder and Rom Folder Variables
                    _selectedRomFolder = selectedConfig.SystemFolder;
                    _selectedImageFolder = string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder)
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName)
                        : selectedConfig.SystemImageFolder;

                    // Call DeselectLetter to clear any selected letter
                    _filterMenu.DeselectLetter();

                    ResetPaginationButtons();

                    // Load files from cache or rescan if needed
                    _cachedFiles = await _cacheManager.LoadSystemFilesAsync(selectedSystem, systemFolderPath, fileExtensions, await gameCount);
                }
                else
                {
                    AddNoSystemMessage();
                }
            }
            else
            {
                AddNoSystemMessage();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SystemComboBox_SelectionChanged.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.InvalidSystemConfigMessageBox();
        }
    }

    private void AddNoSystemMessage()
    {
        var noSystemMessage = (string)Application.Current.TryFindResource("NoSystemMessage") ?? "Please select a System";

        // Check the current view mode
        if (_settings.ViewMode == "GridView")
        {
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noSystemMessage}",
                Padding = new Thickness(10)
            });
        }
        else
        {
            // For List view, clear GameListItems
            GameListItems.Clear();
            GameListItems.Add(new GameListFactory.GameListViewItem
            {
                FileName = noSystemMessage,
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _filterMenu.DeselectLetter();
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
            GameListItems.Add(new GameListFactory.GameListViewItem
            {
                FileName = noGamesMatched,
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _filterMenu.DeselectLetter();
    }

    public async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null)
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

        try
        {
            if (CheckIfSystemComboBoxIsNotNull()) return;

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig == null)
            {
                // Notify developer
                const string contextMessage = "Invalid system configuration.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                return;
            }

            // Create allFiles list
            List<string> allFiles;

            // If we are in "FAVORITES" mode, use '_currentSearchResults'
            if (searchQuery == "FAVORITES" && _currentSearchResults != null && _currentSearchResults.Count != 0)
            {
                allFiles = _currentSearchResults;
            }

            // Regular behavior: load files based on startLetter or searchQuery
            else
            {
                // Attempt to use the cached file list first
                _cachedFiles = _cacheManager.GetCachedFiles(selectedSystem);

                // Recount the number of files in the system folder
                var systemFolderPath = selectedConfig.SystemFolder;
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(static ext => $"*.{ext}").ToList();
                var gameCount = CountFiles.CountFilesAsync(systemFolderPath, fileExtensions);
                var cachedFilesCount = _cachedFiles?.Count ?? 0;
                if (cachedFilesCount != await gameCount)
                {
                    // If the cached file list is not up to date, rescan the system folder
                    _cachedFiles = await _cacheManager.LoadSystemFilesAsync(selectedSystem, systemFolderPath, fileExtensions, await gameCount);
                }

                if (_cachedFiles is { Count: > 0 })
                {
                    allFiles = _cachedFiles;
                }
                else
                {
                    // Fall back to scanning the folder if no cache is available
                    allFiles = await GetFilePaths.GetFilesAsync(systemFolderPath, fileExtensions);
                }

                // Filter by TopMenu Letter if specified
                if (!string.IsNullOrWhiteSpace(startLetter))
                {
                    allFiles = await GetFilePaths.FilterFilesAsync(allFiles, startLetter);
                }

                // Process search query (from SearchBox)
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    // If _currentSearchResults already exists, use it
                    if (_currentSearchResults != null && _currentSearchResults.Count != 0)
                    {
                        allFiles = _currentSearchResults;
                    }
                    else
                    {
                        // Check if the system is MAME-based
                        var systemIsMame = selectedConfig.SystemIsMame;

                        // If a system is MAME-based, use the pre-built _mameLookup dictionary for faster lookups.
                        if (systemIsMame && _mameLookup != null)
                        {
                            // Use a case-insensitive comparison.
                            var lowerQuery = searchQuery.ToLowerInvariant();
                            allFiles = await Task.Run(() =>
                                allFiles.FindAll(file =>
                                {
                                    var fileName = Path.GetFileNameWithoutExtension(file);
                                    // Check if the filename contains the search query.
                                    var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                    if (filenameMatch)
                                        return true;

                                    // Lookup in the dictionary.
                                    if (_mameLookup.TryGetValue(fileName, out var description))
                                    {
                                        return description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                    }

                                    return false;
                                }));
                        }
                        else
                        {
                            // For non-MAME systems, use the original filtering by filename.
                            allFiles = await Task.Run(() =>
                                allFiles.FindAll(file =>
                                {
                                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                                    return fileNameWithoutExtension.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                                }));
                        }

                        // Create the search results
                        _currentSearchResults = allFiles;
                    }
                }
            }

            // Sort the collection of files
            allFiles.Sort();

            // Count the collection of files
            _totalFiles = allFiles.Count;

            // Calculate the indices of files displayed on the current page
            var startIndex = (_currentPage - 1) * _filesPerPage + 1; // +1 because we are dealing with a 1-based index for displaying
            var endIndex = startIndex + _filesPerPage; // Actual number of files loaded on this page
            if (endIndex > _totalFiles)
            {
                endIndex = _totalFiles;
            }

            // Pagination related
            if (_totalFiles > _paginationThreshold)
            {
                // Enable pagination and adjust file list based on the current page
                allFiles = allFiles.Skip((_currentPage - 1) * _filesPerPage).Take(_filesPerPage).ToList();

                // Update or create pagination controls
                InitializePaginationButtons();
            }

            // Display message if the number of files == 0
            if (allFiles.Count == 0)
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
                TotalFilesLabel.Content = allFiles.Count == 0 ? $"{displayingfiles0To} {endIndex} {outOf} {_totalFiles} {total}" : $"{displayingfiles} {startIndex} {to} {endIndex} {outOf} {_totalFiles} {total}"
            );

            // Reload the FavoritesConfig
            _favoritesManager = FavoritesManager.LoadFavorites();

            // Initialize GameButtonFactory with updated FavoritesConfig
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _gameFileGrid, this);

            // Initialize GameListFactory with updated FavoritesConfig
            var gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);

            // Display files based on ViewMode
            foreach (var filePath in allFiles)
            {
                if (_settings.ViewMode == "GridView")
                {
                    var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedConfig);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // ListView
                {
                    var gameListViewItem = await gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedConfig);
                    await Dispatcher.InvokeAsync(() => GameListItems.Add(gameListViewItem));
                }
            }

            // Apply visibility settings to each button based on _settings.ShowGames
            ApplyShowGamesSetting();

            // Update the UI to reflect the current pagination status
            UpdatePaginationButtons();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
    }

    private bool CheckIfSystemComboBoxIsNotNull()
    {
        if (SystemComboBox.SelectedItem != null) return false;

        AddNoSystemMessage();
        return true;
    }

    #region Menu Items

    private void SetViewMode(string viewMode)
    {
        if (viewMode == "ListView")
        {
            ListView.IsChecked = true;
            GridView.IsChecked = false;
        }
        else
        {
            GridView.IsChecked = true;
            ListView.IsChecked = false;
        }
    }

    private void SetLanguageAndCheckMenu(string languageCode)
    {
        LanguageArabic.IsChecked = languageCode == "ar";
        LanguageBengali.IsChecked = languageCode == "bn";
        LanguageGerman.IsChecked = languageCode == "de";
        LanguageEnglish.IsChecked = languageCode == "en";
        LanguageSpanish.IsChecked = languageCode == "es";
        LanguageFrench.IsChecked = languageCode == "fr";
        LanguageHindi.IsChecked = languageCode == "hi";
        LanguageIndonesianMalay.IsChecked = languageCode == "id";
        LanguageItalian.IsChecked = languageCode == "it";
        LanguageJapanese.IsChecked = languageCode == "ja";
        LanguageKorean.IsChecked = languageCode == "ko";
        LanguageDutch.IsChecked = languageCode == "nl";
        LanguagePortugueseBr.IsChecked = languageCode == "pt-br";
        LanguageRussian.IsChecked = languageCode == "ru";
        LanguageTurkish.IsChecked = languageCode == "tr";
        LanguageUrdu.IsChecked = languageCode == "ur";
        LanguageVietnamese.IsChecked = languageCode == "vi";
        LanguageChineseSimplified.IsChecked = languageCode == "zh-hans";
        LanguageChineseTraditional.IsChecked = languageCode == "zh-hant";
    }

    private void EasyMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Ensure pagination is reset at the beginning
            ResetPaginationButtons();

            // Clear SearchTextBox
            SearchTextBox.Text = "";

            // Update current filter
            _currentFilter = null;

            // Empty SystemComboBox
            _selectedSystem = null;
            SystemComboBox.SelectedItem = null;
            var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
            SelectedSystem = nosystemselected;
            PlayTime = "00:00:00";

            AddNoSystemMessage();

            EasyModeWindow editSystemEasyModeAddSystemWindow = new();
            editSystemEasyModeAddSystemWindow.ShowDialog();

            // ReLoad and Sort _systemConfigs
            _systemConfigs = SystemManager.LoadSystemConfigs();
            var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName).OrderBy(static name => name).ToList();
            SystemComboBox.ItemsSource = sortedSystemNames;

            // Refresh GameList
            // await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method EasyMode_Click.");
        }
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Ensure pagination is reset at the beginning
            ResetPaginationButtons();

            // Clear SearchTextBox
            SearchTextBox.Text = "";

            // Update current filter
            _currentFilter = null;

            // Empty SystemComboBox
            _selectedSystem = null;
            SystemComboBox.SelectedItem = null;
            var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
            SelectedSystem = nosystemselected;
            PlayTime = "00:00:00";

            AddNoSystemMessage();

            EditSystemWindow editSystemWindow = new(_settings);
            editSystemWindow.ShowDialog();

            // ReLoad and Sort _systemConfigs
            _systemConfigs = SystemManager.LoadSystemConfigs();
            var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName).OrderBy(static name => name).ToList();
            SystemComboBox.ItemsSource = sortedSystemNames;

            // Refresh GameList
            // await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ExpertMode_Click.");
        }
    }

    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ResetUi();

            DownloadImagePackWindow downloadImagePack = new();
            downloadImagePack.ShowDialog();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method DownloadImagePack_Click.");
        }
    }

    public void LoadOrReloadSystemConfig()
    {
        _systemConfigs = SystemManager.LoadSystemConfigs();
        var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName).OrderBy(static name => name).ToList();
        SystemComboBox.ItemsSource = sortedSystemNames;
    }

    public void ResetUi()
    {
        // Ensure pagination is reset at the beginning
        ResetPaginationButtons();

        // Clear SearchTextBox
        SearchTextBox.Text = "";

        // Update current filter
        _currentFilter = null;

        // Empty SystemComboBox
        _selectedSystem = null;
        SystemComboBox.SelectedItem = null;
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        SelectedSystem = nosystemselected;
        PlayTime = "00:00:00";

        AddNoSystemMessage();
    }

    private async void EditLinks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetLinksWindow editLinksWindow = new(_settings);
            editLinksWindow.ShowDialog();

            // Refresh GameList
            await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method EditLinks_Click.");
        }
    }

    private void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        try
        {
            // Update the settings
            _settings.EnableGamePadNavigation = menuItem.IsChecked;

            _settings.Save();

            // Start or stop the GamePadController
            if (menuItem.IsChecked)
            {
                GamePadController.Instance2.Start();
            }
            else
            {
                GamePadController.Instance2.Stop();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to toggle gamepad.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ToggleGamepadFailureMessageBox();
        }
    }

    private void SetGamepadDeadZone_Click(object sender, RoutedEventArgs e)
    {
        SetGamepadDeadZoneWindow setGamepadDeadZoneWindow = new(_settings);
        setGamepadDeadZoneWindow.ShowDialog();

        // Update the GamePadController dead zone settings from SettingsManager
        GamePadController.Instance2.DeadZoneX = _settings.DeadZoneX;
        GamePadController.Instance2.DeadZoneY = _settings.DeadZoneY;

        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Stop();
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }
    }

    private async void ToggleFuzzyMatching_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            try
            {
                _settings.EnableFuzzyMatching = menuItem.IsChecked;
                _settings.Save();

                // Re-load game files to apply the new setting
                await LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Failed to toggle fuzzy matching.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ToggleFuzzyMatchingFailureMessageBox();
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in method ToggleFuzzyMatching_Click");
        }
    }

    private void SetFuzzyMatchingThreshold_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Pass the current settings manager to the dialog
            var setThresholdWindow = new SetFuzzyMatchingWindow(_settings);
            setThresholdWindow.ShowDialog(); // Use ShowDialog() to make it modal

            // After the dialog closes, the settings are saved within the dialog.
            // No need to explicitly save here.
            // Re-load game files to apply the new threshold if fuzzy matching is enabled
            if (_settings.EnableFuzzyMatching)
            {
                // Use _ = to suppress the warning about not awaiting the Task
                _ = LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to open Set Fuzzy Matching Threshold window.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SetFuzzyMatchingThresholdFailureMessageBox();
        }
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        SupportWindow supportRequestWindow = new();
        supportRequestWindow.ShowDialog();
    }

    private void Donate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://www.purelogiccode.com/Donate",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Unable to open the Donation Link from the menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorOpeningDonationLinkMessageBox();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        AboutWindow aboutWindow = new();
        aboutWindow.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowAllGames_Click(object sender, RoutedEventArgs e)
    {
        // Show all games regardless of cover
        UpdateGameVisibility(static _ => true);
        UpdateShowGamesSetting("ShowAll");
        UpdateMenuCheckMarks("ShowAll");
    }

    private void ShowGamesWithCover_Click(object sender, RoutedEventArgs e)
    {
        // Show games that have covers (not using the default image)
        UpdateGameVisibility(static btn =>
        {
            if (btn.Tag is GameButtonTag tag)
                return !tag.IsDefaultImage;

            return false;
        });
        UpdateShowGamesSetting("ShowWithCover");
        UpdateMenuCheckMarks("ShowWithCover");
    }

    private void ShowGamesWithoutCover_Click(object sender, RoutedEventArgs e)
    {
        // Show games that are using the default image (no cover available)
        UpdateGameVisibility(static btn =>
        {
            if (btn.Tag is GameButtonTag tag)
                return tag.IsDefaultImage;

            return false;
        });
        UpdateShowGamesSetting("ShowWithoutCover");
        UpdateMenuCheckMarks("ShowWithoutCover");
    }

    private void UpdateGameVisibility(Func<Button, bool> visibilityCondition)
    {
        foreach (var child in _gameFileGrid.Children)
        {
            if (child is Button btn)
            {
                btn.Visibility = visibilityCondition(btn) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private void UpdateShowGamesSetting(string showGames)
    {
        _settings.ShowGames = showGames;
        _settings.Save();
    }

    private void UpdateMenuCheckMarks(string selectedMenu)
    {
        ShowAll.IsChecked = selectedMenu == "ShowAll";
        ShowWithCover.IsChecked = selectedMenu == "ShowWithCover";
        ShowWithoutCover.IsChecked = selectedMenu == "ShowWithoutCover";
    }

    private async void ButtonSize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var sizeText = clickedItem.Name.Replace("Size", "");

            if (!int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out var newSize)) return;

            _gameButtonFactory.ImageHeight = newSize; // Update the image height
            _settings.ThumbnailSize = newSize;
            _settings.Save();

            UpdateThumbnailSizeCheckMarks(newSize);

            // Reload List of Games
            await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in method ButtonSize_Click.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    private async void ButtonAspectRatio_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var aspectRatio = clickedItem.Name;
            _settings.ButtonAspectRatio = aspectRatio;
            _settings.Save();

            UpdateButtonAspectRatioCheckMarks(aspectRatio);

            // Reload List of Games
            await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in method ButtonAspectRatio_Click";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    private async void GamesPerPage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var pageText = clickedItem.Name.Replace("Page", "");
            if (!int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out var newPage)) return;

            _filesPerPage = newPage;
            _paginationThreshold = newPage;
            _settings.GamesPerPage = newPage;

            _settings.Save();
            UpdateNumberOfGamesPerPageCheckMarks(newPage);

            // Refresh GameList
            await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method GamesPerPage_Click.");
        }
    }

    private void GlobalSearch_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();

        var globalSearchWindow = new GlobalSearchWindow(_systemConfigs, _machines, _mameLookup, _settings, _favoritesManager, this);
        globalSearchWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void GlobalStats_Click(object sender, RoutedEventArgs e)
    {
        var globalStatsWindow = new GlobalStatsWindow(_systemConfigs);
        globalStatsWindow.Show();
    }

    private void Favorites_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();

        var favoritesWindow = new FavoritesWindow(_settings, _systemConfigs, _machines, _favoritesManager, this);
        favoritesWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void PlayHistory_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();

        var playHistoryWindow = new PlayHistoryWindow(_systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);
        playHistoryWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }


    private void UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
        Size100.IsChecked = selectedSize == 100;
        Size150.IsChecked = selectedSize == 150;
        Size200.IsChecked = selectedSize == 200;
        Size250.IsChecked = selectedSize == 250;
        Size300.IsChecked = selectedSize == 300;
        Size350.IsChecked = selectedSize == 350;
        Size400.IsChecked = selectedSize == 400;
        Size450.IsChecked = selectedSize == 450;
        Size500.IsChecked = selectedSize == 500;
        Size550.IsChecked = selectedSize == 550;
        Size600.IsChecked = selectedSize == 600;
        Size650.IsChecked = selectedSize == 650;
        Size700.IsChecked = selectedSize == 700;
        Size750.IsChecked = selectedSize == 750;
        Size800.IsChecked = selectedSize == 800;
    }

    private void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize)
    {
        Page100.IsChecked = selectedSize == 100;
        Page200.IsChecked = selectedSize == 200;
        Page300.IsChecked = selectedSize == 300;
        Page400.IsChecked = selectedSize == 400;
        Page500.IsChecked = selectedSize == 500;
    }

    private void UpdateShowGamesCheckMarks(string selectedValue)
    {
        ShowAll.IsChecked = selectedValue == "ShowAll";
        ShowWithCover.IsChecked = selectedValue == "ShowWithCover";
        ShowWithoutCover.IsChecked = selectedValue == "ShowWithoutCover";
    }

    private void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        Square.IsChecked = selectedValue == "Square";
        Wider.IsChecked = selectedValue == "Wider";
        SuperWider.IsChecked = selectedValue == "SuperWider";
        Taller.IsChecked = selectedValue == "Taller";
        SuperTaller.IsChecked = selectedValue == "SuperTaller";
    }

    private async void ChangeViewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Equals(sender, GridView))
            {
                GridView.IsChecked = true;
                ListView.IsChecked = false;
                _settings.ViewMode = "GridView";

                GameFileGrid.Visibility = Visibility.Visible;
                ListViewPreviewArea.Visibility = Visibility.Collapsed;

                // Ensure pagination is reset at the beginning
                ResetPaginationButtons();

                // Clear SearchTextBox
                SearchTextBox.Text = "";

                // Update current filter
                _currentFilter = null;

                // Empty SystemComboBox
                _selectedSystem = null;
                SystemComboBox.SelectedItem = null;
                var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
                SelectedSystem = nosystemselected;
                PlayTime = "00:00:00";

                AddNoSystemMessage();
            }
            else if (Equals(sender, ListView))
            {
                GridView.IsChecked = false;
                ListView.IsChecked = true;
                _settings.ViewMode = "ListView";

                GameFileGrid.Visibility = Visibility.Collapsed;
                ListViewPreviewArea.Visibility = Visibility.Visible;

                // Ensure pagination is reset at the beginning
                ResetPaginationButtons();

                // Clear SearchTextBox
                SearchTextBox.Text = "";

                // Update current filter
                _currentFilter = null;

                // Empty SystemComboBox
                _selectedSystem = null;
                PreviewImage.Source = null;
                SystemComboBox.SelectedItem = null;

                // Set selected system
                var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
                SelectedSystem = nosystemselected;
                PlayTime = "00:00:00";

                AddNoSystemMessage();

                await LoadGameFilesAsync();
            }

            _settings.Save(); // Save the updated ViewMode
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error while using the method ChangeViewMode_Click.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorChangingViewModeMessageBox();
        }
    }

    private void ApplyShowGamesSetting()
    {
        switch (_settings.ShowGames)
        {
            case "ShowAll":
                ShowAllGames_Click(ShowAll, null);
                break;
            case "ShowWithCover":
                ShowGamesWithCover_Click(ShowWithCover, null);
                break;
            case "ShowWithoutCover":
                ShowGamesWithoutCover_Click(ShowWithoutCover, null);
                break;
        }
    }

    private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        var selectedLanguage = menuItem.Name switch
        {
            "LanguageArabic" => "ar",
            "LanguageBengali" => "bn",
            "LanguageGerman" => "de",
            "LanguageEnglish" => "en",
            "LanguageSpanish" => "es",
            "LanguageFrench" => "fr",
            "LanguageHindi" => "hi",
            "LanguageIndonesianMalay" => "id",
            "LanguageItalian" => "it",
            "LanguageJapanese" => "ja",
            "LanguageKorean" => "ko",
            "LanguageDutch" => "nl",
            "LanguagePortugueseBr" => "pt-br",
            "LanguageRussian" => "ru",
            "LanguageTurkish" => "tr",
            "LanguageUrdu" => "ur",
            "LanguageVietnamese" => "vi",
            "LanguageChineseSimplified" => "zh-hans",
            "LanguageChineseTraditional" => "zh-hant",
            _ => "en"
        };

        _settings.Language = selectedLanguage;
        _settings.Save();

        // Update checked status
        SetLanguageAndCheckMenu(selectedLanguage);

        SaveApplicationSettings();

        QuitApplication.RestartApplication();
    }

    #endregion

    #region Theme Options

    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        var baseTheme = menuItem.Name;
        var currentAccent = ThemeManager.Current.DetectTheme(this)?.ColorScheme;
        App.ChangeTheme(baseTheme, currentAccent);

        UncheckBaseThemes();
        menuItem.IsChecked = true;
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        var accentColor = menuItem.Name;
        var currentBaseTheme = ThemeManager.Current.DetectTheme(this)?.BaseColorScheme;
        App.ChangeTheme(currentBaseTheme, accentColor);

        UncheckAccentColors();
        menuItem.IsChecked = true;
    }

    private void UncheckBaseThemes()
    {
        Light.IsChecked = false;
        Dark.IsChecked = false;
    }

    private void UncheckAccentColors()
    {
        Red.IsChecked = false;
        Green.IsChecked = false;
        Blue.IsChecked = false;
        Purple.IsChecked = false;
        Orange.IsChecked = false;
        Lime.IsChecked = false;
        Emerald.IsChecked = false;
        Teal.IsChecked = false;
        Cyan.IsChecked = false;
        Cobalt.IsChecked = false;
        Indigo.IsChecked = false;
        Violet.IsChecked = false;
        Pink.IsChecked = false;
        Magenta.IsChecked = false;
        Crimson.IsChecked = false;
        Amber.IsChecked = false;
        Yellow.IsChecked = false;
        Brown.IsChecked = false;
        Olive.IsChecked = false;
        Steel.IsChecked = false;
        Mauve.IsChecked = false;
        Taupe.IsChecked = false;
        Sienna.IsChecked = false;
    }

    private void SetCheckedTheme(string baseTheme, string accentColor)
    {
        switch (baseTheme)
        {
            case "Light":
                Light.IsChecked = true;
                break;
            case "Dark":
                Dark.IsChecked = true;
                break;
        }

        switch (accentColor)
        {
            case "Red":
                Red.IsChecked = true;
                break;
            case "Green":
                Green.IsChecked = true;
                break;
            case "Blue":
                Blue.IsChecked = true;
                break;
            case "Purple":
                Purple.IsChecked = true;
                break;
            case "Orange":
                Orange.IsChecked = true;
                break;
            case "Lime":
                Lime.IsChecked = true;
                break;
            case "Emerald":
                Emerald.IsChecked = true;
                break;
            case "Teal":
                Teal.IsChecked = true;
                break;
            case "Cyan":
                Cyan.IsChecked = true;
                break;
            case "Cobalt":
                Cobalt.IsChecked = true;
                break;
            case "Indigo":
                Indigo.IsChecked = true;
                break;
            case "Violet":
                Violet.IsChecked = true;
                break;
            case "Pink":
                Pink.IsChecked = true;
                break;
            case "Magenta":
                Magenta.IsChecked = true;
                break;
            case "Crimson":
                Crimson.IsChecked = true;
                break;
            case "Amber":
                Amber.IsChecked = true;
                break;
            case "Yellow":
                Yellow.IsChecked = true;
                break;
            case "Brown":
                Brown.IsChecked = true;
                break;
            case "Olive":
                Olive.IsChecked = true;
                break;
            case "Steel":
                Steel.IsChecked = true;
                break;
            case "Mauve":
                Mauve.IsChecked = true;
                break;
            case "Taupe":
                Taupe.IsChecked = true;
                break;
            case "Sienna":
                Sienna.IsChecked = true;
                break;
        }
    }

    #endregion

    #region Pagination

    private void ResetPaginationButtons()
    {
        _prevPageButton.IsEnabled = false;
        _nextPageButton.IsEnabled = false;
        _currentPage = 1;
        Scroller.ScrollToTop();
        TotalFilesLabel.Content = null;
    }

    private void InitializePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        Scroller.ScrollToTop();
    }

    private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_currentPage <= 1) return;

            _currentPage--;
            if (_currentSearchResults.Count != 0)
            {
                await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
            }
            else
            {
                await LoadGameFilesAsync(_currentFilter);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Previous page button error.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.NavigationButtonErrorMessageBox();
        }
    }

    private async void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);

            if (_currentPage >= totalPages) return;

            _currentPage++;
            if (_currentSearchResults.Count != 0)
            {
                await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
            }
            else
            {
                await LoadGameFilesAsync(_currentFilter);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Next page button error.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.NavigationButtonErrorMessageBox();
        }
    }

    private void UpdatePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
    }

    #endregion

    #region MainWindow Search

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ExecuteSearch();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SearchButton_Click.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
            {
                await ExecuteSearch();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method SearchTextBox_KeyDown.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async Task ExecuteSearch()
    {
        ResetPaginationButtons();

        _currentSearchResults.Clear();

        // Call DeselectLetter to clear any selected letter
        _filterMenu.DeselectLetter();

        var searchQuery = SearchTextBox.Text.Trim();

        if (SystemComboBox.SelectedItem == null)
        {
            // Notify user
            MessageBoxLibrary.SelectSystemBeforeSearchMessageBox();

            return;
        }

        if (string.IsNullOrEmpty(searchQuery))
        {
            // Notify user
            MessageBoxLibrary.EnterSearchQueryMessageBox();

            return;
        }

        var searchingpleasewait = (string)Application.Current.TryFindResource("Searchingpleasewait") ?? "Searching, please wait...";
        var pleaseWaitWindow = new PleaseWaitWindow(searchingpleasewait);
        await ShowPleaseWaitWindowAsync(pleaseWaitWindow);

        try
        {
            await LoadGameFilesAsync(null, searchQuery);
        }
        finally
        {
            await ClosePleaseWaitWindowAsync(pleaseWaitWindow);
        }
    }

    #endregion

    #region Launch Tools

    private void CreateBatchFilesForXbox360XBLAGames_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForXbox360XBLAGames_Click();
    }

    private void BatchConvertIsoToXiso_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertIsoToXiso_Click();
    }

    private void BatchConvertToCHD_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertToCHD_Click();
    }

    private void BatchConvertToCompressedFile_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertToCompressedFile_Click();
    }

    private void BatchVerifyCHDFiles_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchVerifyCHDFiles_Click();
    }

    private void BatchVerifyCompressedFiles_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchVerifyCompressedFiles_Click();
    }

    private void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForPS3Games_Click();
    }

    private void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForScummVMGames_Click();
    }

    private void CreateBatchFilesForSegaModel3Games_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForSegaModel3Games_Click();
    }

    private void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForWindowsGames_Click();
    }

    private void FindRomCover_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();
        LaunchTools.FindRomCoverLaunch_Click(_selectedImageFolder, _selectedRomFolder);
    }

    #endregion

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SaveApplicationSettings();

        // Delete temp folders and files before close
        CleanSimpleLauncherFolder.CleanupTrash();

        Dispose();

        // Stop Controller Timer
        StopControllerTimer();
    }

    private void StopControllerTimer()
    {
        if (_controllerCheckTimer == null) return;

        _controllerCheckTimer.Stop();
        _controllerCheckTimer = null;
    }

    public void Dispose()
    {
        // Dispose gamepad resources
        GamePadController.Instance2.Stop();
        GamePadController.Instance2.Dispose();

        // Dispose tray icon resources
        _trayIconManager?.Dispose();

        // Dispose instances of HttpClient
        Stats.DisposeHttpClient();
        UpdateChecker.DisposeHttpClient();
        SupportWindow.DisposeHttpClient();
        LogErrors.DisposeHttpClient();

        // Stop and dispose timers
        if (_controllerCheckTimer != null)
        {
            _controllerCheckTimer.Stop();
            _controllerCheckTimer = null;
        }

        // Dispose TrayIconManager
        _trayIconManager?.Dispose();

        // Clean up collections
        GameListItems?.Clear();
        _cachedFiles?.Clear();
        _currentSearchResults?.Clear();
        _systemConfigs?.Clear();

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }
}

