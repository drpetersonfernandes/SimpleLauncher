﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
using Orientation = System.Windows.Controls.Orientation;

namespace SimpleLauncher
{
    public partial class MainWindow : INotifyPropertyChanged
    {

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
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
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
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize default values for SelectedSystem and PlayTime
            if (SystemComboBox.Items.Count > 0)
            {
                // Set the first system as the default selected system
                SystemComboBox.SelectedIndex = 0; // You can change this to set another system if needed
                SelectedSystem = SystemComboBox.SelectedItem.ToString();
        
                // Retrieve the playtime for the default selected system
                var defaultSystemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == SelectedSystem);
                PlayTime = defaultSystemPlayTime != null ? defaultSystemPlayTime.PlayTime : "00:00:00";
            }
            else
            {
                SelectedSystem = "No system selected";
                PlayTime = "00:00:00";
            }

            DataContext = this; // Ensure the DataContext is set to the current MainWindow instance for binding
            
            // Tray icon
            InitializeTrayIcon();
            
            // Load settings.xml
            _settings = new SettingsConfig();
            
            // Set the initial theme
            App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
            SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
            
            // Initialize favorite's manager and load favorites
            _favoritesManager = new FavoritesManager();
            _favoritesConfig = _favoritesManager.LoadFavorites();
            
            // Load mame.xml
            // string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.xml");
            _machines = MameConfig.LoadFromXml();
            
            // Load system.xml
            try
            {
                // string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");
                _systemConfigs = SystemConfig.LoadSystemConfigs();

                // Sort the system names in alphabetical order
                var sortedSystemNames = _systemConfigs.Select(config => config.SystemName).OrderBy(name => name).ToList();

                SystemComboBox.ItemsSource = sortedSystemNames;
            }
            catch (Exception ex)
            {
                string contextMessage = $"system.xml was not found in the application folder.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("The file system.xml is missing.\n\nThe application will be shutdown.\n\nPlease reinstall Simple Launcher to restore this file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

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
                // Ensure pagination is reset at the beginning
                ResetPaginationButtons();

                // Clear SearchTextBox
                SearchTextBox.Text = "";
    
                // Update current filter
                _currentFilter = selectedLetter;

                // Load games
                await LoadGameFilesAsync(selectedLetter);
            };
            
            // Pagination related
            PrevPageButton.IsEnabled = false;
            NextPageButton.IsEnabled = false;
            _prevPageButton = PrevPageButton; // Connects the field to the XAML-defined button
            _nextPageButton = NextPageButton; // Connects the field to the XAML-defined button

