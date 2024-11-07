using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    internal class GameListViewFactory
    {
        private readonly ComboBox _emulatorComboBox;
        private readonly ComboBox _systemComboBox;
        private readonly List<SystemConfig> _systemConfigs;
        private readonly List<MameConfig> _machines;
        private readonly SettingsConfig _settings;
        private readonly FavoritesConfig _favoritesConfig;
        private readonly MainWindow _mainWindow;

        public GameListViewFactory(ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs, 
                                   List<MameConfig> machines, SettingsConfig settings, FavoritesConfig favoritesConfig, MainWindow mainWindow)
        {
            _emulatorComboBox = emulatorComboBox;
            _systemComboBox = systemComboBox;
            _systemConfigs = systemConfigs;
            _machines = machines;
            _settings = settings;
            _favoritesConfig = favoritesConfig;
            _mainWindow = mainWindow;
        }

        public class GameListViewItem
        {
            public string FileName { get; set; }
            public string MachineDescription { get; set; }
            public string FilePath { get; set; } // Store the full file path for easy access
            public ContextMenu ContextMenu { get; set; }

        }

        public Task<GameListViewItem> CreateGameListViewItemAsync(string filePath, string systemName, SystemConfig systemConfig)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string machineDescription = systemConfig.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

            // Create the GameListViewItem with file details
            var gameListViewItem = new GameListViewItem
            {
                FileName = fileNameWithoutExtension,
                MachineDescription = machineDescription,
                FilePath = filePath,
                ContextMenu = CreateContextMenu(filePath, systemName, systemConfig)
            };

            return Task.FromResult(gameListViewItem);
        }

        private string GetMachineDescription(string fileName)
        {
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            return machine?.Description ?? string.Empty;
        }

        private ContextMenu CreateContextMenu(string filePath, string systemName, SystemConfig systemConfig)
        {
            var contextMenu = new ContextMenu();

            var launchMenuItem = new MenuItem { Header = "Launch Game" };
            launchMenuItem.Click += async (_, _) => await LaunchGame(filePath, systemName);
            contextMenu.Items.Add(launchMenuItem);

            var addToFavoritesMenuItem = new MenuItem { Header = "Add to Favorites" };
            addToFavoritesMenuItem.Click += (_, _) => AddToFavorites(systemName, Path.GetFileName(filePath));
            contextMenu.Items.Add(addToFavoritesMenuItem);

            return contextMenu;
        }

        public async Task HandleDoubleClick(GameListViewItem selectedItem)
        {
            if (selectedItem == null) return;

            string selectedSystem = _systemComboBox.SelectedItem as string;
            var systemConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (systemConfig != null)
            {
                // Launch the game using the full file path stored in the selected item
                await LaunchGame(selectedItem.FilePath, selectedSystem);
            }
        }

        private async Task LaunchGame(string filePath, string systemName)
        {
            await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
        }

        private void AddToFavorites(string systemName, string fileNameWithExtension)
        {
            // Logic to add the game to favorites
        }
        
        public void HandleSelectionChanged(GameListViewItem selectedItem)
        {
            if (selectedItem != null)
            {
                string filePath = selectedItem.FilePath;  // Use the full file path here
                string selectedSystem = _systemComboBox.SelectedItem as string;
                var systemConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

                if (systemConfig != null)
                {
                    // Get the preview image path
                    string previewImagePath = GetPreviewImagePath(filePath, systemConfig);

                    // Update the preview image in MainWindow
                    _mainWindow.PreviewImage.Source = new BitmapImage(new Uri(previewImagePath, UriKind.RelativeOrAbsolute));
                }
            }
        }

        private string GetPreviewImagePath(string filePath, SystemConfig systemConfig)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // First, check in the SystemImageFolder if it's defined
            string imageFolder = !string.IsNullOrEmpty(systemConfig.SystemImageFolder)
                ? systemConfig.SystemImageFolder
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemConfig.SystemName);

            string[] extensions = [".png", ".jpg", ".jpeg"];

            // Look for the image file in the defined or default image folder
            foreach (var extension in extensions)
            {
                string imagePath = Path.Combine(imageFolder, $"{fileNameWithoutExtension}{extension}");
                if (File.Exists(imagePath))
                {
                    return imagePath;
                }
            }

            // If no specific image found, try the user-defined default image in SystemImageFolder
            string userDefinedDefaultImagePath = Path.Combine(imageFolder, "default.png");
            if (File.Exists(userDefinedDefaultImagePath))
            {
                return userDefinedDefaultImagePath;
            }

            // If user-defined default image isn't found, fallback to the global default image
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");
        }
    }
}
