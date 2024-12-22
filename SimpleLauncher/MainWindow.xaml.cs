using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ControlzEx.Theming;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Markup;

namespace SimpleLauncher;

public partial class MainWindow : INotifyPropertyChanged
{
    public ObservableCollection<GameListFactory.GameListViewItem> GameListItems { get; set; } = new();
        
    // Logic to update the System Name and PlayTime in the Statusbar
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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Declare _gameListFactory
    private readonly GameListFactory _gameListFactory;
        
    // tray icon
    private NotifyIcon _trayIcon;
    private ContextMenuStrip _trayMenu;
        
    // pagination related
    private int _currentPage = 1;
    private int _filesPerPage;
    private int _totalFiles;
    private int _paginationThreshold;
    private readonly Button _nextPageButton;
    private readonly Button _prevPageButton;
    private string _currentFilter;
    private List<string> _currentSearchResults = new();
        
    // Instance variables
    private readonly List<SystemConfig> _systemConfigs;
    private readonly LetterNumberMenu _letterNumberMenu;
    private readonly WrapPanel _gameFileGrid;
    private GameButtonFactory _gameButtonFactory;
    private readonly SettingsConfig _settings;
    private readonly List<MameConfig> _machines;
    private FavoritesConfig _favoritesConfig;
    private readonly FavoritesManager _favoritesManager;
        
    // Selected Image folder and Rom folder
    private string _selectedImageFolder;
    private string _selectedRomFolder;
        
    public MainWindow()
    {
        InitializeComponent();
            
        DataContext = this; // Ensure the DataContext is set to the current MainWindow instance for binding

        // Tray icon
        InitializeTrayIcon();
            
        // Load settings.xml
        _settings = new SettingsConfig();
        
        // Apply language
        ApplyLanguage(_settings.Language);
        SetLanguageMenuChecked(_settings.Language);
            
        // Set the initial theme
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
            
        // Load mame.xml
        _machines = MameConfig.LoadFromXml();
            
        // Load system.xml
        try
        {
            _systemConfigs = SystemConfig.LoadSystemConfigs();
            // Sort the system names in alphabetical order
            var sortedSystemNames = _systemConfigs.Select(config => config.SystemName).OrderBy(name => name).ToList();

            SystemComboBox.ItemsSource = sortedSystemNames;
        }
        catch (Exception ex)
        {
            string contextMessage = $"'system.xml' is corrupted.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("The file 'system.xml' is corrupted.\n\n" +
                            "You need to fix it manually or delete it.\n\n" +
                            "The application will be shutdown.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Shutdown current application instance
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        // Apply settings to application from settings.xml
        EnableGamePadNavigation.IsChecked = _settings.EnableGamePadNavigation;
        UpdateMenuCheckMarks(_settings.ThumbnailSize);
        UpdateMenuCheckMarks2(_settings.GamesPerPage);
        UpdateMenuCheckMarks3(_settings.ShowGames);
        _filesPerPage = _settings.GamesPerPage;
        _paginationThreshold = _settings.GamesPerPage;

        // Initialize the GamePadController
        // Setting the error logger for GamePad
        GamePadController.Instance2.ErrorLogger = (ex, msg) => LogErrors.LogErrorAsync(ex, msg).Wait();

        // Check if GamePad navigation is enabled in the settings
        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }

        // Initialize _gameFileGrid
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;
            
        // Add the StackPanel from LetterNumberMenu to the MainWindow's Grid
        // Initialize LetterNumberMenu and add it to the UI
        _letterNumberMenu = new LetterNumberMenu();
        LetterNumberMenu.Children.Clear(); // Clear if necessary
        LetterNumberMenu.Children.Add(_letterNumberMenu.LetterPanel); // Add the LetterPanel directly
            
        // Create and integrate LetterNumberMenu
        _letterNumberMenu.OnLetterSelected += async selectedLetter =>
        {
            ResetPaginationButtons(); // Ensure pagination is reset at the beginning
            SearchTextBox.Text = "";  // Clear SearchTextBox
            _currentFilter = selectedLetter; // Update current filter
            await LoadGameFilesAsync(selectedLetter); // Load games
        };
            
        _letterNumberMenu.OnFavoritesSelected += async () =>
        {
            ResetPaginationButtons();
            SearchTextBox.Text = ""; // Clear search field
            _currentFilter = null; // Clear any active filter

            // Filter favorites for the selected system and store them in _currentSearchResults
            var favoriteGames = GetFavoriteGamesForSelectedSystem();
            if (favoriteGames.Any())
            {
                _currentSearchResults = favoriteGames.ToList(); // Store only favorite games in _currentSearchResults
                await LoadGameFilesAsync(null, "FAVORITES"); // Call LoadGameFilesAsync with "FAVORITES" query
            }
            else
            {
                AddNoFilesMessage();
                MessageBox.Show("No favorite games found for the selected system.", "Favorites", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        };
            
        // Initialize favorite's manager and load favorites
        _favoritesManager = new FavoritesManager();
        _favoritesConfig = _favoritesManager.LoadFavorites();
            
        // Pagination related
        PrevPageButton.IsEnabled = false;
        NextPageButton.IsEnabled = false;
        _prevPageButton = PrevPageButton;
        _nextPageButton = NextPageButton;

        // Initialize _gameButtonFactory with settings
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, _gameFileGrid, this);
            
        // Initialize _gameListFactory with required parameters
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, this);

        // Check if a system is already selected, otherwise show the message
        if (SystemComboBox.SelectedItem == null)
        {
            AddNoSystemMessage();
        }

        // Check for updates using Async Event Handler
        Loaded += async (_, _) => await UpdateChecker.CheckForUpdatesAsync(this);
            
        // Stats using Async Event Handler
        Loaded += async (_, _) => await Stats.CallApiAsync();

        // Check for command-line arguments
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("whatsnew"))
        {
            // Show UpdateHistory after the MainWindow is fully loaded
            Loaded += (_, _) => OpenUpdateHistory();
        }
        
