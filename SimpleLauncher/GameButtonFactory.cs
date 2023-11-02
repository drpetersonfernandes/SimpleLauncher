using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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

        public async Task<Button> CreateGameButtonAsync(string filePath, string systemName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

            // Determine the image path based on the filename
            string imagePath = DetermineImagePath(fileNameWithoutExtension, systemName);

            // Check if default image is used and add a tag to the button
            bool isDefaultImage = imagePath.EndsWith(DefaultImagePath);

            var image = new Image
            {
                Height = ImageHeight,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Load the image asynchronously
            await LoadImageAsync(image, imagePath);

            var textBlock = new TextBlock
            {
                Text = fileNameWithoutExtension,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = fileNameWithoutExtension // Display the full filename on hover
            };

            var youtubeIcon = CreateYoutubeIcon(fileNameWithoutExtension, systemName);
            var infoIcon = CreateInfoIcon(fileNameWithoutExtension);

            var grid = new Grid
            {
                Width = ButtonWidth,
                Height = ButtonHeight
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

            // Add the main content (StackPanel) and the YouTube icon to the Grid
            grid.Children.Add(stackPanel);
            grid.Children.Add(youtubeIcon);
            grid.Children.Add(infoIcon);

            var button = new Button
            {
                Content = grid,
                Width = ButtonWidth,
                Height = ButtonHeight,
                MaxHeight = ButtonHeight,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };

            button.PreviewMouseLeftButtonDown += (sender, args) =>
            {
                if (args.OriginalSource is Image img &&
                   (img.Name == "youtubeIcon" || img.Name == "infoIcon"))
                {
                    // If the event source is our youtubeIcon or infoIcon, set the event as handled
                    args.Handled = true;
                }
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            if (isDefaultImage)
            {
                button.Tag = "DefaultImage";
            }

            //Button click event
            GameLaunchHandler gameLaunchHandler = new GameLaunchHandler();
            button.Click += async (sender, args) =>
            {
                PlayClickSound();
                await gameLaunchHandler.HandleButtonClick(filePath, EmulatorComboBox, SystemComboBox, SystemConfigs);
            };

            return button;
        }

        private void PlayClickSound()
        {
            try
            {
                string soundPath = Path.Combine(_baseDirectory, "audio", "click.mp3");
                MediaPlayer mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
                mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                // Handle exceptions or log errors
                Debug.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        //private static async Task LoadImageAsync(Image image, string imagePath)
        //{
        //    var bitmapImage = new BitmapImage();
        //    bitmapImage.BeginInit();
        //    bitmapImage.UriSource = new Uri(imagePath);
        //    bitmapImage.EndInit();

        //    // Wait for the image to load
        //    await Task.Run(() => bitmapImage.Freeze());

        //    image.Source = bitmapImage;
        //}

        private async Task LoadImageAsync(Image imageControl, string imagePath)
        {
            if (imageControl == null)
                throw new ArgumentNullException(nameof(imageControl));

            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("Invalid image path.", nameof(imagePath));

            BitmapImage bitmapImage = null;

            await Task.Run(() =>
            {
                using (var stream = File.OpenRead(imagePath))
                {
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Important for multi-threaded access
                }
            });

            // Update the UI thread with the loaded image
            if (bitmapImage != null)
            {
                imageControl.Dispatcher.Invoke(() =>
                {
                    imageControl.Source = bitmapImage;
                });
            }
        }

        private Image CreateYoutubeIcon(string fileNameWithoutExtension, string systemName)
        {
            var youtubeIcon = new Image
            {
                Name = "youtubeIcon",
                Source = new BitmapImage(new Uri("images/searchyoutube.png", UriKind.RelativeOrAbsolute)),
                Width = 22,
                Height = 22,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 5, 30, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Set Z-Index to ensure it's on top
            youtubeIcon.SetValue(Grid.ZIndexProperty, 1);

            youtubeIcon.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                PlayClickSound();
                string searchTerm = $"{fileNameWithoutExtension} {systemName}";
                string searchUrl = $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(searchTerm)}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };

            return youtubeIcon;
        }

        private Image CreateInfoIcon(string fileNameWithoutExtension)
        {
            var infoIcon = new Image
            {
                Name = "infoIcon",
                Source = new BitmapImage(new Uri("images/info.png", UriKind.RelativeOrAbsolute)),
                Width = 22,
                Height = 22,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 5, 5, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Set Z-Index to ensure it's on top
            infoIcon.SetValue(Grid.ZIndexProperty, 1);

            infoIcon.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                PlayClickSound();
                string searchUrl = $"https://www.igdb.com/search?type=1&q={Uri.EscapeDataString(fileNameWithoutExtension)}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };

            return infoIcon;
        }

    }
}