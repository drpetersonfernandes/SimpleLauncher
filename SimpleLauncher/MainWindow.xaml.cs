using System;
using System.Collections.Generic;
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
        private readonly int _filesPerPage = 250;
        private int _totalFiles;
        private const int PaginationThreshold = 250;
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

            // Attach the Closing event handler to ensure resources are disposed of
            Closing += MainWindow_Closing;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

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
            _gameFileGrid = FindName("gameFileGrid") as WrapPanel;

            // Create and integrate LetterNumberMenu
            _letterNumberMenu.OnLetterSelected += async (selectedLetter) =>
            {
                await LoadGameFiles(selectedLetter);
            };

            // Add the StackPanel from LetterNumberMenu to the MainWindow's Grid
            Grid.SetRow(_letterNumberMenu.LetterPanel, 1);
            ((Grid)Content).Children.Add(_letterNumberMenu.LetterPanel);
            
            // pagination related
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
                Text = $"\nDirectory: {systemFolderPath}\nTotal number of games in the System directory, excluding files in subdirectories: {gameCount}\n\nPlease select a Letter",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10)
            });
        }

        private void EmulatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the logic when an Emulator is selected
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

                List<string> allFiles = await LoadFiles.GetFilesAsync(systemFolderPath, fileExtensions);

                allFiles = LoadFiles.FilterFiles(allFiles, startLetter);
                allFiles.Sort();
                
                _totalFiles = allFiles.Count;
                
                if (_totalFiles > PaginationThreshold)
                {
                    // Enable pagination and adjust file list based on the current page
                    allFiles = allFiles.Skip((_currentPage - 1) * _filesPerPage).Take(_filesPerPage).ToList();
                    // Update or create pagination controls here (not shown in this snippet)
                    InitializePaginationButtons();
                }

                // Create a new instance of GameButtonFactory within the LoadGameFiles method.
                var factory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings);

                foreach (var filePath in allFiles)
                {
                    // Adjust the CreateGameButton call.
                    Button gameButton = await factory.CreateGameButtonAsync(filePath, SystemComboBox.SelectedItem.ToString(), selectedConfig);
                    GameFileGrid.Children.Add(gameButton);
                }
                
                // Optionally, update the UI to reflect the current pagination status
                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error while loading ROM files");
            }
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
                MessageBox.Show($"An error occurred: {ex.Message}");
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
                MessageBox.Show($"An error occurred: {ex.Message}");
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

        private static async void HandleError(Exception ex, string message)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _ = new LogErrors();
            await LogErrors.LogErrorAsync(ex, message);
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

        #region Menu Items

        private void EditSystem_Click(object sender, RoutedEventArgs e)
        {
            EditSystem editSystemWindow = new();
            editSystemWindow.ShowDialog();
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
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.buymeacoffee.com/purelogiccode",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open the link: " + ex.Message);
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
                _settings.Save();  // Save the settings

                // Your logic to hide games with no cover
                if (menuItem.IsChecked)
                {
                    // Code to hide games
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
                    // Code to show games
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
            // Adjust event handler logic to use the singleton instance
            if (sender is MenuItem menuItem)
            {
                menuItem.IsChecked = !menuItem.IsChecked;
                _settings.EnableGamePadNavigation = menuItem.IsChecked;
                _settings.Save();  // Save the settings

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

        #endregion

    }
}