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

        // Pagination properties
        private const int ItemsPerPage = 10;
        private int _currentPage = 1;
        private int _totalGames;
        private List<string> _gameFilePaths;

        // Property to store game file paths
        public List<string> GameFilePaths
        {
            get => _gameFilePaths;
            set
            {
                _gameFilePaths = value;
                _totalGames = _gameFilePaths.Count;
                _currentPage = 1; // Reset to first page whenever the list is set
            }
        }

        private string DetermineImagePath(string fileNameWithoutExtension, string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return Path.Combine(_baseDirectory, "images", DefaultImagePath); // Return the default image if no system is selected.

            string imagePath = Path.Combine(_baseDirectory, "images", systemName, $"{fileNameWithoutExtension}.png");

            if (File.Exists(imagePath))
                return imagePath;

            return Path.Combine(_baseDirectory, "images", DefaultImagePath); // Return the default image if the specific image doesn't exist.
        }

        public class LazyImage
        {
            private BitmapImage _bitmap;
            readonly private string _imagePath;

            public LazyImage(string imagePath)
            {
                _imagePath = imagePath;
            }

            public BitmapImage Image
            {
                get
                {
                    if (_bitmap == null)
                    {
                        _bitmap = new BitmapImage(new Uri(_imagePath));
                    }
                    return _bitmap;
                }
            }
        }

        // Assuming your ComboBoxes and Configs are in MainWindow, you can pass them as properties.
        public ComboBox EmulatorComboBox { get; set; }
        public ComboBox SystemComboBox { get; set; }
        public List<SystemConfig> SystemConfigs { get; set; }

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

            // Lazy loading of image
            LazyImage lazyImage = new LazyImage(imagePath);

            var image = new Image
            {
                Source = lazyImage.Image,
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

            // youtubeIcon
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
                string searchTerm = $"{fileNameWithoutExtension} {systemName}";
                string searchUrl = $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(searchTerm)}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };

            // infoIcon
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
                string searchUrl = $"https://www.igdb.com/search?type=1&q={Uri.EscapeDataString(fileNameWithoutExtension)}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
                e.Handled = true; // Stops the click event from propagating to the button's main click event
            };

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

            //// Add the button click event handler
            GameLaunchHandler gameLaunchHandler = new GameLaunchHandler();
            button.Click += async (sender, args) =>
            {
                await gameLaunchHandler.HandleButtonClick(filePath, EmulatorComboBox, SystemComboBox, SystemConfigs);
            };

            return button;
        }

        // Method to load buttons for the current page
        public List<Button> LoadCurrentPageButtons()
        {
            var buttons = new List<Button>();
            int start = (_currentPage - 1) * ItemsPerPage;
            int end = Math.Min(start + ItemsPerPage, _totalGames);

            for (int i = start; i < end; i++)
            {
                string filePath = GameFilePaths[i];
                string systemName = SystemComboBox.SelectedItem?.ToString();
                Button gameButton = CreateGameButton(filePath, systemName);
                buttons.Add(gameButton);
            }

            return buttons;
        }

        // Method to go to the next page
        public List<Button> NextPage()
        {
            if ((_currentPage * ItemsPerPage) < _totalGames)
            {
                _currentPage++;
                return LoadCurrentPageButtons();
            }
            return null;
        }

        // Method to go to the previous page
        public List<Button> PreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                return LoadCurrentPageButtons();
            }
            return null;
        }
    }
}