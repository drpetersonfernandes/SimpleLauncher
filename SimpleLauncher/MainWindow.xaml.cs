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
        private readonly MenuActions _menuActions;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the MenuActions with this window context
            _menuActions = new MenuActions(this, zipFileGrid);

            // initialize the controller classes.
            var inputControl = new GamePadController();
            inputControl.Start();

            // Set the title dynamically
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderName = new DirectoryInfo(currentDirectory).Name.TrimEnd('\\');
            this.Title = $"{folderName} Launcher";

            // Create buttons for each letter and add a Click event
            StackPanel letterPanel = new StackPanel { Orientation = Orientation.Horizontal };
            Button selectedButton = null; // A reference to the currently selected button
            Dictionary<string, Button> letterButtons = new Dictionary<string, Button>();

            foreach (char c in Enumerable.Range('A', 26).Select(x => (char)x))
            {
                Button button = new Button { Content = c.ToString(), Width = 30, Height = 30 };

                button.Click += (sender, e) =>
                {
                    // Reset the style of the previously selected button
                    if (selectedButton != null)
                    {
                        selectedButton.ClearValue(Button.BackgroundProperty);
                    }

                    // Update the style of the currently selected button
                    button.Background = System.Windows.Media.Brushes.Green;

                    // Update the selectedButton reference
                    selectedButton = button;

                    LoadZipFiles(c.ToString());
                };

                // Add button to the dictionary
                letterButtons.Add(c.ToString(), button);
                letterPanel.Children.Add(button);
            }

            // Add button for numbers
            Button numButton = new Button { Content = "#", Width = 30, Height = 30 };
            numButton.Click += (sender, e) =>
            {
                // Reset the style of the previously selected button
                if (selectedButton != null)
                {
                    selectedButton.ClearValue(Button.BackgroundProperty);
                }

                // Update the style of the currently selected button
                numButton.Background = System.Windows.Media.Brushes.Green;

                // Update the selectedButton reference
                selectedButton = numButton;
                LoadZipFiles("#");
            };

            letterPanel.Children.Add(numButton);

            // Add the StackPanel to the Grid
            Grid.SetRow(letterPanel, 1);
            ((Grid)this.Content).Children.Add(letterPanel);

            LoadParameters();

            // Read the parameters.txt file and set the default selected item
            string parametersPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "parameters.txt");
            if (File.Exists(parametersPath))
            {
                string[] lines = System.IO.File.ReadAllLines(parametersPath);
                string defaultProgramName = null;

                foreach (string line in lines)
                {
                    if (line.StartsWith("ProgramName: "))
                    {
                        defaultProgramName = line.Substring("ProgramName: ".Length);
                        break;
                    }
                }

                if (defaultProgramName != null)
                {
                    // EmulatorComboBox has a property `Items` that you've populated
                    foreach (var item in MyComboBox.Items)
                    {
                        if (item.ToString() == defaultProgramName)  // Replace `item.ToString()` with how you'd get the ProgramName from your item
                        {
                            MyComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }

            // Simulate a click on the "A" button
            if (letterButtons.ContainsKey("A"))
            {
                Button aButton = letterButtons["A"];
                aButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void CheckMissingImages_Click(object sender, RoutedEventArgs e)
        {
            _menuActions.CheckMissingImages_Click(sender, e);
        }

        private void MoveWrongImages_Click(object sender, RoutedEventArgs e)
        {
            _menuActions.MoveWrongImages_Click(sender, e);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ProgramInfo selectedProgram = (ProgramInfo)MyComboBox.SelectedItem;
            // Do something with selectedProgram
        }

        public class ProgramInfo
        {
            public int Id { get; set; }
            public string ProgramName { get; set; }
            public string ProgramLocation { get; set; }
            public string Parameters { get; set; }

            public override string ToString()
            {
                return ProgramName; // Display the program name in the ComboBox
            }
        }

        private void LoadParameters()
        {
            string filePath = "parameters.txt";
            List<ProgramInfo> programInfos = new List<ProgramInfo>();
            ProgramInfo currentProgramInfo = null;

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ": " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (key == "Id")
                        {
                            currentProgramInfo = new ProgramInfo { Id = int.Parse(value) };

                        }
                        else if (key == "ProgramName")
                        {
                            currentProgramInfo.ProgramName = value;
                        }
                        else if (key == "ProgramLocation")
                        {
                            currentProgramInfo.ProgramLocation = value;
                        }
                        else if (key == "Parameters")
                        {
                            currentProgramInfo.Parameters = value;
                            programInfos.Add(currentProgramInfo); // Add to the list once we reach the last attribute
                        }
                    }
                }

                // Now populate the ComboBox
                MyComboBox.ItemsSource = programInfos;
            }
            else
            {
                MessageBox.Show("parameters.txt not found");
            }
        }

        private string DetermineImagePath(string fileNameWithoutExtension)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagesDirectory = Path.Combine(currentDirectory, "images");
            string imagePath = Path.Combine(imagesDirectory, fileNameWithoutExtension + ".png");
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(imagesDirectory, "default.png");
            }
            return imagePath;
        }

        private async void LoadZipFiles(string startLetter = null)
        {
            try
            {
                // Clear existing grids
                zipFileGrid.Children.Clear();
                // Do the heavy lifting in a background task
                List<string> allFiles = await Task.Run(() =>
                {
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagesDirectory = Path.Combine(currentDirectory, "images");
                    var fileExtensions = new[] { "*.zip", "*.7z", "*.iso", "*.chd", "*.cso" };
                    return fileExtensions.SelectMany(ext => Directory.GetFiles(currentDirectory, ext)).ToList();
                });

                // Once the background task is done, continue on the UI thread.
                if (!allFiles.Any())
                {
                    zipFileGrid.Children.Add(new TextBlock { Text = "Could not find any ROM", FontWeight = FontWeights.Bold });
                    return;
                }

                // Filter files based on the starting letter or number if provided
                if (!string.IsNullOrEmpty(startLetter))
                {
                    if (startLetter == "#")
                    {
                        allFiles = allFiles.Where(file => char.IsDigit(Path.GetFileName(file)[0])).ToList();
                    }
                    else
                    {
                        allFiles = allFiles.Where(file => Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }

                allFiles.Sort();

                foreach (var filePath in allFiles)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

                    //Call the function to determine the image path
                    string imagePath = DetermineImagePath(fileNameWithoutExtension);

                    var image = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePath)),
                        Height = 200,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var textBlock = new TextBlock
                    {
                        Text = fileNameWithoutExtension,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontWeight = FontWeights.Bold,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    textBlock.ToolTip = fileNameWithoutExtension; // Display the full filename on hover

                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 300,
                        Height = 250,
                        MaxHeight = 250 // Limits the maximum height
                    };


                    stackPanel.Children.Add(image);
                    stackPanel.Children.Add(textBlock);

                    var button = new Button
                    {
                        Content = stackPanel,
                        Width = 300,
                        Height = 250, // Default height
                        MaxHeight = 250, // Limits the maximum height
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(0)
                    };

                    button.Click += async (sender, e) =>
                    {
                        ProcessStartInfo psi = null; // Declare the variable here

                        try
                        {
                            if (MyComboBox.SelectedItem is ProgramInfo selectedProgram)
                            {
                                string programLocation = selectedProgram.ProgramLocation;
                                string parameters = selectedProgram.Parameters;
                                string filename = Path.GetFileName(filePath);  // Get the full filename including extension

                                //// Combine the parameters and filename without full path
                                //string arguments = $"{parameters} \"{filename}\"";

                                // Combine the parameters and filename with full path
                                string arguments = $"{parameters} \"{filePath}\"";

                                // Output the entire argument to console
                                Console.WriteLine("Arguments passed to the external program:");
                                Console.WriteLine(arguments);

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

                                // Output to console
                                Console.WriteLine("Standard Output:");
                                Console.WriteLine(output);
                                Console.WriteLine("Standard Error:");
                                Console.WriteLine(error);

                                if (process.ExitCode != 0) // Check if the process exited with an error code
                                {
                                    MessageBox.Show("The emulator could not open this file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    // External program did not start successfully, write to error log
                                    string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                                    string errorMessage = $"Error launching external program: Exit code {process.ExitCode}\n";
                                    errorMessage += $"Process Start Info:\nFileName: {psi.FileName}\nArguments: {psi.Arguments}\n";
                                    File.WriteAllText(errorLogPath, errorMessage);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please select an emulator", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            // An exception occurred while trying to start the process
                            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            // Write the exception details to the error log
                            string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                            string errorDetails = $"Exception Details:\n{ex}\n";
                            if (psi != null)
                            {
                                errorDetails += $"Process Start Info:\nFileName: {psi.FileName}\nArguments: {psi.Arguments}\n";
                            }
                            File.WriteAllText(errorLogPath, errorDetails);
                        }
                    };

                    zipFileGrid.Children.Add(button);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

    }
}