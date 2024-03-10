using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    internal class GameButtonFactory(
        ComboBox emulatorComboBox,
        ComboBox systemComboBox,
        List<SystemConfig> systemConfigs,
        List<MameConfig> machines,
        AppSettings settings)
    {
        private const string DefaultImagePath = "default.png";
        public int ImageHeight { get; set; } = settings.ThumbnailSize; // Initialize ImageHeight
        private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // List to hold machine data
        // Initialize _machines

        private ComboBox EmulatorComboBox { get; set; } = emulatorComboBox;
        private ComboBox SystemComboBox { get; set; } = systemComboBox;
        private List<SystemConfig> SystemConfigs { get; set; } = systemConfigs;

        public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemConfig systemConfig)
        {
            // Load Video Url and Info Url from settings (settings.xml)
            string videoUrl = settings.VideoUrl;
            string infoUrl = settings.InfoUrl;
            
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);
            
            string imagePath = DetermineImagePath(fileNameWithoutExtension, systemConfig.SystemName);
            bool isDefaultImage = imagePath.EndsWith(DefaultImagePath);

            var image = new Image
            {
                Height = ImageHeight,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            await LoadImageAsync(image, imagePath);

            var textBlock = new TextBlock
            {
                Text = fileNameWithoutExtension,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = fileNameWithoutExtension
            };

            if (systemConfig.SystemIsMAME)
            {
                var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
                if (machine != null)
                {
                    var descriptionTextBlock = new TextBlock
                    {
                        Text = machine.Description,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        FontWeight = FontWeights.Bold,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        ToolTip = machine.Description
                    };
                    textBlock.Inlines.Add(new LineBreak());
                    textBlock.Inlines.Add(descriptionTextBlock);
                }
            }

            var youtubeIcon = CreateYoutubeIcon(fileNameWithoutExtension, systemName, videoUrl);
            var infoIcon = CreateInfoIcon(fileNameWithoutExtension, systemName, infoUrl);

            var grid = new Grid
            {
                Width = ImageHeight + 50,
                Height = ImageHeight + 50
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = ImageHeight + 50,
                Height = ImageHeight + 50,
                MaxHeight = ImageHeight + 50
            };

            grid.Children.Add(stackPanel);
            grid.Children.Add(youtubeIcon);
            grid.Children.Add(infoIcon);

            var button = new Button
            {
                Content = grid,
                Width = ImageHeight + 50,
                Height = ImageHeight + 50,
                MaxHeight = ImageHeight + 50,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };

            button.PreviewMouseLeftButtonDown += (_, args) =>
            {
                if (args.OriginalSource is Image img && (img.Name == "youtubeIcon" || img.Name == "infoIcon"))
                {
                    args.Handled = true;
                }
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            if (isDefaultImage)
            {
                button.Tag = "DefaultImage";
            }

            button.Click += async (_, _) =>
            {
                PlayClick.PlayClickSound();
                await GameLauncher.HandleButtonClick(filePath, EmulatorComboBox, SystemComboBox, SystemConfigs);
            };

            return button;
        }

        private string DetermineImagePath(string fileNameWithoutExtension, string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return Path.Combine(_baseDirectory, "images", DefaultImagePath); // Return the default image if no system is selected.

            // Extensions to check
            string[] extensions = [".png", ".jpg", ".jpeg"];

            // Check each extension for a valid image file
            foreach (var ext in extensions)
            {
                string imagePath = Path.Combine(_baseDirectory, "images", systemName, $"{fileNameWithoutExtension}{ext}");
                if (File.Exists(imagePath))
                    return imagePath;
            }

            return Path.Combine(_baseDirectory, "images", DefaultImagePath); // Return the default image if no specific image exists.
        }

        private static async Task LoadImageAsync(Image imageControl, string imagePath)
        {
            ArgumentNullException.ThrowIfNull(imageControl);

            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException(@"Invalid image path.", nameof(imagePath));

            BitmapImage bitmapImage = null;

            await Task.Run(() =>
            {
                using var stream = File.OpenRead(imagePath);
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Important for multi-threaded access
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

        private Image CreateYoutubeIcon(string fileNameWithoutExtension, string systemName, string videoUrl)
        {
            var youtubeIcon = new Image
            {
                Name = "youtubeIcon",
                Source = new BitmapImage(new Uri("images/searchyoutube.png", UriKind.RelativeOrAbsolute)),
                Width = 22,
                Height = 22,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 5, 30, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Set Z-Index to ensure it's on top
            youtubeIcon.SetValue(Panel.ZIndexProperty, 1);

            youtubeIcon.PreviewMouseLeftButtonUp += (_, e) =>
            {
                PlayClick.PlayClickSound();
                string searchTerm = $"{fileNameWithoutExtension} {systemName}";
                string searchUrl = $"{videoUrl}{Uri.EscapeDataString(searchTerm)}";

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = searchUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception exception)
                {
                    MainWindow.HandleError(exception, "The URL provided for Video Link did not work.\nPlease update the Video Link in the Edit Links menu.");
                    throw;
                }

                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };

            return youtubeIcon;
        }

        private Image CreateInfoIcon(string fileNameWithoutExtension, string systemName, string infoUrl)
        {
            var infoIcon = new Image
            {
                Name = "infoIcon",
                Source = new BitmapImage(new Uri("images/info.png", UriKind.RelativeOrAbsolute)),
                Width = 22,
                Height = 22,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 5, 5, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Set Z-Index to ensure it's on top
            infoIcon.SetValue(Panel.ZIndexProperty, 1);

            infoIcon.PreviewMouseLeftButtonUp += (_, e) =>
            {
                PlayClick.PlayClickSound();
                string searchTerm = $"{fileNameWithoutExtension} {systemName}";
                string searchUrl = $"{infoUrl}{Uri.EscapeDataString(searchTerm)}";
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = searchUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception exception)
                {
                    MainWindow.HandleError(exception, "The URL provided for Info Link did not work.\nPlease update the Info Link in the Edit Links menu.");
                    throw;
                }
                

                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };

            return infoIcon;
        }

    }
}