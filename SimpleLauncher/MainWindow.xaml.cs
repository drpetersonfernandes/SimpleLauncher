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
        public MainWindow()
        {
            InitializeComponent();

            // Set the title dynamically
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderName = new DirectoryInfo(currentDirectory).Name.TrimEnd('\\');
            this.Title = $"{folderName} Launcher";

            LoadParameters();
            LoadZipFiles();
        }
                private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the application
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Simple Launcher\nPeterson's Software\n08/2023");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProgramInfo selectedProgram = (ProgramInfo)MyComboBox.SelectedItem;
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
                            currentProgramInfo = new ProgramInfo();
                            currentProgramInfo.Id = int.Parse(value);
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


        private async void LoadZipFiles()
        {
            try
            {
                // Do the heavy lifting in a background task
                List<string> allFiles = await Task.Run(() =>
                {
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagesDirectory = Path.Combine(currentDirectory, "images");
                    var fileExtensions = new[] { "*.zip", "*.7z", "*.iso" };
                    return fileExtensions.SelectMany(ext => Directory.GetFiles(currentDirectory, ext)).ToList();
                });

                // Once the background task is done, continue on the UI thread.
                if (!allFiles.Any())
                {
                    zipFileGrid.Children.Add(new TextBlock { Text = "Could not find any ROM or ISO", FontWeight = FontWeights.Bold });
                    return;
                }

                allFiles.Sort();

                foreach (var filePath in allFiles)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagesDirectory = Path.Combine(currentDirectory, "images");
                    string imagePath = Path.Combine(imagesDirectory, fileNameWithoutExtension + ".png");

                    if (!File.Exists(imagePath))
                    {
                        imagePath = Path.Combine(imagesDirectory, "default.png");
                    }

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
                        TextTrimming = TextTrimming.CharacterEllipsis // Add this line
                    };
                    textBlock.ToolTip = fileNameWithoutExtension; // Display the full filename on hover

                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 250,
                        MaxHeight = 250 // This limits the maximum height
                    };


                    stackPanel.Children.Add(image);
                    stackPanel.Children.Add(textBlock);

                    var button = new Button
                    {
                        Content = stackPanel,
                        Height = 250, // You have this line already, this sets the default height
                        MaxHeight = 250, // This limits the maximum height
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Top
                    };

                    button.Click += async (sender, e) =>
                    {
                        try
                        {
                            if (MyComboBox.SelectedItem is ProgramInfo selectedProgram)
                            {
                                string programLocation = selectedProgram.ProgramLocation;
                                string parameters = selectedProgram.Parameters;
                                string filename = Path.GetFileName(filePath);  // Get the full filename including extension

                                // Combine the parameters and filename
                                string arguments = $"{parameters} \"{filename}\"";


                                // Output the entire argument to console
                                Console.WriteLine("Arguments passed to the external program:");
                                Console.WriteLine(arguments);

                                // Create ProcessStartInfo
                                ProcessStartInfo psi = new ProcessStartInfo
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
                        }
                    };





                    /* Assign the Click event handler
                   button.Click += (sender, e) =>
                   {
                       try
                       {
                           string scriptFilePath = @".\scriptfile.ps1";  // Assuming the file is named scriptfile.ps1
                           string Filename = Path.GetFileName(filePath);  // Get the full filename including extension

                           if (File.Exists(scriptFilePath))
                           {
                               ProcessStartInfo psi = new ProcessStartInfo("PowerShell", $"-ExecutionPolicy Bypass -File {scriptFilePath} -var \"{Filename}\"");
                               // Attempt to execute the PowerShell script
                               Process.Start(psi);
                           }
                           else
                           {
                               MessageBox.Show("Script file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                           }
                       }
                       catch (Exception ex)
                       {
                           // An exception occurred while trying to start the process
                           MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                       }
                   };*/


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