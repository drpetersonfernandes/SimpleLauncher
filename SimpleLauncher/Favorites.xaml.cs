using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleLauncher
{
    public partial class Favorites
    {
        private readonly FavoritesManager _favoritesManager;
        private ObservableCollection<Favorite> _favoriteList;
        private readonly AppSettings _settings;
        private readonly List<SystemConfig> _systemConfigs;

        public Favorites(AppSettings settings, List<SystemConfig> systemConfigs)
        {
            InitializeComponent();
            _favoritesManager = new FavoritesManager();
            _settings = settings;
            _systemConfigs = systemConfigs;
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            var favoritesConfig = _favoritesManager.LoadFavorites();
            _favoriteList = favoritesConfig.FavoriteList;
            FavoritesDataGrid.ItemsSource = _favoriteList;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                _favoriteList.Remove(selectedFavorite);
                _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                MessageBox.Show($"{selectedFavorite.FileName} has been removed from favorites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a favorite to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FavoritesDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                var contextMenu = new ContextMenu();

                AddMenuItem(contextMenu, "Remove from Favorites", () => RemoveFromFavorites(selectedFavorite));
                AddMenuItem(contextMenu, "Open Video Link", () => OpenVideoLink(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Open Info Link", () => OpenInfoLink(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Cover", () => OpenCover(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Title Snapshot", () => OpenTitleSnapshot(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Gameplay Snapshot", () => OpenGameplaySnapshot(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Cart", () => OpenCart(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Video", () => PlayVideo(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Manual", () => OpenManual(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Walkthrough", () => OpenWalkthrough(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Cabinet", () => OpenCabinet(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "Flyer", () => OpenFlyer(selectedFavorite.SystemName, selectedFavorite.FileName));
                AddMenuItem(contextMenu, "PCB", () => OpenPcb(selectedFavorite.SystemName, selectedFavorite.FileName));

                contextMenu.IsOpen = true;
            }
        }

        private void AddMenuItem(ContextMenu contextMenu, string header, Action action)
        {
            var menuItem = new MenuItem
            {
                Header = header
            };
            menuItem.Click += (_, _) => action();
            contextMenu.Items.Add(menuItem);
        }

        private void RemoveFromFavorites(Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
            MessageBox.Show($"{selectedFavorite.FileName} has been removed from favorites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenVideoLink(string systemName, string fileName)
        {
            string searchTerm = $"{fileName} {systemName}";
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

        private void OpenInfoLink(string systemName, string fileName)
        {
            string searchTerm = $"{fileName} {systemName}";
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
    }
}
