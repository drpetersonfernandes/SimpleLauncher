using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public partial class MainWindow : Window
    {
        // Instance variables
        readonly private GamePadController _inputControl;
        private readonly MenuActions _menuActions;
        readonly private List<SystemConfig> _systemConfigs;
        private readonly GameHandler _gameHandler = new GameHandler();
        readonly private LogErrors _logger = new LogErrors();
        readonly private LetterNumberItems _letterNumberItems = new LetterNumberItems();

        public MainWindow()
        {
            InitializeComponent();

            // Attach the Closing event handler to ensure resources are disposed of
            this.Closing += MainWindow_Closing;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            // Initialize the GamePadController.cs
            _inputControl = new GamePadController((ex, msg) => _logger.LogErrorAsync(ex, msg).Wait());
            _inputControl.Start();

            // Set the path to the 7z.dll
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.dll");
            SevenZipBase.SetLibraryPath(dllPath);

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

            // Initialize the MenuActions with this window context
            _menuActions = new MenuActions(this, gameFileGrid);

            // Create and integrate LetterNumberItems
            _letterNumberItems.OnLetterSelected += async (selectedLetter) =>
            {
                await LoadgameFiles(selectedLetter);
            };


            // Add the StackPanel from LetterNumberItems to the MainWindow's Grid
            Grid.SetRow(_letterNumberItems.LetterPanel, 1);
            ((Grid)this.Content).Children.Add(_letterNumberItems.LetterPanel);
            // Simulate a click on the "A" button
            _letterNumberItems.SimulateClick("A");

        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ExtractFile.Instance.Cleanup();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_inputControl != null)
            {
                _inputControl.Stop();
                _inputControl.Dispose();
            }
        }
        private void HideGames_Click(object sender, RoutedEventArgs e)
        {
            _menuActions.HideGames_Click(sender, e);
        }

        private void ShowGames_Click(object sender, RoutedEventArgs e)
        {
            _menuActions.ShowGames_Click(sender, e);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            _menuActions.About_Click(sender, e);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _menuActions.Exit_Click(sender, e);
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

                    // Load game files for the selected system
                    //LoadgameFiles("A");
                }
            }
            // Reset the letter to "A" each time the system is changed
            _letterNumberItems.SimulateClick("A");
        }


        private void EmulatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the logic when an Emulator is selected
            //EmulatorConfig selectedProgram = (EmulatorConfig)EmulatorComboBox.SelectedItem;
            // For now, you can leave it empty if there's no specific logic to implement yet.
        }

        private async Task LoadgameFiles(string startLetter = null)
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

                List<string> allFiles = await _gameHandler.GetFilesAsync(systemFolderPath, fileExtensions);

                if (!allFiles.Any())
                {
                    AddNoRomsMessage();
                    return;
                }

                allFiles = _gameHandler.FilterFiles(allFiles, startLetter);
                allFiles.Sort();

                // Create a new instance of GameButtonFactory within the LoadgameFiles method.
                var factory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs);

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

        //private void ChangeTheme(bool isDarkTheme)
        //{
        //    ResourceDictionary newTheme = new ResourceDictionary();
        //    if (isDarkTheme)
        //    {
        //        newTheme.Source = new Uri("pack://application:,,,/DarkTheme.xaml");
        //    }
        //    else
        //    {
        //        newTheme.Source = new Uri("pack://application:,,,/LightTheme.xaml");
        //    }

        //    Application.Current.Resources.MergedDictionaries.Clear();
        //    Application.Current.Resources.MergedDictionaries.Add(newTheme);
        //}


        private async void HandleError(Exception ex, string message)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            var logger = new LogErrors();
            await logger.LogErrorAsync(ex, message);
        }

    }
}