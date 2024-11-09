using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    public partial class Favorites
    {
        private readonly FavoritesManager _favoritesManager;
        private ObservableCollection<Favorite> _favoriteList;
        private readonly SettingsConfig _settings;
        private readonly List<SystemConfig> _systemConfigs;
        private readonly List<MameConfig> _machines;
        private readonly MainWindow _mainWindow;

        public Favorites(SettingsConfig settings, List<SystemConfig> systemConfigs, List<MameConfig> machines, MainWindow mainWindow)
        {
            InitializeComponent();
 
            App.ApplyThemeToWindow(this);
            
            _favoritesManager = new FavoritesManager();
            _settings = settings;
            _systemConfigs = systemConfigs;
            _machines = machines;
            _mainWindow = mainWindow;
            LoadFavorites();
            
            // Attach event handler
            Closing += Favorites_Closing; 
        }
        
        private void Favorites_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prepare the process start info
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = processModule.FileName,
                    UseShellExecute = true
                };

                // Start the new application instance
                Process.Start(startInfo);

                // Shutdown the current application instance
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        private void LoadFavorites()
        {
            var favoritesConfig = _favoritesManager.LoadFavorites();
            _favoriteList =
            [
            ];

            foreach (var favorite in favoritesConfig.FavoriteList)
            {
                var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(Path.GetFileNameWithoutExtension(favorite.FileName), StringComparison.OrdinalIgnoreCase));
                var machineDescription = machine?.Description ?? string.Empty;

                var favoriteItem = new Favorite
                {
                    FileName = favorite.FileName,
                    SystemName = favorite.SystemName,
                    MachineDescription = machineDescription,
                    CoverImage = GetCoverImagePath(favorite.SystemName, favorite.FileName) // Set cover image path
                };

                _favoriteList.Add(favoriteItem);
            }
            FavoritesDataGrid.ItemsSource = _favoriteList;
        }

        private string GetCoverImagePath(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                return Path.Combine(baseDirectory, "images", "default.png");
            }

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Specific image path
            string systemImageFolder = systemConfig.SystemImageFolder ?? string.Empty;
            string systemSpecificDirectory = Path.Combine(baseDirectory, systemImageFolder);

            // Global image path
            string globalDirectory = Path.Combine(baseDirectory, "images", systemName);

            // Image extensions to look for
            string[] imageExtensions = [".png", ".jpg", ".jpeg"];

            // Search for the image file
            bool TryFindImage(string directory, out string foundPath)
            {
                foreach (var extension in imageExtensions)
                {
                    string imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                    if (File.Exists(imagePath))
                    {
                        foundPath = imagePath;
                        return true;
                    }
                }
                foundPath = null;
                return false;
            }

            // First try to find the image in the specific directory
            if (TryFindImage(systemSpecificDirectory, out string foundImagePath))
            {
                return foundImagePath;
            }
            // If not found, try the global directory
            else if (TryFindImage(globalDirectory, out foundImagePath))
            {
                return foundImagePath;
            }
            else
            {
                // If not found, use default image
                return Path.Combine(baseDirectory, "images", "default.png");
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                _favoriteList.Remove(selectedFavorite);
                _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });

                PreviewImage.Source = null;
            }
            else
            {
                MessageBox.Show("Please select a favorite to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
      
        private async void LaunchGame_Click(object sender, RoutedEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
            }
            else
            {
                MessageBox.Show("Please select a game to launch.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task LaunchGameFromFavorite(string fileName, string systemName)
        {
            try
            {
                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
                if (systemConfig == null)
                {

                    string formattedException = $"There was an error in the Favorites window.\n\nNo system configuration found for the selected favorite.";
                    Exception ex = new(formattedException);
                    await LogErrors.LogErrorAsync(ex, formattedException);
                   
                    MessageBox.Show("There was an error loading the system configuration for this favorite.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
                if (emulatorConfig == null)
                {
                    string formattedException = $"There was an error in the Favorites window.\n\nNo emulator configuration found for the selected favorite.";
                    Exception ex = new(formattedException);
                    await LogErrors.LogErrorAsync(ex, formattedException);
                    
                    MessageBox.Show("No emulator configuration found for the selected favorite.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string fullPath = GetFullPath(Path.Combine(systemConfig.SystemFolder, fileName));
                
                // Check if the file exists
                if (!File.Exists(fullPath))
                {
                    string formattedException = $"There was an error in the Favorites window.\n\nThe favorite file does not exist.";
                    Exception exception = new(formattedException);
                    await LogErrors.LogErrorAsync(exception, formattedException);
                    
                    // Remove the favorite from the list since the file no longer exists
                    var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
                    if (favoriteToRemove != null)
                    {
                        _favoriteList.Remove(favoriteToRemove);
                        _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                    }
                    
                    MessageBox.Show("The game file does not exist!\n\nThe favorite has been removed from the list.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var mockSystemComboBox = new ComboBox();
                var mockEmulatorComboBox = new ComboBox();

                mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
                mockSystemComboBox.SelectedItem = systemConfig.SystemName;

                mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
                mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

                await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
            }
            catch (Exception ex)
            {
                string formattedException = $"There was an error launching the game from Favorites.\n\nFile Path: {fileName}\nSystem Name: {systemName}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                MessageBox.Show($"There was an error launching the game from Favorites.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string GetFullPath(string path)
        {
            if (path.StartsWith(@".\"))
            {
                path = path.Substring(2);
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        private void FavoritesDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                var contextMenu = new ContextMenu();

                // "Launch Selected Game" MenuItem
                var launchIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var launchMenuItem = new MenuItem
                {
                    Header = "Launch Selected Game",
                    Icon = launchIcon
                };
                launchMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    _ = LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
                };

                // "Remove from Favorites" MenuItem
                var removeIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var removeMenuItem = new MenuItem
                {
                    Header = "Remove from Favorites",
                    Icon = removeIcon
                };
                removeMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RemoveFromFavorites(selectedFavorite);
                };

                // "Open Video Link" MenuItem
                var videoLinkIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var videoLinkMenuItem = new MenuItem
                {
                    Header = "Open Video Link",
                    Icon = videoLinkIcon
                };
                videoLinkMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenVideoLink(selectedFavorite.SystemName, selectedFavorite.FileName, selectedFavorite.MachineDescription);
                };

                // "Open Info Link" MenuItem
                var infoLinkIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var infoLinkMenuItem = new MenuItem
                {
                    Header = "Open Info Link",
                    Icon = infoLinkIcon
                };
                infoLinkMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenInfoLink(selectedFavorite.SystemName, selectedFavorite.FileName, selectedFavorite.MachineDescription);
                };

                // "Open ROM History" MenuItem
                var openHistoryIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var openHistoryMenuItem = new MenuItem
                {
                    Header = "Open ROM History",
                    Icon = openHistoryIcon
                };
                openHistoryMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);
                    var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
                    OpenHistoryWindow(selectedFavorite.SystemName, fileNameWithoutExtension, systemConfig);
                };

                // "Cover" MenuItem
                var coverIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var coverMenuItem = new MenuItem
                {
                    Header = "Cover",
                    Icon = coverIcon
                };
                coverMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenCover(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Title Snapshot" MenuItem
                var titleSnapshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var titleSnapshotMenuItem = new MenuItem
                {
                    Header = "Title Snapshot",
                    Icon = titleSnapshotIcon
                };
                titleSnapshotMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenTitleSnapshot(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Gameplay Snapshot" MenuItem
                var gameplaySnapshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var gameplaySnapshotMenuItem = new MenuItem
                {
                    Header = "Gameplay Snapshot",
                    Icon = gameplaySnapshotIcon
                };
                gameplaySnapshotMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenGameplaySnapshot(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Cart" MenuItem
                var cartIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var cartMenuItem = new MenuItem
                {
                    Header = "Cart",
                    Icon = cartIcon
                };
                cartMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenCart(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Video" MenuItem
                var videoIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var videoMenuItem = new MenuItem
                {
                    Header = "Video",
                    Icon = videoIcon
                };
                videoMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    PlayVideo(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Manual" MenuItem
                var manualIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var manualMenuItem = new MenuItem
                {
                    Header = "Manual",
                    Icon = manualIcon
                };
                manualMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenManual(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Walkthrough" MenuItem
                var walkthroughIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var walkthroughMenuItem = new MenuItem
                {
                    Header = "Walkthrough",
                    Icon = walkthroughIcon
                };
                walkthroughMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenWalkthrough(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Cabinet" MenuItem
                var cabinetIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var cabinetMenuItem = new MenuItem
                {
                    Header = "Cabinet",
                    Icon = cabinetIcon
                };
                cabinetMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenCabinet(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Flyer" MenuItem
                var flyerIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var flyerMenuItem = new MenuItem
                {
                    Header = "Flyer",
                    Icon = flyerIcon
                };
                flyerMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenFlyer(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "PCB" MenuItem
                var pcbIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var pcbMenuItem = new MenuItem
                {
                    Header = "PCB",
                    Icon = pcbIcon
                };
                pcbMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenPcb(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                contextMenu.Items.Add(launchMenuItem);
                contextMenu.Items.Add(removeMenuItem);
                contextMenu.Items.Add(videoLinkMenuItem);
                contextMenu.Items.Add(infoLinkMenuItem);
                contextMenu.Items.Add(openHistoryMenuItem);
                contextMenu.Items.Add(coverMenuItem);
                contextMenu.Items.Add(titleSnapshotMenuItem);
                contextMenu.Items.Add(gameplaySnapshotMenuItem);
                contextMenu.Items.Add(cartMenuItem);
                contextMenu.Items.Add(videoMenuItem);
                contextMenu.Items.Add(manualMenuItem);
                contextMenu.Items.Add(walkthroughMenuItem);
                contextMenu.Items.Add(cabinetMenuItem);
                contextMenu.Items.Add(flyerMenuItem);
                contextMenu.Items.Add(pcbMenuItem);

                contextMenu.IsOpen = true;
            }
        }
        
        private void RemoveFromFavorites(Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
        }

        private void OpenVideoLink(string systemName, string fileName, string machineDescription = null)
        {
            var searchTerm =
                // Check if machineDescription is provided and not empty
                !string.IsNullOrEmpty(machineDescription) ? $"{machineDescription} {systemName}" : $"{Path.GetFileNameWithoutExtension(fileName)} {systemName}";

            string searchUrl = $"{_settings.VideoUrl}{Uri.EscapeDataString(searchTerm)}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                
                string formattedException = $"There was a problem opening the Video Link.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"There was a problem opening the Video Link.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenInfoLink(string systemName, string fileName, string machineDescription = null)
        {
            var searchTerm =
                // Check if machineDescription is provided and not empty
                !string.IsNullOrEmpty(machineDescription) ? $"{machineDescription} {systemName}" : $"{Path.GetFileNameWithoutExtension(fileName)} {systemName}";

            string searchUrl = $"{_settings.InfoUrl}{Uri.EscapeDataString(searchTerm)}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                string formattedException = $"There was a problem opening the Info Link.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"There was a problem opening the Info Link.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OpenHistoryWindow(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig)
        {
            string romName = fileNameWithoutExtension.ToLowerInvariant();
           
            // Attempt to find a matching machine description
            string searchTerm = fileNameWithoutExtension;
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
            {
                searchTerm = machine.Description;
            }

            try
            {
                var historyWindow = new RomHistoryWindow(romName, systemName, searchTerm, systemConfig);
                historyWindow.Show();

            }
            catch (Exception ex)
            {
                string contextMessage = $"There was a problem opening the History window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"There was a problem opening the History window.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCover(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                string formattedException = $"There was a problem getting the system configuration for the selected favorite in the Favorites window.";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                MessageBox.Show("There was an error trying to open the cover image.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Specific image path
            string systemImageFolder = systemConfig.SystemImageFolder ?? string.Empty;
            string systemSpecificDirectory = Path.Combine(baseDirectory, systemImageFolder);

            // Global image path
            string globalDirectory = Path.Combine(baseDirectory, "images", systemName);

            // Image extensions to look for
            string[] imageExtensions = [".png", ".jpg", ".jpeg"];

            // Search for the image file
            bool TryFindImage(string directory, out string foundPath)
            {
                foreach (var extension in imageExtensions)
                {
                    string imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                    if (File.Exists(imagePath))
                    {
                        foundPath = imagePath;
                        return true;
                    }
                }
                foundPath = null;
                return false;
            }

            // First try to find the image in the specific directory
            if (TryFindImage(systemSpecificDirectory, out string foundImagePath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(foundImagePath);
                imageViewerWindow.Show();
            }
            // If not found, try the global directory
            else if (TryFindImage(globalDirectory, out foundImagePath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(foundImagePath);
                imageViewerWindow.Show();
            }
            else
            {
                MessageBox.Show("There is no cover associated with this favorite.", "Cover not found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenTitleSnapshot(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
            string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in titleSnapshotExtensions)
            {
                string titleSnapshotPath = Path.Combine(titleSnapshotDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(titleSnapshotPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(titleSnapshotPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no title snapshot associated with this favorite.", "Title Snapshot not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenGameplaySnapshot(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
            string[] gameplaySnapshotExtensions = [".png", ".jpg", ".jpeg"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in gameplaySnapshotExtensions)
            {
                string gameplaySnapshotPath = Path.Combine(gameplaySnapshotDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(gameplaySnapshotPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(gameplaySnapshotPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no gameplay snapshot associated with this favorite.", "Gameplay Snapshot not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCart(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
            string[] cartExtensions = [".png", ".jpg", ".jpeg"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in cartExtensions)
            {
                string cartPath = Path.Combine(cartDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(cartPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(cartPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no cart associated with this favorite.", "Cart not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PlayVideo(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string videoDirectory = Path.Combine(baseDirectory, "videos", systemName);
            string[] videoExtensions = [".mp4", ".avi", ".mkv"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in videoExtensions)
            {
                string videoPath = Path.Combine(videoDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(videoPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = videoPath,
                        UseShellExecute = true
                    });
                    return;
                }
            }

            MessageBox.Show("There is no video file associated with this favorite.", "Video not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenManual(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string manualDirectory = Path.Combine(baseDirectory, "manuals", systemName);
            string[] manualExtensions = [".pdf"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in manualExtensions)
            {
                string manualPath = Path.Combine(manualDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(manualPath))
                {
                    try
                    {
                        // Use the default PDF viewer to open the file
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = manualPath,
                            UseShellExecute = true
                        });
                        return;
                    }
                    catch (Exception ex)
                    {
                        string formattedException = $"Failed to open the manual in the Favorites window\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        Exception exception = new(formattedException);
                        Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                        
                        MessageBox.Show($"Failed to open the manual for this favorite.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            MessageBox.Show("There is no manual associated with this favorite.", "Manual not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenWalkthrough(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walkthroughDirectory = Path.Combine(baseDirectory, "walkthrough", systemName);
            string[] walkthroughExtensions = [".pdf"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in walkthroughExtensions)
            {
                string walkthroughPath = Path.Combine(walkthroughDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(walkthroughPath))
                {
                    try
                    {
                        // Use the default PDF viewer to open the file
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = walkthroughPath,
                            UseShellExecute = true
                        });
                        return;
                    }
                    catch (Exception ex)
                    {
                        string formattedException = $"Failed to open the walkthrough file in the Favorites window\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                        
                        MessageBox.Show($"Failed to open the walkthrough file.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            MessageBox.Show("There is no walkthrough file associated with this favorite.", "Walkthrough not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCabinet(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
            string[] cabinetExtensions = [".png", ".jpg", ".jpeg"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in cabinetExtensions)
            {
                string cabinetPath = Path.Combine(cabinetDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(cabinetPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(cabinetPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no cabinet file associated with this favorite.", "Cabinet not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenFlyer(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
            string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in flyerExtensions)
            {
                string flyerPath = Path.Combine(flyerDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(flyerPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(flyerPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no flyer file associated with this favorite.", "Flyer not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenPcb(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
            string[] pcbExtensions = [".png", ".jpg", ".jpeg"];

            // Remove the original file extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            foreach (var extension in pcbExtensions)
            {
                string pcbPath = Path.Combine(pcbDirectory, fileNameWithoutExtension + extension);
                if (File.Exists(pcbPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(pcbPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no PCB file associated with this favorite.", "PCB not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private async void FavoritesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
                {
                    await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"There was an error trying to launch a favorite using the MouseDoubleClick method.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                MessageBox.Show($"There was an error trying to launch this favorite.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void FavoritesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                var imagePath = selectedFavorite.CoverImage;
                PreviewImage.Source = File.Exists(imagePath) ? new BitmapImage(new Uri(imagePath, UriKind.Absolute)) :
                    // Set a default image if the selected image doesn't exist
                    new BitmapImage(new Uri("pack://application:,,,/images/default.png"));
            }
        }
        
        private void FavoritesDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveSelectedFavorite();
            }
        }
        
        private void RemoveSelectedFavorite()
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                _favoriteList.Remove(selectedFavorite);
                _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
            }
            else
            {
                MessageBox.Show("Please select a favorite to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}