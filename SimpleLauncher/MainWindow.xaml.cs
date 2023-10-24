using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    public partial class MainWindow : Window
    {
        // Instance variables
        readonly private GamePadController _inputControl;
        private readonly MenuActions _menuActions;
        readonly private List<SystemConfig> _systemConfigs;
        private readonly GameHandler _gameHandler = new GameHandler();
        private static readonly object _lockObject = new object();

        // Constants
        private const string DefaultImagePath = "default.png";
        private const int ImageHeight = 200;
        private const int StackPanelWidth = 300;
        private const int StackPanelHeight = 250;
        private const int ButtonWidth = 300;
        private const int ButtonHeight = 250;

        public MainWindow()
        {
            InitializeComponent();
            
            // Attach the Closing event handler to ensure resources are disposed of
            this.Closing += MainWindow_Closing;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            // Initialize the GamePadController.cs
            _inputControl = new GamePadController((ex, msg) => LogErrorAsync(ex, msg).Wait());
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
            _menuActions = new MenuActions(this, zipFileGrid);

            // Create and integrate LetterNumberItems
            LetterNumberItems letterNumberItems = new LetterNumberItems();
            letterNumberItems.OnLetterSelected += (selectedLetter) =>
            {
                LoadgameFiles(selectedLetter);
            };

            // Add the StackPanel from LetterNumberItems to the MainWindow's Grid
            Grid.SetRow(letterNumberItems.LetterPanel, 1);
            ((Grid)this.Content).Children.Add(letterNumberItems.LetterPanel);
            // Simulate a click on the "A" button
            letterNumberItems.SimulateClick("A");

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
                    LoadgameFiles("A");
                }
            }
        }

        private void EmulatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the logic when an Emulator is selected
            //EmulatorConfig selectedProgram = (EmulatorConfig)EmulatorComboBox.SelectedItem;
            // For now, you can leave it empty if there's no specific logic to implement yet.
        }

        private string DetermineImagePath(string fileNameWithoutExtension)
        {
            if (SystemComboBox.SelectedItem != null)
            {
                string selectedSystem = SystemComboBox.SelectedItem.ToString();

                // Create the path based on the selected system and the file name
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(basePath, "images", selectedSystem);
                string imagePath = Path.Combine(imagesDirectory, fileNameWithoutExtension + ".png");

                return File.Exists(imagePath) ? imagePath : Path.Combine(basePath, "images", DefaultImagePath);
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", DefaultImagePath);
            // Return default image path if no system is selected
        }

        private async void LoadgameFiles(string startLetter = null)
        {
            try
            {
                zipFileGrid.Children.Clear();

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

                string systemFolderPath = selectedConfig.SystemFolder; // Get the SystemFolder from the selected configuration
                List<string> allFiles = await _gameHandler.GetFilesAsync(systemFolderPath); // Modify the GetFilesAsync method to accept a path parameter

                if (!allFiles.Any())
                {
                    AddNoRomsMessage();
                    return;
                }

                allFiles = _gameHandler.FilterFiles(allFiles, startLetter);
                allFiles.Sort();

                foreach (var filePath in allFiles)
                {
                    Button gameButton = CreateGameButton(filePath);
                    zipFileGrid.Children.Add(gameButton);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error while loading ROM files");
            }
        }

        private void AddNoRomsMessage()
        {
            zipFileGrid.Children.Add(new TextBlock
            {
                Text = "Could not find any ROM",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10) // This will add 10 pixels of padding on all sides
            });
        }

        private void AddNoSystemMessage()
        {
            zipFileGrid.Children.Add(new TextBlock
            {
                Text = "Please select a System",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10) // This will add 10 pixels of padding on all sides
            });
        }

        private Button CreateGameButton(string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

            // Determine the image path based on the filename
            string imagePath = DetermineImagePath(fileNameWithoutExtension);

            var image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath)),
                Height = ImageHeight,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var textBlock = new TextBlock
            {
                Text = fileNameWithoutExtension,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = fileNameWithoutExtension // Display the full filename on hover
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = StackPanelWidth,
                Height = StackPanelHeight,
                MaxHeight = StackPanelHeight // Limits the maximum height
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            var button = new Button
            {
                Content = stackPanel,
                Width = ButtonWidth,
                Height = ButtonHeight,
                MaxHeight = ButtonHeight,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };

            button.Click += async (sender, args) =>
            {
                ProcessStartInfo psi = null;

                try
                {
                    if (EmulatorComboBox.SelectedItem != null)
                    {
                        string selectedEmulatorName = EmulatorComboBox.SelectedItem.ToString();
                        string selectedSystem = SystemComboBox.SelectedItem.ToString();

                        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

                        if (systemConfig == null)
                        {
                            MessageBox.Show("Please select a valid system", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

                        if (emulatorConfig == null)
                        {
                            MessageBox.Show("Selected emulator configuration not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string gamePathToLaunch = filePath;  // Default to the original path

                        // Determine if extraction is needed based on system configuration
                        if (systemConfig.ExtractFileBeforeLaunch)
                        {
                            string fileExtension = Path.GetExtension(filePath).ToLower();

                            if (fileExtension == ".zip" || fileExtension == ".7z")
                            {
                                // Here, we'll use the first format in the FileFormatsToLaunch list for extraction.
                                // Modify if needed.
                                string formatToLaunch = systemConfig.FileFormatsToLaunch.FirstOrDefault();

                                if (string.IsNullOrEmpty(formatToLaunch))
                                {
                                    MessageBox.Show("No format specified for launch in the system configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                gamePathToLaunch = ExtractFile.Instance.ExtractArchiveToTemp(filePath, formatToLaunch);

                                if (string.IsNullOrEmpty(gamePathToLaunch))
                                {
                                    MessageBox.Show("Couldn't find a file with the specified extension after extraction.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }


                        string programLocation = emulatorConfig.EmulatorLocation;
                        string parameters = emulatorConfig.EmulatorParameters;
                        string filename = Path.GetFileName(gamePathToLaunch);
                        string arguments = $"{parameters} \"{gamePathToLaunch}\"";

                        // Create ProcessStartInfo
                        psi = new ProcessStartInfo
                        {
                            FileName = programLocation,
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        // Launch the external program
                        Process process = new Process { StartInfo = psi };
                        process.Start();

                        // Read the output streams
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();

                        // Wait for the process to exit
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            MessageBox.Show("The emulator could not open this file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                            string errorMessage = $"Error launching external program: Exit code {process.ExitCode}\n";
                            errorMessage += $"Process Start Info:\nFileName: {psi.FileName}\nArguments: {psi.Arguments}\n";
                            File.AppendAllText(errorLogPath, errorMessage);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select an emulator", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                    string errorDetails = $"Exception Details:\n{ex}\n";
                    if (psi != null)
                    {
                        errorDetails += $"Process Start Info:\nFileName: {psi.FileName}\nArguments: {psi.Arguments}\n";
                    }
                    File.AppendAllText(errorLogPath, errorDetails);
                }
            };

            return button;
        }

        private async Task LogErrorAsync(Exception ex, string contextMessage = null)
        {
            await Task.Run(() =>
            {
                string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string errorMessage = $"Date: {DateTime.Now}\nContext: {contextMessage}\nException Details:\n{ex}\n\n";

                lock (_lockObject)
                {
                    File.AppendAllText(errorLogPath, errorMessage);
                }
            });
        }

        private async void HandleError(Exception ex, string message)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            await LogErrorAsync(ex, message);
        }

    }
}