using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public class GameListFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemConfig> systemConfigs,
    List<MameConfig> machines,
    SettingsConfig settings,
    FavoritesConfig favoritesConfig,
    MainWindow mainWindow)
{
    private readonly FavoritesManager _favoritesManager = new();

    public class GameListViewItem : INotifyPropertyChanged
    {
        private readonly string _fileName;
        private string _machineDescription;
        public string FilePath { get; init; }
        public ContextMenu ContextMenu { get; set; }
        private bool _isFavorite;
            
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        public string FileName
        {
            get => _fileName;
            init
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string MachineDescription
        {
            get => _machineDescription;
            set
            {
                _machineDescription = value;
                OnPropertyChanged(nameof(MachineDescription));
            }
        }
            
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public Task<GameListViewItem> CreateGameListViewItemAsync(string filePath, string systemName, SystemConfig systemConfig)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string machineDescription = systemConfig.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

        // Check if this file is a favorite
        bool isFavorite = favoritesConfig.FavoriteList
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

    private ContextMenu CreateContextMenu(string filePath, string systemName, SystemConfig systemConfig)
    {
        string fileNameWithExtension = Path.GetFileName(filePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            
        var contextMenu = new ContextMenu();

        // Launch Game Context Menu
        var launchMenuItemIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
            Width = 16,
            Height = 16
        };
        string launchGame2 = (string)Application.Current.TryFindResource("LaunchGame") ?? "Launch Game";
        var launchMenuItem = new MenuItem
        {
            Header = launchGame2,
            Icon = launchMenuItemIcon
        };
        launchMenuItem.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            await GameLauncher.HandleButtonClick(filePath, emulatorComboBox, systemComboBox, systemConfigs, settings, mainWindow);
        };

        // Add To Favorites Context Menu
        var addToFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
            Width = 16,
            Height = 16
        };
        string addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
        var addToFavorites = new MenuItem
        {
            Header = addToFavorites2,
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
        string removeFromFavorites2 = (string)Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites";
        var removeFromFavorites = new MenuItem
        {
            Header = removeFromFavorites2,
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
        string openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var openVideoLink = new MenuItem
        {
            Header = openVideoLink2,
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
        string openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var openInfoLink = new MenuItem
        {
            Header = openInfoLink2,
            Icon = openInfoLinkIcon
        };
        openInfoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            OpenInfoLink(systemName, fileNameWithoutExtension);
        };
            
        // Open History Context Menu
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png")),
            Width = 16,
            Height = 16
        };
        string openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryWindow = new MenuItem
        {
            Header = openRomHistory2,
            Icon = openHistoryIcon
        };
        openHistoryWindow.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            OpenHistoryWindow(systemName, fileNameWithoutExtension, systemConfig);
        };

        // Open Cover Context Menu
        var openCoverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        string cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var openCover = new MenuItem
        {
            Header = cover2,
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
        string titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var openTitleSnapshot = new MenuItem
        {
            Header = titleSnapshot2,
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
        string gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var openGameplaySnapshot = new MenuItem
        {
            Header = gameplaySnapshot2,
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
        string cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var openCart = new MenuItem
        {
            Header = cart2,
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
        string video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var openVideo = new MenuItem
        {
            Header = video2,
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
        string manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var openManual = new MenuItem
        {
            Header = manual2,
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
        string walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var openWalkthrough = new MenuItem
        {
            Header = walkthrough2,
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
        string cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var openCabinet = new MenuItem
        {
            Header = cabinet2,
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
        string flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var openFlyer = new MenuItem
        {
            Header = flyer2,
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
        string pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var openPcb = new MenuItem
        {
            Header = pCb2,
            Icon = openPcbIcon
        };
        openPcb.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            OpenPcb(systemName, fileNameWithoutExtension);
        };
            
        // Take Screenshot Context Menu
        var takeScreenshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        string takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
        var takeScreenshot = new MenuItem
        {
            Header = takeScreenshot2,
            Icon = takeScreenshotIcon
        };

        takeScreenshot.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            MessageBoxLibrary.TakeScreenShotMessageBox();
           
            _ = TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig);
            await GameLauncher.HandleButtonClick(filePath, emulatorComboBox, systemComboBox, systemConfigs, settings, mainWindow);
        };
            
        // Delete Game Context Menu
        var deleteGameIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/delete.png")),
            Width = 16,
            Height = 16
        };
        string deleteGame2 = (string)Application.Current.TryFindResource("DeleteGame") ?? "Delete Game";
        var deleteGame = new MenuItem
        {
            Header = deleteGame2,
            Icon = deleteGameIcon
        };
        
        deleteGame.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            
            DoYouWanToDeleteMessageBox();
            void DoYouWanToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DeleteFile(filePath, fileNameWithExtension);
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        string formattedException = $"Error deleting the file.\n\n" +
                                                    $"Exception type: {ex.GetType().Name}\n" +
                                                    $"Exception details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                                
                        // Notify user
                        MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                    }
                    RemoveFromFavorites2(systemName, fileNameWithExtension);
                }
            }
        };

        contextMenu.Items.Add(launchMenuItem);
        contextMenu.Items.Add(addToFavorites);
        contextMenu.Items.Add(removeFromFavorites);
        contextMenu.Items.Add(openVideoLink);
        contextMenu.Items.Add(openInfoLink);
        contextMenu.Items.Add(openHistoryWindow);
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
        contextMenu.Items.Add(takeScreenshot);
        contextMenu.Items.Add(deleteGame);

        // Return
        return contextMenu;
    }
        
    private async Task LaunchGame(string filePath)
    {
        await GameLauncher.HandleButtonClick(filePath, emulatorComboBox, systemComboBox, systemConfigs, settings, mainWindow);
    }

    private void AddToFavorites(string systemName, string fileNameWithExtension)
    {
        try
        {
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            if (!favorites.FavoriteList.Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                 && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase)))
            {
                favorites.FavoriteList.Add(new Favorite
                {
                    FileName = fileNameWithExtension,
                    SystemName = systemName
                });

                _favoritesManager.SaveFavorites(favorites);

                // Find the GameListViewItem and update its IsFavorite property
                var gameItem = mainWindow.GameListItems
                    .FirstOrDefault(g => g.FileName.Equals(Path.GetFileNameWithoutExtension(fileNameWithExtension), StringComparison.OrdinalIgnoreCase)); 
               
                if (gameItem != null)
                {
                    gameItem.IsFavorite = true;
                }
            }
            else
            {
                // Notify user
                MessageBoxLibrary.GameIsAlreadyInFavoritesMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while adding a game to the favorites.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
        }
    }
        
    private void RemoveFromFavorites(string systemName, string fileNameWithExtension)
    {
        try
        {
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);
                _favoritesManager.SaveFavorites(favorites);

                // Find the GameListViewItem and update its IsFavorite property
                var gameItem = mainWindow.GameListItems
                    .FirstOrDefault(g => g.FileName.Equals(Path.GetFileNameWithoutExtension(fileNameWithExtension), StringComparison.OrdinalIgnoreCase)); 
                    
                if (gameItem != null)
                {
                    gameItem.IsFavorite = false;
                }
            }
            else
            {
                // Notify user
                MessageBoxLibrary.FileIsNotInFavoritesMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while removing a game from favorites.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            MessageBoxLibrary.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }
    
    private void RemoveFromFavorites2(string systemName, string fileNameWithExtension)
    {
        try
        {
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);
                _favoritesManager.SaveFavorites(favorites);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred in the method RemoveFromFavorites2 in the class GameListFactory.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
    
    private void OpenVideoLink(string systemName, string fileNameWithoutExtension)
    {
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        string searchUrl = $"{settings.VideoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
            // Notify developer
            string contextMessage = $"There was a problem opening the Video Link.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorOpeningVideoLinkMessageBox();
        }
    }

    private void OpenInfoLink(string systemName, string fileNameWithoutExtension)
    {
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        string searchUrl = $"{settings.InfoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
            // Notify developer
            string contextMessage = $"There was a problem opening the Info Link.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ProblemOpeningInfoLinkMessageBox();
        }
    }
        
    private void OpenHistoryWindow(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig)
    {
        string romName = fileNameWithoutExtension.ToLowerInvariant();
           
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
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
            // Notify developer
            string contextMessage = $"There was a problem opening the History window.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.CouldNotOpenHistoryWindowMessageBox();
        }
    }
    
    private void OpenCover(string systemName, string fileName, SystemConfig systemConfig)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string systemImageFolder = systemConfig.SystemImageFolder;
        
        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }
        
        string globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

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

        // Try to find the image in the systemImageFolder directory first
        // Then try to find in the globalImageDirectory
        if (TryFindImage(systemImageFolder, out string foundImagePath) || TryFindImage(globalImageDirectory, out foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(foundImagePath);
            imageViewerWindow.Show();
                
        }
        else
        {
            // Notify user
            MessageBoxLibrary.ThereIsNoCoverMessageBox();
        }
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

        // Notify user
        MessageBoxLibrary.ThereIsNoTitleSnapshotMessageBox();
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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoGameplaySnapshotMessageBox();
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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoCartMessageBox();
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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoVideoFileMessageBox();
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
                    // Notify developer
                    string contextMessage = $"There was a problem opening the manual.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.CouldNotOpenManualMessageBox();

                    return;
                }
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoManualMessageBox();
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
                    // Notify developer
                    string contextMessage = $"There was a problem opening the walkthrough.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.CouldNotOpenWalkthroughMessageBox();
                    
                    return;
                }
            }
        }
        
        // Notify user
        MessageBoxLibrary.ThereIsNoWalkthroughMessageBox();
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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoCabinetMessageBox();
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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoFlyerMessageBox();
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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoPcbMessageBox();
    }

    private async Task TakeScreenshotOfSelectedWindow(string fileNameWithoutExtension, SystemConfig systemConfig)
    {
        try
        {
            // Optionally, clear the preview image
            mainWindow.PreviewImage.Source = null;
            
            string systemName = systemConfig.SystemName;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string systemImageFolder = systemConfig.SystemImageFolder;

            if (string.IsNullOrEmpty(systemImageFolder))
            {
                systemImageFolder = Path.Combine(baseDirectory, "images", systemName);
                Directory.CreateDirectory(systemImageFolder);
            }
            
            // Wait for 4 seconds
            await Task.Delay(4000);
                
            // Get the list of open windows
            var openWindows = WindowManager.GetOpenWindows();

            // Show the selection dialog
            var dialog = new WindowSelectionDialog(openWindows);
            if (dialog.ShowDialog() != true || dialog.SelectedWindowHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr hWnd = dialog.SelectedWindowHandle;
                
            WindowScreenshot.Rect rect;

            // Try to get the client area dimensions
            if (!WindowScreenshot.GetClientAreaRect(hWnd, out var clientRect))
            {
                // If the client area fails, fall back to the full window dimensions
                if (!WindowScreenshot.GetWindowRect(hWnd, out rect))
                {
                    throw new Exception("Failed to retrieve window dimensions.");
                }
            }
            else
            {
                // Successfully retrieved client area
                rect = clientRect;
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            string screenshotPath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}.png");

            // Capture the window into a bitmap
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        new System.Drawing.Point(rect.Left, rect.Top),
                        System.Drawing.Point.Empty,
                        new System.Drawing.Size(width, height));
                }

                // Save the screenshot
                bitmap.Save(screenshotPath, ImageFormat.Png);
            }

            PlayClick.PlayShutterSound();
            
            // Wait
            await Task.Delay(1000);
            
            // Show the flash effect
            var flashWindow = new FlashOverlayWindow();
            await flashWindow.ShowFlashAsync();
            
            // Reload the current Game List
            await mainWindow.LoadGameFilesAsync();

        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"There was a problem saving the screenshot.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.CouldNotSaveScreenshotMessageBox();
        }
    }

    private async void DeleteFile(string filePath, string fileNameWithExtension)
    {
        try
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    PlayClick.PlayTrashSound();

                    // Notify user
                    MessageBoxLibrary.FileSuccessfullyDeletedMessageBox(fileNameWithExtension);
                    
                    // Reload the current Game List
                    await mainWindow.LoadGameFilesAsync();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'." +
                                          $"Exception type: {ex.GetType().Name}\n" +
                                          $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, errorMessage);
                    
                    // Notify user
                    MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
                }
            }
            else
            {
                // Notify user
                MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Generic error in the method DeleteFile.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
        }
    }

    public void HandleSelectionChanged(GameListViewItem selectedItem)
    {
        if (selectedItem != null)
        {
            string filePath = selectedItem.FilePath;
            string selectedSystem = systemComboBox.SelectedItem as string;
            var systemConfig = systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
            if (systemConfig != null)
            {
                // Get the preview image path
                string previewImagePath = GetPreviewImagePath(filePath, systemConfig);
                
                // Set the preview image if a valid path is returned
                if (!string.IsNullOrEmpty(previewImagePath))
                {
                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(previewImagePath);
                        MemoryStream memoryStream = new MemoryStream(imageBytes);

                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = memoryStream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load immediately to avoid file locks
                            bitmap.EndInit();
                            mainWindow.PreviewImage.Source = bitmap;
                        });
                    }
                    catch (Exception ex)
                    {
                        mainWindow.PreviewImage.Source = null;
                        
                        // Notify developer
                        string errorMessage = $"An error occurred while setting up the preview image in the GameListFactory class.\n\n" +
                                              $"Exception type: {ex.GetType().Name}\n" +
                                              $"Exception details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                    }
                }
                else
                {
                    // Clear the image if no preview is available
                    mainWindow.PreviewImage.Source = null;

                    MessageBoxLibrary.DefaultImageNotFoundMessageBox();
                }
            }
        }
    }

    private string GetPreviewImagePath(string filePath, SystemConfig systemConfig)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string imageFolder = systemConfig.SystemImageFolder;
        
        // Be sure that SystemImageFolder path is absolute
        if (!Path.IsPathRooted(imageFolder))
        {
            if (imageFolder != null) imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageFolder);
        }
        
        imageFolder = !string.IsNullOrEmpty(imageFolder)
            ? imageFolder
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemConfig.SystemName);

        string[] extensions = [".png", ".jpg", ".jpeg"];

        // Function to get the imagePath
        foreach (var extension in extensions)
        {
            string imagePath = Path.Combine(imageFolder, $"{fileNameWithoutExtension}{extension}");
            if (File.Exists(imagePath))
            {
                return imagePath;
            }
        }

        // load default image
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
        
        return string.Empty; // Return empty if no image is found (not even a default image)
    }
        
    private string GetMachineDescription(string fileName)
    {
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        return machine?.Description ?? string.Empty;
    }
    
    public async Task HandleDoubleClick(GameListViewItem selectedItem)
    {
        if (selectedItem == null) return;

        string selectedSystem = systemComboBox.SelectedItem as string;
        var systemConfig = systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

        if (systemConfig != null)
        {
            // Launch the game using the full file path stored in the selected item
            await LaunchGame(selectedItem.FilePath);
        }
    }
}