            // Initialize _gameButtonFactory with settings
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, _gameFileGrid, this);

            // Check if a system is already selected, otherwise show the message
            if (SystemComboBox.SelectedItem == null)
            {
                AddNoSystemMessage();
            }

            // Check for updates using Async Event Handler
            Loaded += async (_, _) => await UpdateChecker.CheckForUpdatesAsync(this);
            
            // Attach the Load and Close event handler.
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            
            // Stats using Async Event Handler
            Loaded += async (_, _) => await Stats.CallApiAsync();
        }

        // The app will delete generated temp files before close.
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ExtractCompressedFile.Instance2.Cleanup();
        }

        // Save Application Settings
        private void SaveApplicationSettings()
        {
            _settings.MainWindowWidth = Width;
            _settings.MainWindowHeight = Height;
            _settings.MainWindowTop = Top;
            _settings.MainWindowLeft = Left;
            _settings.MainWindowState = WindowState.ToString();

            // Set other settings from the application's current state
            _settings.ThumbnailSize = _gameButtonFactory.ImageHeight; // Assuming ImageHeight is used for ThumbnailSize
            _settings.GamesPerPage = _filesPerPage;
            _settings.ShowGames = _settings.ShowGames; // This assumes the setting is already updated correctly
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

        // Load state and size of MainWindow and also the theme from settings.xml
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Width = _settings.MainWindowWidth;
            Height = _settings.MainWindowHeight;
            Top = _settings.MainWindowTop;
            Left = _settings.MainWindowLeft;
            WindowState = (WindowState)Enum.Parse(typeof(WindowState), _settings.MainWindowState);

            // Apply saved theme settings
            App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
            SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
        }

        // Dispose gamepad resources and Save MainWindow state and size to setting.xml before close.
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            GamePadController.Instance2.Stop();
            GamePadController.Instance2.Dispose();
            SaveApplicationSettings();
        }

        // Restart Application
        // Used in cases that need to reload system.xml or update the pagination settings or update the video and info links 
        private void MainWindow_Restart()
        {
            // Save Application Settings
            SaveApplicationSettings();

            // Prepare the process start info
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = processModule.FileName,
                    UseShellExecute = true
                };

                // Start new application instance
                Process.Start(startInfo);

                // Shutdown current application instance
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }
       
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
                    Text = @"SimpleLauncher",
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
                ShowTrayMessage("SimpleLauncher is minimized to the tray.");
            }
            base.OnStateChanged(e);
        }

        // Method to display a balloon message on the tray icon
        private void ShowTrayMessage(string message)
        {
            _trayIcon.BalloonTipTitle = @"SimpleLauncher";
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

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchTextBox.Text = "";
            EmulatorComboBox.ItemsSource = null;
            EmulatorComboBox.SelectedIndex = -1;
            
            // Reset search results
            _currentSearchResults.Clear();

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
                    int gameCount = CountFiles(systemFolderPath, fileExtensions);
                    DisplaySystemInfo(systemFolderPath, gameCount, selectedConfig);
                    
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

        private static int CountFiles(string folderPath, List<string> fileExtensions)
        {
            if (!Directory.Exists(folderPath))
            {
                return 0;
            }

            try
            {
                int fileCount = 0;

                foreach (string extension in fileExtensions)
                {
                    string searchPattern = $"*.{extension}";
                    fileCount += Directory.EnumerateFiles(folderPath, searchPattern).Count();
                }

                return fileCount;
            }
            catch (Exception ex)
            {
                string contextMessage = $"An error occurred while counting files in the Main window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("An error occurred while counting files.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void DisplaySystemInfo(string systemFolder, int gameCount, SystemConfig selectedConfig)
        {
            // Clear existing content
            GameFileGrid.Children.Clear();

            // Create a StackPanel to hold TextBlocks vertically
            var verticalStackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };

            // Create and add system info TextBlock
            var systemInfoTextBlock = new TextBlock
            {
                Text = $"\nSystem Folder: {systemFolder}\n" +
                       $"System Image Folder: {selectedConfig.SystemImageFolder ?? "[Using default image folder]"}\n" +
                       $"System is MAME? {selectedConfig.SystemIsMame}\n" +
                       $"Format to Search in the System Folder: {string.Join(", ", selectedConfig.FileFormatsToSearch)}\n" +
                       $"Extract File Before Launch? {selectedConfig.ExtractFileBeforeLaunch}\n" +
                       $"Format to Launch After Extraction: {string.Join(", ", selectedConfig.FileFormatsToLaunch)}\n",
                Padding = new Thickness(0),
                TextWrapping = TextWrapping.Wrap
            };
            verticalStackPanel.Children.Add(systemInfoTextBlock);

            // Add the number of games in the system folder
            var gameCountTextBlock = new TextBlock
            {
                Text = $"Total number of games in the System Folder, excluding files in subdirectories: {gameCount}",
                Padding = new Thickness(0),
                TextWrapping = TextWrapping.Wrap
            };
            verticalStackPanel.Children.Add(gameCountTextBlock);

            // Determine the image folder to search
            string imageFolderPath = selectedConfig.SystemImageFolder;
            if (string.IsNullOrWhiteSpace(imageFolderPath))
            {
                // Use default image folder if SystemImageFolder is not set
                imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName);
            }

            // Add the number of images in the system's image folder
            if (Directory.Exists(imageFolderPath))
            {
                var imageExtensions = new List<string> { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" };
                int imageCount = imageExtensions.Sum(ext => Directory.GetFiles(imageFolderPath, ext).Length);

                var imageCountTextBlock = new TextBlock
                {
                    Text = $"Number of images in the System Image Folder: {imageCount}",
                    Padding = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap
                };
                verticalStackPanel.Children.Add(imageCountTextBlock);
            }
            else
            {
                var noImageFolderTextBlock = new TextBlock
                {
                    Text = "System Image Folder does not exist or is not specified.",
                    Padding = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap
                };
                verticalStackPanel.Children.Add(noImageFolderTextBlock);
            }

            // Dynamically create and add a TextBlock for each emulator to the vertical StackPanel
            foreach (var emulator in selectedConfig.Emulators)
            {
                var emulatorInfoTextBlock = new TextBlock
                {
                    Text = $"\nEmulator Name: {emulator.EmulatorName}\n" +
                           $"Emulator Location: {emulator.EmulatorLocation}\n" +
                           $"Emulator Parameters: {emulator.EmulatorParameters}\n",
                    Padding = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap
                };
                verticalStackPanel.Children.Add(emulatorInfoTextBlock);
            }

            // Add the vertical StackPanel to the horizontal WrapPanel
            GameFileGrid.Children.Add(verticalStackPanel);

            // Validate the System
            ValidateSystemConfiguration(systemFolder, selectedConfig);
        }

        private void ValidateSystemConfiguration(string systemFolder, SystemConfig selectedConfig)
        {
            StringBuilder errorMessages = new StringBuilder();
            bool hasErrors = false;

            // Validate the system folder path
            if (!IsValidPath(systemFolder))
            {
                hasErrors = true;
                errorMessages.AppendLine($"System Folder path is not valid or does not exist: '{systemFolder}'\n\n");
            }

            // Validate the system image folder path if it's provided. Allow null or empty.
            if (!string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder) && !IsValidPath(selectedConfig.SystemImageFolder))
            {
                hasErrors = true;
                errorMessages.AppendLine($"System Image Folder path is not valid or does not exist: '{selectedConfig.SystemImageFolder}'\n\n");
            }

            // Validate each emulator's location path if it's provided. Allow null or empty.
            foreach (var emulator in selectedConfig.Emulators)
            {
                if (!string.IsNullOrWhiteSpace(emulator.EmulatorLocation) && !IsValidPath(emulator.EmulatorLocation))
                {
                    hasErrors = true;
                    errorMessages.AppendLine($"Emulator location is not valid for {emulator.EmulatorName}: '{emulator.EmulatorLocation}'\n\n");
                }
            }
            
            // Display all error messages if there are any errors
            if (hasErrors)
            {
                string extraline = "Edit System to fix it.";
                MessageBox.Show(errorMessages + extraline,"Validation Errors", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Check paths in SystemFolder, SystemImageFolder and EmulatorLocation. Allow relative paths.
        private bool IsValidPath(string path)
        {
            // Check if the path is not null or whitespace
            if (string.IsNullOrWhiteSpace(path)) return false;

            // Check if the path is an absolute path and exists
            if (Directory.Exists(path) || File.Exists(path)) return true;

            // Assume the path might be relative and combine it with the base directory
            // Allow relative paths
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, path);

            // Check if the combined path exists
            return Directory.Exists(fullPath) || File.Exists(fullPath);
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
                string errorMessage = $"Previous page button error in the Main window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);

                MessageBox.Show("There was an error in the button.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);
            try
            {
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
                string errorMessage = $"Next page button error in the Main window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);

                MessageBox.Show("There was an error with the button.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        
        private void UpdatePaginationButtons()
        {
            _prevPageButton.IsEnabled = _currentPage > 1;
            _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        }

        private void AddNoSystemMessage()
        {
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = "\nPlease select a System",
                Padding = new Thickness(10)
            });

            // Deselect any selected letter when no system is selected
            _letterNumberMenu.DeselectLetter();
        }
        
        private void NoFilesMessage()
        {
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = "\nUnfortunately, no games matched your search query or the selected button.",
                Padding = new Thickness(10)
            });

            // Deselect any selected letter when no system is selected
            _letterNumberMenu.DeselectLetter();
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
        }
        
        private void UpdateMenuCheckMarks3(string selectedValue)
        {
            ShowAll.IsChecked = (selectedValue == "ShowAll");
            ShowWithCover.IsChecked = (selectedValue == "ShowWithCover");
            ShowWithoutCover.IsChecked = (selectedValue == "ShowWithoutCover");
        }
       
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteSearch();
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ExecuteSearch();
            }
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

        private async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null)
        {
            // Move scroller to top
            Scroller.Dispatcher.Invoke(() => Scroller.ScrollToTop());

            // Clear FileGrid
            GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Clear());

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

                    MessageBox.Show("There was an error while loading the system configuration for this system.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get the SystemFolder from the selected configuration
                string systemFolderPath = selectedConfig.SystemFolder;

                // Extract the file extensions from the selected system configuration
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList();

                List<string> allFiles;

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    // Use stored search results if available
                    if (_currentSearchResults.Any())
                    {
                        allFiles = _currentSearchResults;
                    }
                    else
                    {
                        // List of files with that match the system extensions
                        // then sort the list alphabetically 
                        allFiles = await GetFilesAsync(systemFolderPath, fileExtensions);

                        if (!string.IsNullOrWhiteSpace(startLetter))
                        {
                            allFiles = await FilterFilesAsync(allFiles, startLetter);
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
                    _currentSearchResults.Clear();
    
                    // List of files with that match the system extensions
                    // then sort the list alphabetically 
                    allFiles = await GetFilesAsync(systemFolderPath, fileExtensions);

                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        allFiles = await FilterFilesAsync(allFiles, startLetter);
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
                    NoFilesMessage();
                }

                // Update the UI to reflect the current pagination status and the indices of files being displayed
                TotalFilesLabel.Dispatcher.Invoke(() => 
                    TotalFilesLabel.Content = allFiles.Count == 0 ? $"Displaying files 0 to {endIndex} out of {_totalFiles} total" : $"Displaying files {startIndex} to {endIndex} out of {_totalFiles} total"
                );

                // Reload the FavoritesConfig
                _favoritesConfig = _favoritesManager.LoadFavorites();
    
                // Create a new instance of GameButtonFactory with updated FavoritesConfig
                _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, _gameFileGrid, this);

                // Create Button action for each cell
                foreach (var filePath in allFiles)
                {
                    // Adjust the CreateGameButton call.
                    Button gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, SystemComboBox.SelectedItem.ToString(), selectedConfig);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }

                // Apply visibility settings to each button based on _settings.ShowGames
                ApplyShowGamesSetting();

                // Update the UI to reflect the current pagination status
                UpdatePaginationButtons();

            }
            catch (Exception ex)
            {
                string errorMessage = $"Error while using the method LoadGameFilesAsync in the Main window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                MessageBox.Show("There was an error while creating the game buttons.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (!Directory.Exists(directoryPath))
                    {
                        return new List<string>();
                    }
                    var foundFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(directoryPath, ext)).ToList();
                    return foundFiles;
                }
                catch (Exception ex)
                {
                    string errorMessage = $"There was an error using the method GetFilesAsync in the Main window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, errorMessage);

                    MessageBox.Show("There was an error finding the files.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<string>();
                }
            });
        }

        private static async Task<List<string>> FilterFilesAsync(List<string> files, string startLetter)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(startLetter))
                    return files; // If no startLetter is provided, no filtering is required

                if (startLetter == "#")
                {
                    return files.Where(file => char.IsDigit(Path.GetFileName(file)[0])).ToList();
                }

                return files.Where(file => Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
            });
        }
        
        #region Menu Items
        
        private void EasyMode_Click(object sender, RoutedEventArgs e)
        {
            // Save Application Settings
            SaveApplicationSettings();
                
            EditSystemEasyMode editSystemEasyModeWindow = new(_settings);
            editSystemEasyModeWindow.ShowDialog();
        }

        private void ExpertMode_Click(object sender, RoutedEventArgs e)
        {
            // Save Application Settings
            SaveApplicationSettings();
                
            EditSystem editSystemWindow = new(_settings);
            editSystemWindow.ShowDialog();
        }
        
        private void EditLinks_Click(object sender, RoutedEventArgs e)
        {
            // Save Application Settings
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
                    FileName = "https://purelogiccode.com/Donate",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                string contextMessage = $"Unable to open the Donation Link from the menu.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("There was an error opening the Donation Link.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ThumbnailSize_Click(object sender, RoutedEventArgs e)
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
                    
                    // Inform user
                    MessageBox.Show("The thumbnail size is set.\n\nReload the list of games to see the new size.", "Thumbnail size is set", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
            // Save Application Settings
            SaveApplicationSettings();
            
            var favoritesWindow = new Favorites(_settings, _systemConfigs, _machines, this);
            favoritesWindow.Show();
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
            // Uncheck other accent color menu items as needed
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

    }
}