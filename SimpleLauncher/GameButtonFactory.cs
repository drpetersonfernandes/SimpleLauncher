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

        // List to hold MAME descriptions from mame.xml
        private ComboBox EmulatorComboBox { get; set; } = emulatorComboBox;
        private ComboBox SystemComboBox { get; set; } = systemComboBox;
        private List<SystemConfig> SystemConfigs { get; set; } = systemConfigs;

        public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemConfig systemConfig)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);
            
            string imagePath = DetermineImagePath(fileNameWithoutExtension, systemConfig.SystemName, systemConfig);
            bool isDefaultImage = imagePath.EndsWith(DefaultImagePath);
            
            // Default search term for Video link and Info link
            string searchTerm = fileNameWithoutExtension;

            var textBlock = new TextBlock
            {
                Text = fileNameWithoutExtension,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = fileNameWithoutExtension
            };

            if (systemConfig.SystemIsMame)
            {
                var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
                if (machine != null)
                {
                    // Check if the machine's description is not null or empty; otherwise, keep using fileNameWithoutExtension
                    searchTerm = !string.IsNullOrWhiteSpace(machine.Description) ? machine.Description : fileNameWithoutExtension;

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
            var youtubeIcon = CreateYoutubeIcon(searchTerm, systemName, settings.VideoUrl);
            var infoIcon = CreateInfoIcon(searchTerm, systemName, settings.InfoUrl);

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
            
            var image = new Image
            {
                Height = ImageHeight,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            await LoadImageAsync(image, button, imagePath, DefaultImagePath);
            
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

        private string DetermineImagePath(string fileNameWithoutExtension, string systemName, SystemConfig systemConfig)
        {
            // Check if systemConfig or its SystemImageFolder is null or empty
            string baseImageDirectory = string.IsNullOrEmpty(systemConfig?.SystemImageFolder)
                ? Path.Combine(_baseDirectory, "images", systemName)
                : Path.Combine(_baseDirectory, systemConfig.SystemImageFolder);

            // Extensions to check
            string[] extensions = new string[] {".png", ".jpg", ".jpeg"};

            // Check each extension for a valid image file
            foreach (var ext in extensions)
            {
                string imagePath = Path.Combine(baseImageDirectory, $"{fileNameWithoutExtension}{ext}");
                if (File.Exists(imagePath))
                    return imagePath;
            }

            // Try to find default.png in the SystemImageFolder if specified, otherwise use the global default
            string defaultImagePath = Path.Combine(baseImageDirectory, "default.png");
            if (File.Exists(defaultImagePath))
            {
                return defaultImagePath;
            }

            // Fall back to the global default image path if no specific or system default image exists
            return Path.Combine(_baseDirectory, "images", DefaultImagePath);
        }

        private static async Task LoadImageAsync(Image imageControl, Button button, string imagePath, string defaultImagePath)
        {
            ArgumentNullException.ThrowIfNull(imageControl);

            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException(@"Invalid image path.", nameof(imagePath));
            try
            {
                var bitmapImage = await Task.Run(() =>
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    // Ensure the stream stays open until the BitmapImage is loaded
                    bi.StreamSource = File.OpenRead(imagePath);
                    bi.EndInit();
                    // Important for multi-threaded access
                    bi.Freeze();
                    return bi;
                });

                // Assign the loaded image to the image control on the UI thread
                imageControl.Source = bitmapImage;
            }
            catch (Exception)
            {
                // If an exception occurs (e.g., the image is corrupt), load a default image
                // This uses the dispatcher to ensure UI elements are accessed on the UI thread
                imageControl.Dispatcher.Invoke(() => LoadFallbackImage(imageControl, button, defaultImagePath));
                MessageBox.Show($"Unable to load image: {Path.GetFileName(imagePath)}.\n\nThis image is corrupted!\n\nA default image will be displayed instead.", "Image Loading Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void LoadFallbackImage(Image imageControl, Button button, string defaultImagePath)
        {
            string fallbackImagePath = defaultImagePath;

            // If the specific default image doesn't exist, try the global default image
            if (!File.Exists(fallbackImagePath))
            {
                fallbackImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", DefaultImagePath);
            }

            if (File.Exists(fallbackImagePath))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(fallbackImagePath, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Important for multi-threaded access
                imageControl.Source = bitmapImage; // Assign the fallback image
                button.Tag = "DefaultImage"; // Tagging the button to indicate a default image is used
            }
            else
            {
                // If even the global default image is not found, handle accordingly
                MessageBox.Show("No valid default image found.\n\nPlease reinstall the Simple Launcher.", "Image Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Image CreateYoutubeIcon(string searchTerm, string systemName, string videoUrl)
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
                string searchTerm2 = $"{searchTerm} {systemName}";
                string searchUrl = $"{videoUrl}{Uri.EscapeDataString(searchTerm2)}";

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
                    string contextMessage = $"There was a problem open up the Video Link.\n\nException details: {exception}";
                    Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                    MessageBox.Show($"There was a problem open up the Video Link.\n\nException details: {exception.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                    throw;
                }
                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };
            return youtubeIcon;
        }

        private Image CreateInfoIcon(string searchTerm, string systemName, string infoUrl)
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
                string searchTerm2 = $"{searchTerm} {systemName}";
                string searchUrl = $"{infoUrl}{Uri.EscapeDataString(searchTerm2)}";
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
                    string contextMessage = $"There was a problem open up the Info Link.\n\nException details: {exception}";
                    Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                    MessageBox.Show($"There was a problem open up the Info Link.\n\nException details: {exception.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                    throw;
                }
                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };
            return infoIcon;
        }

    }
}