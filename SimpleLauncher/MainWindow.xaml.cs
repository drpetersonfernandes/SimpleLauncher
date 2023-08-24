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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // MaxHeight = height of one cell * maximum number of rows
            zipFileGrid.MaxHeight = 500 * 4;

            // Set the title dynamically
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderName = new DirectoryInfo(currentDirectory).Name.TrimEnd('\\');
            this.Title = $"{folderName} Launcher";

            LoadZipFiles();
        }

        private void LoadZipFiles()
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(currentDirectory, "images");

                // Search for zip, 7z, and iso files
                var fileExtensions = new[] { "*.zip", "*.7z", "*.iso" };

                // LINQ query to get all files with the desired extensions
                List<string> allFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(currentDirectory, ext)).ToList();

                if (!allFiles.Any())
                {
                    zipFileGrid.Children.Add(new TextBlock { Text = "Could'n find any ROM or ISO", FontWeight = System.Windows.FontWeights.Bold });
                    return;
                }

                allFiles.Sort();

                foreach (var filePath in allFiles)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                    // Capitalize the first letter of each word
                    fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

                    string imagePath = Path.Combine(imagesDirectory, fileNameWithoutExtension + ".jpg");

                    if (!File.Exists(imagePath))
                    {
                        imagePath = Path.Combine(imagesDirectory, "default.jpg");
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

                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    stackPanel.Children.Add(image);
                    stackPanel.Children.Add(textBlock);

                    var button = new Button
                    {
                        Content = stackPanel,
                        Height = 400,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Top
                    };

                    // Assign the Click event handler
                    button.Click += (sender, e) =>
                    {
                        // Specify the batch file and the argument
                        string batchFilePath = @"C:\Path\To\Your\BatchFile.bat";
                        string Filename = Path.GetFileName(filePath);  // Get the full filename including extension

                        if (File.Exists(batchFilePath))
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(batchFilePath, Filename);

                            // Execute the batch file
                            Process.Start(psi);
                        }
                        else
                        {
                            MessageBox.Show("Batch file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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