        // Attach the Load and Close event handler.
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }
    
    private void ApplyLanguage(string cultureCode = null)
    {
        try
        {
            // Determine the culture code (default to CurrentUICulture if not provided)
            var culture = string.IsNullOrEmpty(cultureCode)
                ? CultureInfo.CurrentUICulture
                : new CultureInfo(cultureCode);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Load the resource dictionary
            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"/resources/strings.{culture.Name}.xaml", UriKind.Relative)
            };

            // Replace the current localization dictionary
            var existingDictionary = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("strings.") ?? false);

            if (existingDictionary != null)
            {
                Resources.MergedDictionaries.Remove(existingDictionary);
            }

            Resources.MergedDictionaries.Add(dictionary);

            // Apply the culture to the application
            LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load language resources: {ex.Message}", "Language Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Fallback to English
            var fallbackDictionary = new ResourceDictionary
            {
                Source = new Uri("/resources/strings.en.xaml", UriKind.Relative)
            };

            Resources.MergedDictionaries.Add(fallbackDictionary);
        }
    }
    
    private void SetLanguageMenuChecked(string languageCode)
    {
        LanguageArabic.IsChecked = languageCode == "ar";
        LanguageGerman.IsChecked = languageCode == "de";
        LanguageEnglish.IsChecked = languageCode == "en";
        LanguageSpanish.IsChecked = languageCode == "es";
        LanguageFrench.IsChecked = languageCode == "fr";
        LanguageHindi.IsChecked = languageCode == "hi";
        LanguageItalian.IsChecked = languageCode == "it";
        LanguageJapanese.IsChecked = languageCode == "ja";
        LanguageKorean.IsChecked = languageCode == "ko";
        LanguageDutch.IsChecked = languageCode == "nl";
        LanguagePortugueseBr.IsChecked = languageCode == "pt-br";
        LanguagePortuguesePt.IsChecked = languageCode == "pt-pt";
        LanguageRussian.IsChecked = languageCode == "ru";
        LanguageTurkish.IsChecked = languageCode == "tr";
        LanguageVietnamese.IsChecked = languageCode == "vi";
        LanguageChineseSimplified.IsChecked = languageCode == "zh-hans";
        LanguageChineseTraditional.IsChecked = languageCode == "zh-hant";
    }

    // Open UpdateHistory window
    private void OpenUpdateHistory()
    {
        var updateHistoryWindow = new UpdateHistory();
        updateHistoryWindow.Show();
    }

    private void SaveApplicationSettings()
    {
        _settings.MainWindowWidth = Width;
        _settings.MainWindowHeight = Height;
        _settings.MainWindowTop = Top;
        _settings.MainWindowLeft = Left;
        _settings.MainWindowState = WindowState.ToString();

        // Set other settings from the application's current state
        _settings.ThumbnailSize = _gameButtonFactory.ImageHeight;
        _settings.GamesPerPage = _filesPerPage;
        _settings.ShowGames = _settings.ShowGames;
        _settings.EnableGamePadNavigation = EnableGamePadNavigation.IsChecked;

        // Save theme settings
        var detectedTheme = ThemeManager.Current.DetectTheme(this);
        if (detectedTheme != null)
        {
            _settings.BaseTheme = detectedTheme.BaseColorScheme;
            _settings.AccentColor = detectedTheme.ColorScheme;
        }

        _settings.Save();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Windows state
        Width = _settings.MainWindowWidth;
        Height = _settings.MainWindowHeight;
        Top = _settings.MainWindowTop;
        Left = _settings.MainWindowLeft;
        WindowState = (WindowState)Enum.Parse(typeof(WindowState), _settings.MainWindowState);
            
        // SelectedSystem and PlayTime
        SelectedSystem = "No system selected";
        PlayTime = "00:00:00";

        // Theme settings
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
            
        // ViewMode state
        SetViewMode(_settings.ViewMode);
        
        // Check if application has write access
        if (!IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory))
        {
            MessageBox.Show(
                "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.\n\n" +
                "It needs write access to its folder.\n\n" +
                "Please move the application folder to a writable location like 'C:\\SimpleLauncher', 'D:\\SimpleLauncher', or the 'Documents' folder.\n\n" +
                "If possible, run it with administrative privileges.",
                "Access Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        // Save MainWindow state
        SaveApplicationSettings();
    }
    
    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        // // Delete generated temp files before close.
        ExtractCompressedFile.Instance2.Cleanup();
        
        // Delete temp folders and files before close.
        CleanSimpleLauncherFolder.CleanupTrash();
        
        // Dispose gamepad resources
        GamePadController.Instance2.Stop();
        GamePadController.Instance2.Dispose();
    }

    // Used in cases that need to reload system.xml or update the pagination settings or update the video and info links 
    private void MainWindow_Restart()
    {
        SaveApplicationSettings();

        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processModule.FileName,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
        
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
        
    private List<string> GetFavoriteGamesForSelectedSystem()
    {
        // Reload favorites to ensure we have the latest data
        _favoritesConfig = _favoritesManager.LoadFavorites();
            
        string selectedSystem = SystemComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedSystem))
        {
            return new List<string>();
        }

        // Retrieve the system configuration for the selected system
        var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        if (selectedConfig == null)
        {
            return new List<string>();
        }

        // Get the system folder path
        string systemFolderPath = selectedConfig.SystemFolder;

        // Filter the favorites and build the full file path for each favorite game
        var favoriteGamePaths = _favoritesConfig.FavoriteList
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => Path.Combine(systemFolderPath, fav.FileName))
            .ToList();

        return favoriteGamePaths;
    }
    
    private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SearchTextBox.Text = ""; // Empty search field
        EmulatorComboBox.ItemsSource = null; // Null selected emulator
        EmulatorComboBox.SelectedIndex = -1; // No emulator selected
        PreviewImage.Source = null; // Empty PreviewImage
            
        // Reset search results
        _currentSearchResults.Clear();
            
        // Hide ListView
        GameFileGrid.Visibility = Visibility.Visible;
        ListViewPreviewArea.Visibility = Visibility.Collapsed;

        if (SystemComboBox.SelectedItem != null)
        {
            string selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig != null)
            {
                // Populate EmulatorComboBox with the emulators for the selected system
                EmulatorComboBox.ItemsSource = selectedConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();

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
                string systemFolderPath = selectedConfig.SystemFolder;
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"{ext}").ToList();
                int gameCount = FileManager.CountFiles(systemFolderPath, fileExtensions);
                
                // SystemInfo
                SystemManager.DisplaySystemInfo(systemFolderPath, gameCount, selectedConfig, _gameFileGrid);
                    
                // Update Image Folder and Rom Folder Variables
                _selectedRomFolder = selectedConfig.SystemFolder;
                _selectedImageFolder = string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder) 
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName) 
                    : selectedConfig.SystemImageFolder;
                    
                // Call DeselectLetter to clear any selected letter
                _letterNumberMenu.DeselectLetter();
                    
                // Reset pagination controls
                ResetPaginationButtons();
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

    private void AddNoSystemMessage()
    {
        // Check the current view mode
        if (_settings.ViewMode == "GridView")
        {
            // Clear existing content in Grid view and add the message
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = "\nPlease select a System",
                Padding = new Thickness(10)
            });
        }
        else
        {
            // For List view, clear existing items in the ObservableCollection instead
            GameListItems.Clear();
            GameListItems.Add(new GameListFactory.GameListViewItem
            {
                FileName = "Please select a System",
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _letterNumberMenu.DeselectLetter();
    }
        
    private void AddNoFilesMessage()
    {
        // Check the current view mode
        if (_settings.ViewMode == "GridView")
        {
            // Clear existing content in Grid view and add the message
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = "\nUnfortunately, no games matched your search query or the selected button.",
                Padding = new Thickness(10)
            });
        }
        else
        {
            // For List view, clear existing items in the ObservableCollection instead
            GameListItems.Clear();
            GameListItems.Add(new GameListFactory.GameListViewItem
            {
                FileName = "Unfortunately, no games matched your search query or the selected button.",
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _letterNumberMenu.DeselectLetter();
    }
        
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
            if (_currentPage > 1)
            {
                _currentPage--;
                if (_currentSearchResults.Any())
                {
                    await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
                }
                else
                {
                    await LoadGameFilesAsync(_currentFilter);
                }
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Previous page button error in the Main window.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            MessageBox.Show("There was an error in this button.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);

            if (_currentPage < totalPages)
            {
                _currentPage++;
                if (_currentSearchResults.Any())
                {
                    await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
                }
                else
                {
                    await LoadGameFilesAsync(_currentFilter);
                }
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Next page button error in the Main window.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            MessageBox.Show("There was an error with this button.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            string errorMessage = $"Error while using the method SearchButton_Click.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            MessageBox.Show("There was an error with this method.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            string errorMessage = $"Error while using the method SearchTextBox_KeyDown.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            MessageBox.Show("There was an error with this method.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);        }
    }

    private async Task ExecuteSearch()
    {
        // Pagination reset
        ResetPaginationButtons();
            
        // Reset search results
        _currentSearchResults.Clear();
    
        // Call DeselectLetter to clear any selected letter
        _letterNumberMenu.DeselectLetter();

        var searchQuery = SearchTextBox.Text.Trim();

        if (SystemComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a system before searching.", "System Not Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(searchQuery))
        {
            MessageBox.Show("Please enter a search query.", "Search Query Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var pleaseWaitWindow = new PleaseWaitSearch();
        await ShowPleaseWaitWindowAsync(pleaseWaitWindow);

        var startTime = DateTime.Now;

        try
        {
            await LoadGameFilesAsync(null, searchQuery);
        }
        finally
        {
            var elapsed = DateTime.Now - startTime;
            var remainingTime = TimeSpan.FromSeconds(1) - elapsed;
            if (remainingTime > TimeSpan.Zero)
            {
                await Task.Delay(remainingTime);
            }
            await ClosePleaseWaitWindowAsync(pleaseWaitWindow);
        }
    }
        
    #endregion

    private Task ShowPleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Show);
        });
    }

    private Task ClosePleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Close);
        });
    }

    public async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null)
    {
        // Move scroller to top
        Scroller.Dispatcher.Invoke(() => Scroller.ScrollToTop());
            
        // Clear PreviewImage
        PreviewImage.Source = null;

        // Clear FileGrid
        GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Clear());
            
        // Clear the ListItems
        await Dispatcher.InvokeAsync(() => GameListItems.Clear());
            
        // Check ViewMode and apply it to the UI
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
            if (SystemComboBox.SelectedItem == null)
            {
                AddNoSystemMessage();
                return;
            }
 
            string selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
            if (selectedConfig == null)
            {
                string errorMessage = "Error while loading the selected system configuration in the Main window, using the method LoadGameFilesAsync.";
                Exception ex = new Exception(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                MessageBox.Show("There was an error while loading the system configuration for this system.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            List<string> allFiles;
                
            // Check if we are in "FAVORITES" mode
            if (searchQuery == "FAVORITES" && _currentSearchResults != null && _currentSearchResults.Any())
            {
                allFiles = _currentSearchResults;
            }
            // Regular behavior: load files based on startLetter or searchQuery
            else
            {
                // Get the SystemFolder from the selected configuration
                string systemFolderPath = selectedConfig.SystemFolder;

                // Extract the file extensions from the selected system configuration
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    // Use stored search results if available
                    if (_currentSearchResults != null && _currentSearchResults.Count != 0)
                    {
                        allFiles = _currentSearchResults;
                    }
                    else
                    {
                        // List of files with that match the system extensions
                        // then sort the list alphabetically 
                        allFiles = await FileManager.GetFilesAsync(systemFolderPath, fileExtensions);

                        if (!string.IsNullOrWhiteSpace(startLetter))
                        {
                            allFiles = await FileManager.FilterFilesAsync(allFiles, startLetter);
                        }

                        bool systemIsMame = selectedConfig.SystemIsMame;

                        allFiles = await Task.Run(() => allFiles.Where(file =>
                        {
                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                            // Search in filename
                            bool filenameMatch = fileNameWithoutExtension.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (!systemIsMame) // If not a MAME system, return match based on filename only
                            {
                                return filenameMatch;
                            }

                            // For MAME systems, additionally check the description for a match
                            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
                            bool descriptionMatch = machine != null && machine.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;

                            return filenameMatch || descriptionMatch;

                        }).ToList());

                        // Store the search results
                        _currentSearchResults = allFiles;
                    }
                }
                else
                {
                    // Reset search results if no search query is provided
                    _currentSearchResults?.Clear();
    
                    // List of files with that match the system extensions
                    // then sort the list alphabetically 
                    allFiles = await FileManager.GetFilesAsync(systemFolderPath, fileExtensions);

                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        allFiles = await FileManager.FilterFilesAsync(allFiles, startLetter);
                    }
                }
            }

            // Sort the collection of files
            allFiles.Sort();

            // Count the collection of files
            _totalFiles = allFiles.Count;

            // Calculate the indices of files displayed on the current page
            int startIndex = (_currentPage - 1) * _filesPerPage + 1; // +1 because we are dealing with a 1-based index for displaying
            int endIndex = startIndex + _filesPerPage; // Actual number of files loaded on this page
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
            TotalFilesLabel.Dispatcher.Invoke(() => 
                TotalFilesLabel.Content = allFiles.Count == 0 ? $"Displaying files 0 to {endIndex} out of {_totalFiles} total" : $"Displaying files {startIndex} to {endIndex} out of {_totalFiles} total"
            );

            // Reload the FavoritesConfig
            _favoritesConfig = _favoritesManager.LoadFavorites();
                
            // Initialize GameButtonFactory with updated FavoritesConfig
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, _gameFileGrid, this);
                
            // Initialize GameListFactory with updated FavoritesConfig
            var gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, this);

            // Display files based on ViewMode
            foreach (var filePath in allFiles)
            {
                if (_settings.ViewMode == "GridView")
                {
                    Button gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedConfig);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // For list view
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
            string errorMessage = $"Error while using the method LoadGameFilesAsync in the Main window.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            MessageBox.Show("There was an error while loading the game list.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void GameDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GameDataGrid.SelectedItem is GameListFactory.GameListViewItem selectedItem)
        {
            var gameListViewFactory = new GameListFactory(
                EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, this
            );
            gameListViewFactory.HandleSelectionChanged(selectedItem);
        }
    }

    private async void GameDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
            string errorMessage = $"Error while using the method GameDataGrid_MouseDoubleClick.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            MessageBox.Show("There was an error with this method.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private static bool IsWritableDirectory(string path)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(path))
                return false;

            // Generate a unique temporary file path
            string testFile = Path.Combine(path, Guid.NewGuid().ToString() + ".tmp");

            // Attempt to create and delete the file
            using (FileStream fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                fs.Close();
            }
            File.Delete(testFile);

            return true;
        }
        catch
        {
            return false;
        }
    }
        
    #region Menu Items
        
    private void EasyMode_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
                
        EditSystemEasyMode editSystemEasyModeWindow = new(_settings);
        editSystemEasyModeWindow.ShowDialog();
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
                
        EditSystem editSystemWindow = new(_settings);
        editSystemWindow.ShowDialog();
    }
    
    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        DownloadImagePack downloadImagePack = new();
        downloadImagePack.ShowDialog();
    }
        
    private void EditLinks_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
                
        EditLinks editLinksWindow = new(_settings);
        editLinksWindow.ShowDialog();
    }
    private void BugReport_Click(object sender, RoutedEventArgs e)
    {
        BugReport bugReportWindow = new();
        bugReportWindow.ShowDialog();
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
            string contextMessage = $"Unable to open the Donation Link from the menu.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("There was an error opening the Donation Link.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        About aboutWindow = new();
        aboutWindow.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
        
    private void ShowAllGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateGameVisibility(visibilityCondition: _ => true); // Show all games
        UpdateShowGamesSetting("ShowAll");
        UpdateMenuCheckMarks("ShowAll");
    }
        
    private void ShowGamesWithCover_Click(object sender, RoutedEventArgs e)
    {
        UpdateGameVisibility(visibilityCondition: btn => btn.Tag?.ToString() != "DefaultImage"); // Show games with covers only
        UpdateShowGamesSetting("ShowWithCover");
        UpdateMenuCheckMarks("ShowWithCover");
    }

    private void ShowGamesWithoutCover_Click(object sender, RoutedEventArgs e)
    {
        UpdateGameVisibility(visibilityCondition: btn => btn.Tag?.ToString() == "DefaultImage"); // Show games without covers only
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
        
    private void EnableGamePadNavigation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            menuItem.IsChecked = !menuItem.IsChecked;
            _settings.EnableGamePadNavigation = menuItem.IsChecked;
            _settings.Save();

            if (menuItem.IsChecked)
            {
                GamePadController.Instance2.Start();
            }
            else
            {
                GamePadController.Instance2.Stop();
            }
        }
    }

    private async void ThumbnailSize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem clickedItem)
            {
                // Extract the numeric value from the header
                var sizeText = clickedItem.Header.ToString();
                if (sizeText != null && int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out int newSize))
                {
                    _gameButtonFactory.ImageHeight = newSize; // Update the image height
                    _settings.ThumbnailSize = newSize; // Update the settings
                    _settings.Save(); // Save the settings
                    UpdateMenuCheckMarks(newSize);
                    
                    // Reload List of Games
                    await LoadGameFilesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error while using the method ThumbnailSize_Click.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            MessageBox.Show("There was an error with this method.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void GamesPerPage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem clickedItem)
        {
            // Extract the numeric value from the header
            var pageText = clickedItem.Header.ToString();
            if (pageText != null && int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out int newPage))
            {
                _filesPerPage = newPage; // Update the page size
                _paginationThreshold = newPage; // update pagination threshold
                _settings.GamesPerPage = newPage; // Update the settings
                    
                _settings.Save(); // Save the settings
                UpdateMenuCheckMarks2(newPage);
                    
                // Save Application Settings
                SaveApplicationSettings();
                    
                // Restart Application
                MainWindow_Restart();
            }
        }
    }
        
    private void GlobalSearch_Click(object sender, RoutedEventArgs e)
    {
        var globalSearchWindow = new GlobalSearch(_systemConfigs, _machines, _settings, this);
        globalSearchWindow.Show();
    }
        
    private void GlobalStats_Click(object sender, RoutedEventArgs e)
    {
        var globalStatsWindow = new GlobalStats(_systemConfigs);
        globalStatsWindow.Show();
    }
        
    private void Favorites_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
            
        var favoritesWindow = new Favorites(_settings, _systemConfigs, _machines, this);
        favoritesWindow.Show();
    }
        
    private void OrganizeSystemImages_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string findRomCoverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "FindRomCover", "FindRomCover.exe");

            if (File.Exists(findRomCoverPath))
            {
                string absoluteImageFolder = null;
                string absoluteRomFolder = null;

                // Check if _selectedImageFolder and _selectedRomFolder are set
                if (!string.IsNullOrEmpty(_selectedImageFolder))
                {
                    absoluteImageFolder = Path.GetFullPath(Path.IsPathRooted(_selectedImageFolder)
                        ? _selectedImageFolder
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedImageFolder));
                }

                if (!string.IsNullOrEmpty(_selectedRomFolder))
                {
                    absoluteRomFolder = Path.GetFullPath(Path.IsPathRooted(_selectedRomFolder)
                        ? _selectedRomFolder
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedRomFolder));
                }

                // Determine arguments based on available folders
                string arguments = string.Empty;
                if (absoluteImageFolder != null && absoluteRomFolder != null)
                {
                    arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
                }

                // Start the process with or without arguments
                Process.Start(new ProcessStartInfo
                {
                    FileName = findRomCoverPath,
                    Arguments = arguments,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBoxResult reinstall = MessageBox.Show(
                    "'FindRomCover.exe' was not found in the expected path.\n\n" +
                    "Do you want to reinstall 'Simple Launcher' to fix it?",
                    "File Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (reinstall == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' manually.",
                        "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while launching 'FindRomCover.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show("An error occurred while launching 'FindRomCover.exe'.\n\n" +
                            "This type of error is usually related to low permission settings for Simple Launcher. Try running it with administrative permissions.\n\n" +
                            "The error has been reported to the developer, who will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself, check the file 'error_user.log' inside the 'Simple Launcher' folder.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForPs3GamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.exe");

            if (File.Exists(createBatchFilesForPs3GamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForPs3GamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBoxResult reinstall = MessageBox.Show(
                    "'CreateBatchFilesForPS3Games.exe' was not found in the expected path.\n\n" +
                    "Do you want to reinstall 'Simple Launcher' to fix it?",
                    "File Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (reinstall == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' manually.",
                        "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForPS3Games.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("An error occurred while launching 'CreateBatchFilesForPS3Games.exe'.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself check the file 'error_user.log' inside 'Simple Launcher' folder",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForScummVmGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.exe");

            if (File.Exists(createBatchFilesForScummVmGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForScummVmGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBoxResult reinstall = MessageBox.Show(
                    "'CreateBatchFilesForScummVMGames.exe' was not found in the expected path.\n\n" +
                    "Do you want to reinstall 'Simple Launcher' to fix it?",
                    "File Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (reinstall == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' manually.",
                        "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForScummVMGames.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("An error occurred while launching 'CreateBatchFilesForScummVMGames.exe'.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself check the file 'error_user.log' inside 'Simple Launcher' folder",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void CreateBatchFilesForSegaModel3Games_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForSegaModel3Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.exe");

            if (File.Exists(createBatchFilesForSegaModel3Path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForSegaModel3Path,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBoxResult reinstall = MessageBox.Show(
                    "'CreateBatchFilesForSegaModel3Games.exe' was not found in the expected path.\n\n" +
                    "Do you want to reinstall 'Simple Launcher' to fix it?",
                    "File Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (reinstall == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' manually.",
                        "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForSegaModel3Games.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("An error occurred while launching 'CreateBatchFilesForSegaModel3Games.exe'.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself check the file 'error_user.log' inside 'Simple Launcher' folder",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForWindowsGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.exe");

            if (File.Exists(createBatchFilesForWindowsGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForWindowsGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBoxResult reinstall = MessageBox.Show(
                    "'CreateBatchFilesForWindowsGames.exe' was not found in the expected path.\n\n" +
                    "Do you want to reinstall 'Simple Launcher' to fix it?",
                    "File Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (reinstall == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' manually.",
                        "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForWindowsGames.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("An error occurred while launching 'CreateBatchFilesForWindowsGames.exe'.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself check the file 'error_user.log' inside 'Simple Launcher' folder",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CreateBatchFilesForXbox360XBLAGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForXbox360XblaGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.exe");

            if (File.Exists(createBatchFilesForXbox360XblaGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForXbox360XblaGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBoxResult reinstall = MessageBox.Show(
                    "'CreateBatchFilesForXbox360XBLAGames.exe' was not found in the expected path.\n\n" +
                    "Do you want to reinstall 'Simple Launcher' to fix it?",
                    "File Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (reinstall == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    MessageBox.Show("Please reinstall 'Simple Launcher' manually.",
                        "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForXbox360XBLAGames.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("An error occurred while launching 'CreateBatchFilesForXbox360XBLAGames.exe'.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself check the file 'error_user.log' inside 'Simple Launcher' folder",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void UpdateMenuCheckMarks(int selectedSize)
    {
        Size100.IsChecked = (selectedSize == 100);
        Size150.IsChecked = (selectedSize == 150);
        Size200.IsChecked = (selectedSize == 200);
        Size250.IsChecked = (selectedSize == 250);
        Size300.IsChecked = (selectedSize == 300);
        Size350.IsChecked = (selectedSize == 350);
        Size400.IsChecked = (selectedSize == 400);
        Size450.IsChecked = (selectedSize == 450);
        Size500.IsChecked = (selectedSize == 500);
        Size550.IsChecked = (selectedSize == 550);
        Size600.IsChecked = (selectedSize == 600);
    }
        
    private void UpdateMenuCheckMarks2(int selectedSize)
    {
        Page100.IsChecked = (selectedSize == 100);
        Page200.IsChecked = (selectedSize == 200);
        Page300.IsChecked = (selectedSize == 300);
        Page400.IsChecked = (selectedSize == 400);
        Page500.IsChecked = (selectedSize == 500);
        Page1000.IsChecked = (selectedSize == 1000);
    }
        
    private void UpdateMenuCheckMarks3(string selectedValue)
    {
        ShowAll.IsChecked = (selectedValue == "ShowAll");
        ShowWithCover.IsChecked = (selectedValue == "ShowWithCover");
        ShowWithoutCover.IsChecked = (selectedValue == "ShowWithoutCover");
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
                SelectedSystem = "No system selected";
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
                SelectedSystem = "No system selected";
                PlayTime = "00:00:00";
                AddNoSystemMessage();
                await LoadGameFilesAsync();
            }
            _settings.Save(); // Save the updated ViewMode
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error while using the method ChangeViewMode_Click.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            MessageBox.Show("There was an error with this method.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        if (sender is MenuItem menuItem)
        {
            string selectedLanguage = menuItem.Name switch
            {
                "LanguageChineseSimplified" => "zh-hans",
                "LanguageChineseTraditional" => "zh-hant",
                "LanguageGerman" => "de",
                "LanguageEnglish" => "en",
                "LanguageSpanish" => "es",
                "LanguageFrench" => "fr",
                "LanguageJapanese" => "ja",                
                "LanguageKorean" => "ko",
                "LanguagePortugueseBr" => "pt-br",
                "LanguagePortuguesePt" => "pt-pt",
                "LanguageRussian" => "ru",
                _ => "en"
            };

            // // Apply Language
            // ApplyLanguage(selectedLanguage);

            // Save settings
            _settings.Language = selectedLanguage;
            _settings.Save();

            // Update checked status
            SetLanguageMenuChecked(selectedLanguage);
            
            // Restart Application
            MainWindow_Restart();
        }
    }

    #endregion
        
    #region Theme Options
        
    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            string baseTheme = menuItem.Header.ToString();
            string currentAccent = ThemeManager.Current.DetectTheme(this)?.ColorScheme;
            App.ChangeTheme(baseTheme, currentAccent);

            UncheckBaseThemes();
            menuItem.IsChecked = true;
        }
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            string accentColor = menuItem.Header.ToString();
            string currentBaseTheme = ThemeManager.Current.DetectTheme(this)?.BaseColorScheme;
            App.ChangeTheme(currentBaseTheme, accentColor);

            UncheckAccentColors();
            menuItem.IsChecked = true;
        }
    }

    private void UncheckBaseThemes()
    {
        LightTheme.IsChecked = false;
        DarkTheme.IsChecked = false;
    }

    private void UncheckAccentColors()
    {
        RedAccent.IsChecked = false;
        GreenAccent.IsChecked = false;
        BlueAccent.IsChecked = false;
        PurpleAccent.IsChecked = false;
        OrangeAccent.IsChecked = false;
        LimeAccent.IsChecked = false;
        EmeraldAccent.IsChecked = false;
        TealAccent.IsChecked = false;
        CyanAccent.IsChecked = false;
        CobaltAccent.IsChecked = false;
        IndigoAccent.IsChecked = false;
        VioletAccent.IsChecked = false;
        PinkAccent.IsChecked = false;
        MagentaAccent.IsChecked = false;
        CrimsonAccent.IsChecked = false;
        AmberAccent.IsChecked = false;
        YellowAccent.IsChecked = false;
        BrownAccent.IsChecked = false;
        OliveAccent.IsChecked = false;
        SteelAccent.IsChecked = false;
        MauveAccent.IsChecked = false;
        TaupeAccent.IsChecked = false;
        SiennaAccent.IsChecked = false;
    }

    private void SetCheckedTheme(string baseTheme, string accentColor)
    {
        switch (baseTheme)
        {
            case "Light":
                LightTheme.IsChecked = true;
                break;
            case "Dark":
                DarkTheme.IsChecked = true;
                break;
        }

        switch (accentColor)
        {
            case "Red":
                RedAccent.IsChecked = true;
                break;
            case "Green":
                GreenAccent.IsChecked = true;
                break;
            case "Blue":
                BlueAccent.IsChecked = true;
                break;
            case "Purple":
                PurpleAccent.IsChecked = true;
                break;
            case "Orange":
                OrangeAccent.IsChecked = true;
                break;
            case "Lime":
                LimeAccent.IsChecked = true;
                break;
            case "Emerald":
                EmeraldAccent.IsChecked = true;
                break;
            case "Teal":
                TealAccent.IsChecked = true;
                break;
            case "Cyan":
                CyanAccent.IsChecked = true;
                break;
            case "Cobalt":
                CobaltAccent.IsChecked = true;
                break;
            case "Indigo":
                IndigoAccent.IsChecked = true;
                break;
            case "Violet":
                VioletAccent.IsChecked = true;
                break;
            case "Pink":
                PinkAccent.IsChecked = true;
                break;
            case "Magenta":
                MagentaAccent.IsChecked = true;
                break;
            case "Crimson":
                CrimsonAccent.IsChecked = true;
                break;
            case "Amber":
                AmberAccent.IsChecked = true;
                break;
            case "Yellow":
                YellowAccent.IsChecked = true;
                break;
            case "Brown":
                BrownAccent.IsChecked = true;
                break;
            case "Olive":
                OliveAccent.IsChecked = true;
                break;
            case "Steel":
                SteelAccent.IsChecked = true;
                break;
            case "Mauve":
                MauveAccent.IsChecked = true;
                break;
            case "Taupe":
                TaupeAccent.IsChecked = true;
                break;
            case "Sienna":
                SiennaAccent.IsChecked = true;
                break;
        }
    }
    #endregion
    
    #region TrayIcon
        
    private void InitializeTrayIcon()
    {
        // Create a context menu for the tray icon
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Open", null, OnOpen);
        _trayMenu.Items.Add("Exit", null, OnExit);

        // Load the embedded icon from resources
        var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/SimpleLauncher;component/icon/icon.ico"))?.Stream;

        // Create the tray icon using the embedded icon
        if (iconStream != null)
        {
            _trayIcon = new NotifyIcon
            {
                Icon = new Icon(iconStream), // Set icon from stream
                ContextMenuStrip = _trayMenu,
                Text = @"Simple Launcher",
                Visible = true
            };

            // Handle tray icon events
            _trayIcon.DoubleClick += OnOpen;
        }
    }
        
    // Handle "Open" context menu item or tray icon double-click
    private void OnOpen(object sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    // Handle "Exit" context menu item
    private void OnExit(object sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        Application.Current.Shutdown();
    }

    // Override the OnStateChanged method to hide the window when minimized
    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            ShowTrayMessage("Simple Launcher is minimized to the tray.");
        }
        base.OnStateChanged(e);
    }

    // Method to display a balloon message on the tray icon
    private void ShowTrayMessage(string message)
    {
        _trayIcon.BalloonTipTitle = @"Simple Launcher";
        _trayIcon.BalloonTipText = message;
        _trayIcon.ShowBalloonTip(3000); // Display for 3 seconds
    }

    // Clean up resources when closing the application
    protected override void OnClosing(CancelEventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        base.OnClosing(e);
    }
        
    #endregion
}