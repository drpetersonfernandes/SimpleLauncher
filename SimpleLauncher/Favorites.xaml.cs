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
        private readonly AppSettings _settings;
        private readonly List<SystemConfig> _systemConfigs;
        private readonly List<MameConfig> _machines;

        public Favorites(AppSettings settings, List<SystemConfig> systemConfigs, List<MameConfig> machines)
        {
            InitializeComponent();
            _favoritesManager = new FavoritesManager();
            _settings = settings;
            _systemConfigs = systemConfigs;
            _machines = machines;
            LoadFavorites();
            Closing += EditLinks_Closing; // attach event handler
        }
        
        private void EditLinks_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
                return Path.Combine(baseDirectory, "images", "default.png");
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                _favoriteList.Remove(selectedFavorite);
                _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                // MessageBox.Show($"{selectedFavorite.FileName} has been removed from favorites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("System configuration not found for the selected file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
                if (emulatorConfig == null)
                {
                    MessageBox.Show("No emulator configuration found for the selected system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string fullPath = GetFullPath(Path.Combine(systemConfig.SystemFolder, fileName));

                var mockSystemComboBox = new ComboBox();
                var mockEmulatorComboBox = new ComboBox();

                mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
                mockSystemComboBox.SelectedItem = systemConfig.SystemName;

                mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
                mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

                await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs);
            }
            catch (Exception ex)
            {
                string formattedException = $"There was an error launching the game from Favorites.\n\nException Details: {ex.Message}\n\nFile Path: {fileName}\n\nSystem Name: {systemName}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                MessageBox.Show($"{formattedException}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                AddMenuItem(contextMenu, "Launch Selected Game", () => _ = LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName), "pack://application:,,,/images/launch.png");
                AddMenuItem(contextMenu, "Remove from Favorites", () => RemoveFromFavorites(selectedFavorite), "pack://application:,,,/images/brokenheart.png");
                AddMenuItem(contextMenu, "Open Video Link", () => OpenVideoLink(selectedFavorite.SystemName, selectedFavorite.FileName, selectedFavorite.MachineDescription), "pack://application:,,,/images/video.png");
                AddMenuItem(contextMenu, "Open Info Link", () => OpenInfoLink(selectedFavorite.SystemName, selectedFavorite.FileName, selectedFavorite.MachineDescription), "pack://application:,,,/images/info.png");
                AddMenuItem(contextMenu, "Cover", () => OpenCover(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/cover.png");
                AddMenuItem(contextMenu, "Title Snapshot", () => OpenTitleSnapshot(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/snapshot.png");
                AddMenuItem(contextMenu, "Gameplay Snapshot", () => OpenGameplaySnapshot(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/snapshot.png");
                AddMenuItem(contextMenu, "Cart", () => OpenCart(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/cart.png");
                AddMenuItem(contextMenu, "Video", () => PlayVideo(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/video.png");
                AddMenuItem(contextMenu, "Manual", () => OpenManual(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/manual.png");
                AddMenuItem(contextMenu, "Walkthrough", () => OpenWalkthrough(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/walkthrough.png");
                AddMenuItem(contextMenu, "Cabinet", () => OpenCabinet(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/cabinet.png");
                AddMenuItem(contextMenu, "Flyer", () => OpenFlyer(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/flyer.png");
                AddMenuItem(contextMenu, "PCB", () => OpenPcb(selectedFavorite.SystemName, selectedFavorite.FileName), "pack://application:,,,/images/pcb.png");

                contextMenu.IsOpen = true;
            }
        }

        private void AddMenuItem(ContextMenu contextMenu, string header, Action action, string iconPath = null)
        {
            var menuItem = new MenuItem
            {
                Header = header
            };

            if (!string.IsNullOrEmpty(iconPath))
            {
                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                menuItem.Icon = icon;
            }

            menuItem.Click += (_, _) => action();
            contextMenu.Items.Add(menuItem);
        }
        
        private void RemoveFromFavorites(Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
            // MessageBox.Show($"{selectedFavorite.FileName} has been removed from favorites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            catch (Exception exception)
            {
                MessageBox.Show($"There was a problem opening the Video Link.\n\nException details: {exception.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (Exception exception)
            {
                MessageBox.Show($"There was a problem opening the Info Link.\n\nException details: {exception.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCover(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                MessageBox.Show("System configuration not found for the selected file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var imageViewerWindow = new OpenImageFiles();
                imageViewerWindow.LoadImage(foundImagePath);
                imageViewerWindow.Show();
            }
            // If not found, try the global directory
            else if (TryFindImage(globalDirectory, out foundImagePath))
            {
                var imageViewerWindow = new OpenImageFiles();
                imageViewerWindow.LoadImage(foundImagePath);
                imageViewerWindow.Show();
            }
            else
            {
                MessageBox.Show("There is no cover associated with this file or button.", "Cover Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var imageViewerWindow = new OpenImageFiles();
                    imageViewerWindow.LoadImage(titleSnapshotPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no title snapshot associated with this file or button.", "Title Snapshot Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var imageViewerWindow = new OpenImageFiles();
                    imageViewerWindow.LoadImage(gameplaySnapshotPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no gameplay snapshot associated with this file or button.", "Gameplay Snapshot Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var imageViewerWindow = new OpenImageFiles();
                    imageViewerWindow.LoadImage(cartPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no cart associated with this file or button.", "Cart Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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

            MessageBox.Show("There is no video associated with this file or button.", "Video Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        MessageBox.Show($"Failed to open the manual: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            MessageBox.Show("There is no manual associated with this file or button.", "Manual Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        MessageBox.Show($"Failed to open the walkthrough: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            MessageBox.Show("There is no walkthrough associated with this file or button.", "Walkthrough Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var imageViewerWindow = new OpenImageFiles();
                    imageViewerWindow.LoadImage(cabinetPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no cabinet associated with this file or button.", "Cabinet Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var imageViewerWindow = new OpenImageFiles();
                    imageViewerWindow.LoadImage(flyerPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no flyer associated with this file or button.", "Flyer Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var imageViewerWindow = new OpenImageFiles();
                    imageViewerWindow.LoadImage(pcbPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no PCB associated with this file or button.", "PCB Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
