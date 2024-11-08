using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly FavoritesManager _favoritesManager;

        public GameListViewFactory(ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs, 
                                   List<MameConfig> machines, SettingsConfig settings, FavoritesConfig favoritesConfig, MainWindow mainWindow)
        {
            _emulatorComboBox = emulatorComboBox;
            _systemComboBox = systemComboBox;
            _systemConfigs = systemConfigs;
            _machines = machines;
            _settings = settings;
            _favoritesManager = new FavoritesManager();
            _favoritesConfig = favoritesConfig;
            _mainWindow = mainWindow;
        }

        public class GameListViewItem
        {
            public string FileName { get; set; }
            public string MachineDescription { get; set; }
            public string FilePath { get; set; }
            public ContextMenu ContextMenu { get; set; }
            public bool IsFavorite { get; set; } // New property
        }

        public Task<GameListViewItem> CreateGameListViewItemAsync(string filePath, string systemName, SystemConfig systemConfig)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string machineDescription = systemConfig.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

            // Check if this file is a favorite
            bool isFavorite = _favoritesConfig.FavoriteList
                .Any(f => f.FileName.Equals(Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase) &&
                          f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            
            // Create the GameListViewItem with file details
            var gameListViewItem = new GameListViewItem
            {
                FileName = fileNameWithoutExtension,
                MachineDescription = machineDescription,
                FilePath = filePath,
                ContextMenu = CreateContextMenu(filePath, systemName, systemConfig),
                IsFavorite = isFavorite
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
            string fileNameWithExtension = Path.GetFileName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            
            // Check if the game is already in favorites
            bool isFavorite = _favoritesConfig.FavoriteList
                .Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) && 
                          f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            
            var contextMenu = new ContextMenu();

            // Launch Game Context Menu
            var launchMenuItemIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
                Width = 16,
                Height = 16
            };
            var launchMenuItem = new MenuItem
            {
                Header = "Launch Game",
                Icon = launchMenuItemIcon
            };
            launchMenuItem.Click += async (_, _) =>
            {
                PlayClick.PlayClickSound();
                await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
            };

            // Add To Favorites Context Menu
            var addToFavoritesIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
                Width = 16,
                Height = 16
            };
            var addToFavorites = new MenuItem
            {
                Header = "Add To Favorites",
                Icon = addToFavoritesIcon
            };
            addToFavorites.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                AddToFavorites(systemName, fileNameWithExtension);
            };
            
            // Remove From Favorites Context Menu
            var removeFromFavoritesIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png")),
                Width = 16,
                Height = 16
            };
            var removeFromFavorites = new MenuItem
            {
                Header = "Remove From Favorites",
                Icon = removeFromFavoritesIcon
            };
            removeFromFavorites.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                RemoveFromFavorites(systemName, fileNameWithExtension);
            };

            // Open Video Link Context Menu
            var openVideoLinkIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
                Width = 16,
                Height = 16
            };
            var openVideoLink = new MenuItem
            {
                Header = "Open Video Link",
                Icon = openVideoLinkIcon
            };
            openVideoLink.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenVideoLink(systemName, fileNameWithoutExtension);
            };

            // Open Info Link Context Menu
            var openInfoLinkIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
                Width = 16,
                Height = 16
            };
            var openInfoLink = new MenuItem
            {
                Header = "Open Info Link",
                Icon = openInfoLinkIcon
            };
            openInfoLink.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenInfoLink(systemName, fileNameWithoutExtension);
            };

            // Open Cover Context Menu
            var openCoverIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
                Width = 16,
                Height = 16
            };
            var openCover = new MenuItem
            {
                Header = "Cover",
                Icon = openCoverIcon
            };
            openCover.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenCover(systemName, fileNameWithoutExtension, systemConfig);
            };

            // Open Title Snapshot Context Menu
            var openTitleSnapshotIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
                Width = 16,
                Height = 16
            };
            var openTitleSnapshot = new MenuItem
            {
                Header = "Title Snapshot",
                Icon = openTitleSnapshotIcon
            };
            openTitleSnapshot.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenTitleSnapshot(systemName, fileNameWithoutExtension);
            };

            // Open Gameplay Snapshot Context Menu
            var openGameplaySnapshotIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
                Width = 16,
                Height = 16
            };
            var openGameplaySnapshot = new MenuItem
            {
                Header = "Gameplay Snapshot",
                Icon = openGameplaySnapshotIcon
            };
            openGameplaySnapshot.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenGameplaySnapshot(systemName, fileNameWithoutExtension);
            };

            // Open Cart Context Menu
            var openCartIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
                Width = 16,
                Height = 16
            };
            var openCart = new MenuItem
            {
                Header = "Cart",
                Icon = openCartIcon
            };
            openCart.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenCart(systemName, fileNameWithoutExtension);
            };

            // Open Video Context Menu
            var openVideoIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
                Width = 16,
                Height = 16
            };
            var openVideo = new MenuItem
            {
                Header = "Video",
                Icon = openVideoIcon
            };
            openVideo.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                PlayVideo(systemName, fileNameWithoutExtension);
            };

            // Open Manual Context Menu
            var openManualIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
                Width = 16,
                Height = 16
            };
            var openManual = new MenuItem
            {
                Header = "Manual",
                Icon = openManualIcon
            };
            openManual.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenManual(systemName, fileNameWithoutExtension);
            };

            // Open Walkthrough Context Menu
            var openWalkthroughIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
                Width = 16,
                Height = 16
            };
            var openWalkthrough = new MenuItem
            {
                Header = "Walkthrough",
                Icon = openWalkthroughIcon
            };
            openWalkthrough.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenWalkthrough(systemName, fileNameWithoutExtension);
            };

            // Open Cabinet Context Menu
            var openCabinetIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
                Width = 16,
                Height = 16
            };
            var openCabinet = new MenuItem
            {
                Header = "Cabinet",
                Icon = openCabinetIcon
            };
            openCabinet.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenCabinet(systemName, fileNameWithoutExtension);
            };

            // Open Flyer Context Menu
            var openFlyerIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
                Width = 16,
                Height = 16
            };
            var openFlyer = new MenuItem
            {
                Header = "Flyer",
                Icon = openFlyerIcon
            };
            openFlyer.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenFlyer(systemName, fileNameWithoutExtension);
            };

            // Open PCB Context Menu
            var openPcbIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
                Width = 16,
                Height = 16
            };
            var openPcb = new MenuItem
            {
                Header = "PCB",
                Icon = openPcbIcon
            };
            openPcb.Click += (_, _) =>
            {
                PlayClick.PlayClickSound();
                OpenPcb(systemName, fileNameWithoutExtension);
            };

            contextMenu.Items.Add(launchMenuItem);
            contextMenu.Items.Add(addToFavorites);
            contextMenu.Items.Add(removeFromFavorites);
            contextMenu.Items.Add(openVideoLink);
            contextMenu.Items.Add(openInfoLink);
            contextMenu.Items.Add(openCover);
            contextMenu.Items.Add(openTitleSnapshot);
            contextMenu.Items.Add(openGameplaySnapshot);
            contextMenu.Items.Add(openCart);
            contextMenu.Items.Add(openVideo);
            contextMenu.Items.Add(openManual);
            contextMenu.Items.Add(openWalkthrough);
            contextMenu.Items.Add(openCabinet);
            contextMenu.Items.Add(openFlyer);
            contextMenu.Items.Add(openPcb);

            // Return
            return contextMenu;
        }

        private void OpenPcb(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
            string[] pcbExtensions = [".png", ".jpg", ".jpeg"];

            foreach (var extension in pcbExtensions)
            {
                string pcbPath = Path.Combine(pcbDirectory, fileName + extension);
                if (File.Exists(pcbPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(pcbPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no PCB file associated with this game.", "PCB not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenFlyer(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
            string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

            foreach (var extension in flyerExtensions)
            {
                string flyerPath = Path.Combine(flyerDirectory, fileName + extension);
                if (File.Exists(flyerPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(flyerPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no flyer file associated with this game.", "Flyer not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCabinet(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
            string[] cabinetExtensions = [".png", ".jpg", ".jpeg"];

            foreach (var extension in cabinetExtensions)
            {
                string cabinetPath = Path.Combine(cabinetDirectory, fileName + extension);
                if (File.Exists(cabinetPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(cabinetPath);
                    imageViewerWindow.Show();
                    return;
                }
            }

            MessageBox.Show("There is no cabinet file associated with this game.", "Cabinet not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenWalkthrough(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walkthroughDirectory = Path.Combine(baseDirectory, "walkthrough", systemName);
            string[] walkthroughExtensions = [".pdf"];

            foreach (var extension in walkthroughExtensions)
            {
                string walkthroughPath = Path.Combine(walkthroughDirectory, fileName + extension);
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
                        string contextMessage = $"There was a problem opening the walkthrough.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                        
                        MessageBox.Show($"Failed to open the walkthrough.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            MessageBox.Show("There is no walkthrough file associated with this game.", "Walkthrough not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenManual(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string manualDirectory = Path.Combine(baseDirectory, "manuals", systemName);
            string[] manualExtensions = [".pdf"];

            foreach (var extension in manualExtensions)
            {
                string manualPath = Path.Combine(manualDirectory, fileName + extension);
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
                        string contextMessage = $"There was a problem opening the manual.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                        
                        MessageBox.Show($"Failed to open the manual.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            MessageBox.Show("There is no manual associated with this file.", "Manual Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PlayVideo(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string videoDirectory = Path.Combine(baseDirectory, "videos", systemName);
            string[] videoExtensions = [".mp4", ".avi", ".mkv"];

            foreach (var extension in videoExtensions)
            {
                string videoPath = Path.Combine(videoDirectory, fileName + extension);
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
            MessageBox.Show("There is no video file associated with this game.", "Video not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCart(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
            string[] cartExtensions = [".png", ".jpg", ".jpeg"];

            foreach (var extension in cartExtensions)
            {
                string cartPath = Path.Combine(cartDirectory, fileName + extension);
                if (File.Exists(cartPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(cartPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no cart file associated with this game.", "Cart not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenGameplaySnapshot(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
            string[] gameplaySnapshotExtensions = [".png", ".jpg", ".jpeg"];

            foreach (var extension in gameplaySnapshotExtensions)
            {
                string gameplaySnapshotPath = Path.Combine(gameplaySnapshotDirectory, fileName + extension);
                if (File.Exists(gameplaySnapshotPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(gameplaySnapshotPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no gameplay snapshot file associated with this game.", "Gameplay Snapshot not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenTitleSnapshot(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
            string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];

            foreach (var extension in titleSnapshotExtensions)
            {
                string titleSnapshotPath = Path.Combine(titleSnapshotDirectory, fileName + extension);
                if (File.Exists(titleSnapshotPath))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(titleSnapshotPath);
                    imageViewerWindow.Show();
                    return;
                }
            }
            MessageBox.Show("There is no title snapshot file associated with this game.", "Title Snapshot not found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCover(string systemName, string fileName, SystemConfig systemConfig)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string systemImageFolder = systemConfig.SystemImageFolder ?? string.Empty;

            // Construct paths for system-specific and global image directories
            string systemSpecificDirectory = Path.Combine(baseDirectory, systemImageFolder);
            string globalDirectory = Path.Combine(baseDirectory, "images", systemName);

            // Image extensions to look for
            string[] imageExtensions = [".png", ".jpg", ".jpeg"];

            // Function to search for the file in a given directory
            bool TryFindImage(string directory, out string foundPath)
            {
                foreach (var extension in imageExtensions)
                {
                    string imagePath = Path.Combine(directory, fileName + extension);
                    if (File.Exists(imagePath))
                    {
                        foundPath = imagePath;
                        return true;
                    }
                }
                foundPath = null;
                return false;
            }

            // Try to find the image in the system-specific directory first
            if (TryFindImage(systemSpecificDirectory, out string foundImagePath) || TryFindImage(globalDirectory, out foundImagePath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(foundImagePath);
                imageViewerWindow.Show();
            }
            else
            {
                MessageBox.Show("There is no cover file associated with this game.", "Cover not found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenInfoLink(string systemName, string fileNameWithoutExtension)
        {
            // Attempt to find a matching machine description
            string searchTerm = fileNameWithoutExtension;
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
            {
                searchTerm = machine.Description;
            }

            string searchUrl = $"{_settings.InfoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
                string contextMessage = $"There was a problem opening the Info Link.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"There was a problem opening the Info Link.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenVideoLink(string systemName, string fileNameWithoutExtension)
        {
            // Attempt to find a matching machine description
            string searchTerm = fileNameWithoutExtension;
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
            {
                searchTerm = machine.Description;
            }

            string searchUrl = $"{_settings.VideoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
                string contextMessage = $"There was a problem opening the Video Link.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"There was a problem opening the Video Link.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFromFavorites(string systemName, string fileNameWithExtension)
        {
            try
            {
                // Load existing favorites
                FavoritesConfig favorites = _favoritesManager.LoadFavorites();

                // Find the favorite to remove
                var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                    && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

                if (favoriteToRemove != null)
                {
                    favorites.FavoriteList.Remove(favoriteToRemove);

                    // Save the updated favorites list
                    _favoritesManager.SaveFavorites(favorites);

                    MessageBox.Show($"{fileNameWithExtension} has been removed from favorites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"{fileNameWithExtension} is not in favorites.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"An error occurred while removing a game from favorites.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"An error occurred while removing this game from favorites.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task HandleDoubleClick(GameListViewItem selectedItem)
        {
            if (selectedItem == null) return;

            string selectedSystem = _systemComboBox.SelectedItem as string;
            var systemConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (systemConfig != null)
            {
                // Launch the game using the full file path stored in the selected item
                await LaunchGame(selectedItem.FilePath);
            }
        }

        private async Task LaunchGame(string filePath)
        {
            PlayClick.PlayClickSound();
            await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
        }

        private void AddToFavorites(string systemName, string fileNameWithExtension)
        {
            try
            {
                // Load existing favorites
                FavoritesConfig favorites = _favoritesManager.LoadFavorites();

                // Add the new favorite if it doesn't already exist
                if (!favorites.FavoriteList.Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                     && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase)))
                {
                    favorites.FavoriteList.Add(new Favorite
                    {
                        FileName = fileNameWithExtension, // Use the file name with an extension
                        SystemName = systemName
                    });

                    // Save the updated favorites list
                    _favoritesManager.SaveFavorites(favorites);

                    MessageBox.Show($"{fileNameWithExtension} has been added to favorites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    MessageBox.Show($"{fileNameWithExtension} is already in favorites.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"An error occurred while adding a game to the favorites.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"An error occurred while adding this game to the favorites.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public void HandleSelectionChanged(GameListViewItem selectedItem)
        {
            if (selectedItem != null)
            {
                string filePath = selectedItem.FilePath;
                string selectedSystem = _systemComboBox.SelectedItem as string;
                var systemConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

                if (systemConfig != null)
                {
                    // Get the preview image path
                    string previewImagePath = GetPreviewImagePath(filePath, systemConfig);

                    // Set the preview image if a valid path is returned
                    if (!string.IsNullOrEmpty(previewImagePath))
                    {
                        _mainWindow.Dispatcher.Invoke(() =>
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(previewImagePath, UriKind.RelativeOrAbsolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load immediately to avoid file locks
                            bitmap.EndInit();
                            _mainWindow.PreviewImage.Source = bitmap;
                        });
                    }
                    else
                    {
                        // Optionally, clear the image if no preview is available
                        _mainWindow.PreviewImage.Source = null;
                    }
                }
            }
        }

        private string GetPreviewImagePath(string filePath, SystemConfig systemConfig)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // Determine the image folder based on whether SystemImageFolder is set
            string imageFolder = !string.IsNullOrEmpty(systemConfig.SystemImageFolder)
                ? systemConfig.SystemImageFolder
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemConfig.SystemName);

            string[] extensions = [".png", ".jpg", ".jpeg"];

            // Look for the image file in the specified or default image folder
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
            string globalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");
            if (File.Exists(globalDefaultImagePath))
            {
                return globalDefaultImagePath;
            }
            return string.Empty; // Return empty if no image is found
        }

    }
}
