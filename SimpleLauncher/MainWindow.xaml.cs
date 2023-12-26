using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public partial class MainWindow : Window
    {
        // Instance variables
        private GamePadController _inputControl;
        readonly private List<SystemConfig> _systemConfigs;
        private readonly LoadFiles _loadFiles = new();
        readonly private LetterNumberMenu _LetterNumberMenu = new();
        readonly private WrapPanel _gameFileGrid;
        private GameButtonFactory _gameButtonFactory;
        private List<string> _currentGameFilePaths = [];
        private readonly AppSettings _settings;

        // Menu Item Constants
        private const bool DefaultHideGamesWithNoCover = false;
        private const bool DefaultEnableGamePadNavigation = true;
        private const int DefaultThumbnailSize = 350;

        public MainWindow()
        {
            InitializeComponent();

            // Load settings
            _settings = new AppSettings("settings.xml");

            // Apply settings to your application
            HideGamesNoCover.IsChecked = _settings.HideGamesWithNoCover;
            EnableGamePadNavigation.IsChecked = _settings.EnableGamePadNavigation;
            UpdateMenuCheckMarks(_settings.ThumbnailSize);

            // Attach the Closing event handler to ensure resources are disposed of
            this.Closing += MainWindow_Closing;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            // Initialize the GamePadController.cs
            _inputControl = new GamePadController((ex, msg) => LogErrors.LogErrorAsync(ex, msg).Wait());
            _inputControl.Start();

            // Initialize _gameFileGrid
            _gameFileGrid = this.FindName("gameFileGrid") as WrapPanel;

            // Load system.xml and Populate the SystemComboBox
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");
                _systemConfigs = SystemConfig.LoadSystemConfigs(path);
                SystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error while loading system configurations");
            }

            // Create and integrate LetterNumberMenu
            _LetterNumberMenu.OnLetterSelected += async (selectedLetter) =>
            {
                await LoadGameFiles(selectedLetter);
            };

            // Add the StackPanel from LetterNumberMenu to the MainWindow's Grid
            Grid.SetRow(_LetterNumberMenu.LetterPanel, 1);
            ((Grid)this.Content).Children.Add(_LetterNumberMenu.LetterPanel);
            // Simulate a click on the "A" button
            _LetterNumberMenu.SimulateClick("A");

            // Initialize _gameButtonFactory with settings
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _settings);

        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ExtractCompressedFile.Instance.Cleanup();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_inputControl != null)
            {
                _inputControl.Stop();
                _inputControl.Dispose();
            }
        }

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmulatorComboBox.ItemsSource = null;
            EmulatorComboBox.SelectedIndex = -1;

            if (SystemComboBox.SelectedItem != null)
            {
                string selectedSystem = SystemComboBox.SelectedItem.ToString();

                // Get the corresponding SystemConfig for the selected system
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
                }
            }
            // Reset the letter to "A" each time the system is changed
            _LetterNumberMenu.SimulateClick("A");
        }

        private void EmulatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the logic when an Emulator is selected
        }

        private async Task LoadGameFiles(string startLetter = null)
        {
            try
            {
                gameFileGrid.Children.Clear();

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

                List<string> allFiles = await _loadFiles.GetFilesAsync(systemFolderPath, fileExtensions);

                if (allFiles.Count == 0)
                {
                    AddNoRomsMessage();
                    return;
                }

                allFiles = LoadFiles.FilterFiles(allFiles, startLetter);
                allFiles.Sort();

                // Update the list of current game file paths
                _currentGameFilePaths = allFiles;

                // Create a new instance of GameButtonFactory within the LoadGameFiles method.
                var factory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _settings);

                foreach (var filePath in allFiles)
                {
                    // Adjust the CreateGameButton call.
                    Button gameButton = await factory.CreateGameButtonAsync(filePath, SystemComboBox.SelectedItem.ToString());
                    gameFileGrid.Children.Add(gameButton);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error while loading ROM files");
            }
        }

        private void AddNoRomsMessage()
        {
            ClearPreviousMessages("Could not find any ROM");
            gameFileGrid.Children.Add(new TextBlock
            {
                Text = "Could not find any ROM",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10)
            });
        }

        private void AddNoSystemMessage()
        {
            ClearPreviousMessages("Please select a System");
            gameFileGrid.Children.Add(new TextBlock
            {
                Text = "Please select a System",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10)
            });
        }

        private void ClearPreviousMessages(string message)
        {
            var existingMessage = gameFileGrid.Children
                                .OfType<TextBlock>()
                                .FirstOrDefault(tb => tb.Text == message);

            if (existingMessage != null)
            {
                gameFileGrid.Children.Remove(existingMessage);
            }
        }

        private static async void HandleError(Exception ex, string message)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _ = new LogErrors();
            await LogErrors.LogErrorAsync(ex, message);
        }

        #region Menu Items

        public void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Simple Launcher.\nAn Open Source Emulator Launcher.\nVersion 2.3", "About");
        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            if (sender is MenuItem menuItem)
            {
                menuItem.IsChecked = !menuItem.IsChecked;
                _settings.EnableGamePadNavigation = menuItem.IsChecked;
                _settings.Save();  // Save the settings

                if (menuItem.IsChecked)
                {
                    // If the gamepad navigation is being enabled, start the controller.
                    _inputControl ??= new GamePadController((ex, msg) => LogErrors.LogErrorAsync(ex, msg).Wait());
                    _inputControl.Start();
                }
                else
                {
                    // If the gamepad navigation is being disabled, stop and dispose of the controller.
                    _inputControl?.Dispose();
                    _inputControl = null;
                }
            }
        }

        private void ThumbnailSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                // Extract the numeric value from the header
                var sizeText = clickedItem.Header.ToString();
                if (int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out int newSize))
                {
                    _gameButtonFactory.ImageHeight = newSize; // Update the image height
                    _settings.ThumbnailSize = newSize; // Update the settings
                    _settings.Save(); // Save the settings
                    RefreshGameButtons(); // Refresh the UI

                    UpdateMenuCheckMarks(newSize);
                }
            }
        }

        #endregion

        private async void RefreshGameButtons()
        {
            // Clear existing buttons
            _gameFileGrid.Children.Clear();

            // Initialize _gameButtonFactory with default values
            _gameButtonFactory ??= new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _settings);

            // Recreate the buttons with the new image height
            foreach (var filePath in _currentGameFilePaths)
            {
                var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, SystemComboBox.SelectedItem.ToString());
                _gameFileGrid.Children.Add(gameButton);
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

    }
}