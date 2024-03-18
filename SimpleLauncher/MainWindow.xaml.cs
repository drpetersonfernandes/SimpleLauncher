using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private readonly LetterNumberMenu _letterNumberMenu;
        private readonly WrapPanel _gameFileGrid;
        private readonly GameButtonFactory _gameButtonFactory;
        private readonly AppSettings _settings;
        private readonly List<MameConfig> _machines;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load settings.xml
            _settings = new AppSettings("settings.xml");
            
            // Load mame.xml
            string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.xml");
            _machines = MameConfig.LoadFromXml(xmlPath).Result;
            
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
            // HideGamesNoCover.IsChecked = _settings.HideGamesWithNoCover;
            EnableGamePadNavigation.IsChecked = _settings.EnableGamePadNavigation;
            UpdateMenuCheckMarks(_settings.ThumbnailSize);
            UpdateMenuCheckMarks2(_settings.GamesPerPage);
            UpdateMenuCheckMarks3(_settings.ShowGames);
            _filesPerPage = _settings.GamesPerPage; // load GamesPerPage value from setting.xml
            _paginationThreshold = _settings.GamesPerPage; // load GamesPerPage value from setting.xml

            // Initialize the GamePadController.cs
            // Setting the error logger
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
            
            // // Add the StackPanel from LetterNumberMenu to the MainWindow's Grid
            // Grid.SetRow(_letterNumberMenu.LetterPanel, 1);
            // ((Grid)Content).Children.Add(_letterNumberMenu.LetterPanel);
            
            // Initialize LetterNumberMenu and add it to the UI
            _letterNumberMenu = new LetterNumberMenu();
            LetterNumberMenu.Children.Clear(); // Clear if necessary
            LetterNumberMenu.Children.Add(_letterNumberMenu.LetterPanel); // Add the LetterPanel directly
            
            // Create and integrate LetterNumberMenu
            _letterNumberMenu.OnLetterSelected += async (selectedLetter) =>
            {
                // Reset pagination controls
                ResetPaginationButtons();
                
                await LoadGameFiles(selectedLetter);
                
                // Move scroller to top
                Scroller.ScrollToTop();
            };
            
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
            
            // Attach the Load and Close event handler.
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        }

        // Delete temp files from ExtractCompressedFile class.
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ExtractCompressedFile.Instance2.Cleanup();
        }

        // Save state and size of Main Window to settings.xml
        private void SaveWindowState()
        {
            _settings.MainWindowWidth = this.Width;
            _settings.MainWindowHeight = this.Height;
            _settings.MainWindowState = this.WindowState.ToString();
            _settings.Save();
        }

        // Load state and size of MainWindow from settings.xml
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = _settings.MainWindowWidth;
            this.Height = _settings.MainWindowHeight;
            this.WindowState = (WindowState)Enum.Parse(typeof(WindowState), _settings.MainWindowState);
        }

        // Dispose gamepad resources. Save MainWindow state and size to setting.xml.
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GamePadController.Instance2.Stop();
            GamePadController.Instance2.Dispose();
            SaveWindowState();
        }

        // Restart Application
        private void MainWindow_Restart()
        {
            // Explicitly save the MainWindow state and size before restarting
            SaveWindowState();

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

        private void DisplaySystemInfo(string systemFolder, int gameCount, SystemConfig selectedConfig)
        {
            GameFileGrid.Children.Clear();

            var emulatorInfoBuilder = new StringBuilder();
            for (int i = 0; i < selectedConfig.Emulators.Count; i++)
            {
                emulatorInfoBuilder.AppendLine($"Emulator {i + 1} Name: {selectedConfig.Emulators[i].EmulatorName}\n" +
                                               $"Emulator {i + 1} Location: {selectedConfig.Emulators[i].EmulatorLocation}\n" +
                                               $"Emulator {i + 1} Parameters: {selectedConfig.Emulators[i].EmulatorParameters}\n");
            }
            
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\nSystem Folder: {systemFolder}\n" +
                       $"Total number of games in the System Folder, excluding files in subdirectories: {gameCount}\n\n" +
                       $"System Image Folder: {selectedConfig.SystemImageFolder}\n" +
                       $"System is MAME? {selectedConfig.SystemIsMame}\n" +
                       $"Format to Search in the System Folder: {string.Join(", ", selectedConfig.FileFormatsToSearch)}\n" +
                       $"Extract File Before Launch? {selectedConfig.ExtractFileBeforeLaunch}\n" +
                       $"Format to Launch After Extraction: {string.Join(", ", selectedConfig.FileFormatsToLaunch)}\n\n" +
                       emulatorInfoBuilder +
                       $"Please select a Button above to see the games.",
                Padding = new Thickness(10)
            });
        }

        private void EmulatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the logic when an Emulator is selected
            // for future use
        }

        private async Task LoadGameFiles(string startLetter = null, string searchQuery = null)
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
                
                if (!string.IsNullOrWhiteSpace(startLetter))
                {
                    allFiles = LoadFiles.FilterFiles(allFiles, startLetter);
                }
                
                // Search engine
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    bool systemIsMame = selectedConfig.SystemIsMame;
                    allFiles = allFiles.Where(file =>
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
                
                    }).ToList();
                }
                
                //Sort the collection of files
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
                TotalFilesLabel.Content = allFiles.Count == 0 ? $"Displaying files 0 to {endIndex} out of {_totalFiles} total" : $"Displaying files {startIndex} to {endIndex} out of {_totalFiles} total";                

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
                    await LoadGameFiles(_currentFilter);
                    Scroller.ScrollToTop();
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
                    Scroller.ScrollToTop();
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
                Text = "\nI did not find any games with this search query",
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
        
        private void UpdateMenuCheckMarks3(string selectedValue)
        {
            ShowAll.IsChecked = (selectedValue == "ShowAll");
            ShowWithCover.IsChecked = (selectedValue == "ShowWithCover");
            ShowWithoutCover.IsChecked = (selectedValue == "ShowWithoutCover");
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
            // Save MainWindow state and size before call the EditSystem Window
            SaveWindowState();
                
            EditSystem editSystemWindow = new();
            editSystemWindow.ShowDialog();
        }
        
        private void EditLinks_Click(object sender, RoutedEventArgs e)
        {
            // Save MainWindow state and size before call the EditLinks Window
            SaveWindowState();
                
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

        // private void HideGamesNoCover_Click(object sender, RoutedEventArgs e)
        // {
        //     if (sender is MenuItem menuItem)
        //     {
        //         menuItem.IsChecked = !menuItem.IsChecked;
        //         _settings.HideGamesWithNoCover = menuItem.IsChecked;
        //         _settings.Save();
        //
        //         // Hide games with no cover
        //         if (menuItem.IsChecked)
        //         {
        //             foreach (var child in _gameFileGrid.Children)
        //             {
        //                 if (child is Button btn && btn.Tag?.ToString() == "DefaultImage")
        //                 {
        //                     btn.Visibility = Visibility.Collapsed; // Hide the button
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             // Show all games
        //             foreach (var child in _gameFileGrid.Children)
        //             {
        //                 if (child is Button btn)
        //                 {
        //                     btn.Visibility = Visibility.Visible; // Show the button
        //                 }
        //             }
        //         }
        //     }
        // }
        
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
            // Event handler logic to use the singleton instance
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

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
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

            // Show the "Please Wait" window
            var pleaseWaitWindow = new PleaseWaitWindow();
            pleaseWaitWindow.Show();

            try
            {
                await LoadGameFiles(searchQuery: searchQuery);
            }
            finally
            {
                // Close the "Please Wait" window
                pleaseWaitWindow.Close();
            }
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
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

                // Show the "Please Wait" window
                var pleaseWaitWindow = new PleaseWaitWindow();
                pleaseWaitWindow.Show();

                try
                {
                    await LoadGameFiles(searchQuery: searchQuery);
                }
                finally
                {
                    // Close the "Please Wait" window
                    pleaseWaitWindow.Close();
                }
            }
        }


        
    }
}