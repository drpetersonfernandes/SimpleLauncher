using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public partial class MainWindow
    {
        // pagination related
        private int _currentPage = 1;
        private int _filesPerPage; //variable instance
        private int _totalFiles;
        private int _paginationThreshold; //variable instance
        private readonly Button _nextPageButton;
        private readonly Button _prevPageButton;
        private readonly string _currentFilter = null;
        
        // Instance variables
        private readonly List<SystemConfig> _systemConfigs;
        private readonly LetterNumberMenu _letterNumberMenu = new();
        private readonly WrapPanel _gameFileGrid;
        private readonly GameButtonFactory _gameButtonFactory;
        private readonly AppSettings _settings;
        private readonly List<MameConfig> _machines;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load mame.xml
            string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.xml");
            _machines = MameConfig.LoadFromXml(xmlPath).Result;
            
            // Load settings.xml
            _settings = new AppSettings("settings.xml");

            // Load system.xml
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");
                _systemConfigs = SystemConfig.LoadSystemConfigs(path);
                SystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
            }
            catch
            {
                Application.Current.Shutdown();
            }

            // Apply settings to your application
            HideGamesNoCover.IsChecked = _settings.HideGamesWithNoCover;
            EnableGamePadNavigation.IsChecked = _settings.EnableGamePadNavigation;
            UpdateMenuCheckMarks(_settings.ThumbnailSize);
            UpdateMenuCheckMarks2(_settings.GamesPerPage);
            _filesPerPage = _settings.GamesPerPage; // load GamesPerPage value from setting.xml
            _paginationThreshold = _settings.GamesPerPage; // load GamesPerPage value from setting.xml

            // Initialize the GamePadController.cs
            // Setting the error logger
            GamePadController.Instance.ErrorLogger = (ex, msg) => LogErrors.LogErrorAsync(ex, msg).Wait();

            // Check if GamePad navigation is enabled in the settings
            if (_settings.EnableGamePadNavigation)
            {
                GamePadController.Instance.Start();
            }
            else
            {
                GamePadController.Instance.Stop();
            }

            // Initialize _gameFileGrid
            _gameFileGrid = FindName("GameFileGrid") as WrapPanel;

            // Create and integrate LetterNumberMenu
            _letterNumberMenu.OnLetterSelected += async (selectedLetter) =>
            {
                // Reset pagination controls
                ResetPaginationButtons();
                
                await LoadGameFiles(selectedLetter);
            };

            // Add the StackPanel from LetterNumberMenu to the MainWindow's Grid
            Grid.SetRow(_letterNumberMenu.LetterPanel, 1);
            ((Grid)Content).Children.Add(_letterNumberMenu.LetterPanel);
            
            // Pagination related
            PrevPageButton.IsEnabled = false;
            NextPageButton.IsEnabled = false;
            _prevPageButton = PrevPageButton; // Connects the field to the XAML-defined button
            _nextPageButton = NextPageButton; // Connects the field to the XAML-defined button

            // Initialize _gameButtonFactory with settings
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings);

            // Check if a system is already selected, otherwise show the message
            if (SystemComboBox.SelectedItem == null)
            {
                AddNoSystemMessage();
            }

            // Check for updates
            Loaded += async (_, _) => await UpdateChecker.CheckForUpdatesAsync(this);
            
            // Attach the Closing event handler to ensure resources are disposed of
            Closing += MainWindow_Closing;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ExtractCompressedFile.Instance.Cleanup();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GamePadController.Instance.Stop();
            GamePadController.Instance.Dispose();
        }
        
        private void MainWindow_Restart()
        {
            // Prepare the process start info
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = processModule.FileName,
                    UseShellExecute = true
                };

                // Start the new application instance
                Process.Start(startInfo);

                // Shutdown the current application instance
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmulatorComboBox.ItemsSource = null;
            EmulatorComboBox.SelectedIndex = -1;

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

                    // Display the system info
                    string systemFolderPath = selectedConfig.SystemFolder;
                    var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"{ext}").ToList();
                    int gameCount = LoadFiles.CountFiles(systemFolderPath, fileExtensions);
                    DisplaySystemInfo(systemFolderPath, gameCount);
                    
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

        private void DisplaySystemInfo(string systemFolderPath, int gameCount)
        {
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\nDirectory: {systemFolderPath}\nTotal number of games in the System directory, excluding files in subdirectories: {gameCount}\n\nPlease select a Letter above",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10)
            });
        }

        private void EmulatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the logic when an Emulator is selected
            // for future use
        }

        private async Task LoadGameFiles(string startLetter = null)
        {
            try
            {
                GameFileGrid.Children.Clear();

                if (SystemComboBox.SelectedItem == null)
                {
                    AddNoSystemMessage();
                    return;
                }
                string selectedSystem = SystemComboBox.SelectedItem.ToString();
                var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
                if (selectedConfig == null)
                {
                    HandleError(new Exception("Selected system configuration not found"), "Error while loading selected system configuration");
                    return;
                }

                // Get the SystemFolder from the selected configuration
                string systemFolderPath = selectedConfig.SystemFolder;
                
                // Extract the file extensions from the selected system configuration
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList();
                
                // List of files with that match the system extensions
                // then sort the list alphabetically 
                List<string> allFiles = await LoadFiles.GetFilesAsync(systemFolderPath, fileExtensions);
                allFiles = LoadFiles.FilterFiles(allFiles, startLetter);
                allFiles.Sort();

                // Count the list of files
                _totalFiles = allFiles.Count;

                // Pagination related
                if (_totalFiles > _paginationThreshold)
                {
                    // Enable pagination and adjust file list based on the current page
                    allFiles = allFiles.Skip((_currentPage - 1) * _filesPerPage).Take(_filesPerPage).ToList();
                    // Update or create pagination controls
                    InitializePaginationButtons();
                }

                // Create a new instance of GameButtonFactory within the LoadGameFiles method.
                var factory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings);
                
                // Create Button action for each cell
                foreach (var filePath in allFiles)
                {
                    // Adjust the CreateGameButton call.
                    Button gameButton = await factory.CreateGameButtonAsync(filePath, SystemComboBox.SelectedItem.ToString(), selectedConfig);
                    GameFileGrid.Children.Add(gameButton);
                }
                
                // Update the UI to reflect the current pagination status
                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error while loading ROM files");
            }
        }
        
        private void ResetPaginationButtons()
        {
            _prevPageButton.IsEnabled = false;
            _nextPageButton.IsEnabled = false;
            _currentPage = 1;
        }
        private void InitializePaginationButtons()
        {
            _prevPageButton.IsEnabled = _currentPage > 1;
            _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        }
        
        private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    await LoadGameFiles(_currentFilter);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "Previous page button error");
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
                    await LoadGameFiles(_currentFilter);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "Next page button error");
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
                FontWeight = FontWeights.Bold,
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
            Page600.IsChecked = (selectedSize == 600);
            Page700.IsChecked = (selectedSize == 700);
            Page800.IsChecked = (selectedSize == 800);
            Page900.IsChecked = (selectedSize == 900);
            Page1000.IsChecked = (selectedSize == 1000);
        }
       
        public static async void HandleError(Exception ex, string message)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            _ = new LogErrors();
            await LogErrors.LogErrorAsync(ex, message);
        }

        #region Menu Items

        private void EditSystem_Click(object sender, RoutedEventArgs e)
        {
            EditSystem editSystemWindow = new();
            editSystemWindow.ShowDialog();
        }
        
        private void EditLinks_Click(object sender, RoutedEventArgs e)
        {
            EditLinks editLinksWindow = new();
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
                    FileName = "https://www.buymeacoffee.com/purelogiccode",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                HandleError(ex, "Unable to open the donation link");
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

        private void HideGamesNoCover_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                menuItem.IsChecked = !menuItem.IsChecked;
                _settings.HideGamesWithNoCover = menuItem.IsChecked;
                _settings.Save();

                // Hide games with no cover
                if (menuItem.IsChecked)
                {
                    foreach (var child in _gameFileGrid.Children)
                    {
                        if (child is Button btn && btn.Tag?.ToString() == "DefaultImage")
                        {
                            btn.Visibility = Visibility.Collapsed; // Hide the button
                        }
                    }
                }
                else
                {
                    // Show all games
                    foreach (var child in _gameFileGrid.Children)
                    {
                        if (child is Button btn)
                        {
                            btn.Visibility = Visibility.Visible; // Show the button
                        }
                    }
                }
            }
        }

        private void EnableGamePadNavigation_Click(object sender, RoutedEventArgs e)
        {
            // Event handler logic to use the singleton instance
            if (sender is MenuItem menuItem)
            {
                menuItem.IsChecked = !menuItem.IsChecked;
                _settings.EnableGamePadNavigation = menuItem.IsChecked;
                _settings.Save();

                if (menuItem.IsChecked)
                {
                    GamePadController.Instance.Start();
                }
                else
                {
                    GamePadController.Instance.Stop();
                }
            }
        }

        private void ThumbnailSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                // Extract the numeric value from the header
                var sizeText = clickedItem.Header.ToString();
                if (int.TryParse(new string(sizeText!.Where(char.IsDigit).ToArray()), out int newSize))
                {
                    _gameButtonFactory.ImageHeight = newSize; // Update the image height
                    _settings.ThumbnailSize = newSize; // Update the settings
                    _settings.Save(); // Save the settings
                    UpdateMenuCheckMarks(newSize);
                }
            }
        }
        
        private void GamesPerPage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                // Extract the numeric value from the header
                var pageText = clickedItem.Header.ToString();
                if (int.TryParse(new string(pageText!.Where(char.IsDigit).ToArray()), out int newPage))
                {
                    _filesPerPage = newPage; // Update the page size
                    _paginationThreshold = newPage; // update pagination threshold
                    _settings.GamesPerPage = newPage; // Update the settings
                    
                    _settings.Save(); // Save the settings
                    UpdateMenuCheckMarks2(newPage);
                    
                    // Restart Application
                    MainWindow_Restart();
                }
            }
        }

        #endregion

    }
}