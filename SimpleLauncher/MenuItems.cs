using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    public class MenuActions
    {
        private Window _window;
        private WrapPanel _zipFileGrid;

        public MenuActions(Window window, WrapPanel zipFileGrid)
        {
            _window = window;
            _zipFileGrid = zipFileGrid;
        }

        public void CheckMissingImages_Click(object sender, RoutedEventArgs e)
        {
            CheckMissingImages();
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Simple Launcher.\nAn Open Source Emulator Launcher.\nVersion 1.3", "About");
        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            _window.Close();
        }
        private void CheckMissingImages()
        {
            try
            {
                // Path to the program directory and the images sub-directory
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(currentDirectory, "images");

                // Get all game files in the program directory
                var fileExtensions = new[] { "*.zip", "*.7z", "*.iso", "*.chd", "*.cso" };
                List<string> allFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(currentDirectory, ext))
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();

                // Create an empty list to hold missing image files
                List<string> missingImages = new List<string>();

                // Check if each corresponding image exists in the images directory
                foreach (var fileName in allFiles)
                {
                    string imagePath = Path.Combine(imagesDirectory, fileName + ".png");
                    if (!File.Exists(imagePath))
                    {
                        missingImages.Add(fileName);
                    }
                }

                // Write the missing image files to a text file
                if (missingImages.Any())
                {
                    string missingImagesPath = Path.Combine(currentDirectory, "missingimages.txt");
                    File.WriteAllLines(missingImagesPath, missingImages);
                    MessageBox.Show("Missing images found. Check missingimages.txt for details.");
                }
                else
                {
                    MessageBox.Show("No missing images found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        public void MoveWrongImages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(currentDirectory, "images");
                string wrongImagesDirectory = Path.Combine(currentDirectory, "wrongimages");

                // Step 1
                var validExtensions = new[] { "*.zip", "*.7z", "*.iso", "*.chd", "*.cso" };
                var validFileNames = validExtensions.SelectMany(ext => Directory.GetFiles(currentDirectory, ext))
                                                    .Select(Path.GetFileNameWithoutExtension)
                                                    .ToList();

                // Step 4
                validFileNames.Add("default");

                // Create the wrongimages directory if it doesn't exist
                if (!Directory.Exists(wrongImagesDirectory))
                {
                    Directory.CreateDirectory(wrongImagesDirectory);
                }

                // Step 2 & 3
                var imageFiles = Directory.GetFiles(imagesDirectory, "*.png");

                foreach (var imageFile in imageFiles)
                {
                    string imageName = Path.GetFileNameWithoutExtension(imageFile);

                    if (!validFileNames.Contains(imageName, StringComparer.OrdinalIgnoreCase))
                    {
                        // Step 5
                        string destinationPath = Path.Combine(wrongImagesDirectory, Path.GetFileName(imageFile));
                        File.Move(imageFile, destinationPath);
                    }
                }

                // Step 6
                MessageBox.Show("Wrong images were moved to the wrongimages folder.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        public void HideGames_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in _zipFileGrid.Children)
            {
                if (child is Button btn && btn.Content is StackPanel sp)
                {
                    var image = sp.Children.OfType<Image>().FirstOrDefault();
                    if (image != null && image.Source is BitmapImage bmp && bmp.UriSource.LocalPath.EndsWith("default.png"))
                    {
                        btn.Visibility = Visibility.Collapsed; // hide the button
                    }
                }
            }
        }

        public void ShowGames_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in _zipFileGrid.Children)
            {
                if (child is Button btn)
                {
                    btn.Visibility = Visibility.Visible; // Show the button
                }
            }
        }
    }

}
