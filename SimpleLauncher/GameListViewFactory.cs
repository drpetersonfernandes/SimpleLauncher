using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        
        private class GameListViewItem
        {
            public string FileName { get; set; }
            public string MachineDescription { get; set; }
            public string SystemFolder { get; set; }
        }

        public Task<ListViewItem> CreateGameListViewItemAsync(string filePath, string systemName, SystemConfig systemConfig)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string machineDescription = systemConfig.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

            var listViewItem = new ListViewItem
            {
                Content = new GameListViewItem
                {
                    FileName = fileNameWithoutExtension,
                    MachineDescription = machineDescription,
                    SystemFolder = systemConfig.SystemFolder
                },
                Tag = filePath
            };

            // Create context menu
            listViewItem.ContextMenu = CreateContextMenu(filePath, systemName, systemConfig);

            // Set double-click event to launch game
            listViewItem.MouseDoubleClick += async (_, _) => await LaunchGame(filePath, systemName);

            return Task.FromResult(listViewItem);
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

            // Add additional menu items as needed (e.g., add to favorites, open links)
            var addToFavoritesMenuItem = new MenuItem { Header = "Add to Favorites" };
            addToFavoritesMenuItem.Click += (_, _) => AddToFavorites(systemName, Path.GetFileName(filePath));
            contextMenu.Items.Add(addToFavoritesMenuItem);

            return contextMenu;
        }

        private async Task LaunchGame(string filePath, string systemName)
        {
            await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
        }

        private void AddToFavorites(string systemName, string fileNameWithExtension)
        {
            // Logic to add the game to favorites (similar to GameButtonFactory)
        }
    }
}
