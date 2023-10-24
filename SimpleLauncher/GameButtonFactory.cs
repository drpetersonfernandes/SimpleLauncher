using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    internal class GameButtonFactory
    {
        // Constants
        private const string DefaultImagePath = "default.png";
        private const int ImageHeight = 200;
        private const int StackPanelWidth = 300;
        private const int StackPanelHeight = 250;
        private const int ButtonWidth = 300;
        private const int ButtonHeight = 250;

        private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private string DetermineImagePath(string fileNameWithoutExtension, string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return Path.Combine(_baseDirectory, "images", DefaultImagePath); // Return the default image if no system is selected.

            string imagePath = Path.Combine(_baseDirectory, "images", systemName, $"{fileNameWithoutExtension}.png");

            if (File.Exists(imagePath))
                return imagePath;

            return Path.Combine(_baseDirectory, "images", DefaultImagePath); // Return the default image if the specific image doesn't exist.
        }


        // Assuming your ComboBoxes and Configs are in MainWindow, you can pass them as properties.
        public ComboBox EmulatorComboBox { get; set; }
        public ComboBox SystemComboBox { get; set; }
        public List<SystemConfig> SystemConfigs { get; set; } // Assuming SystemConfig is the correct type

        public GameButtonFactory(ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
        {
            EmulatorComboBox = emulatorComboBox;
            SystemComboBox = systemComboBox;
            SystemConfigs = systemConfigs;
        }

        public Button CreateGameButton(string filePath, string systemName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

            // Determine the image path based on the filename
            string imagePath = DetermineImagePath(fileNameWithoutExtension, systemName);

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

                        var systemConfig = SystemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

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
    }
}
