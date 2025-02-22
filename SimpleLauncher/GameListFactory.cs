using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    FavoritesManager favoritesManager,
    MainWindow mainWindow)
{
    private readonly WrapPanel _fakeFileGrid = new();
    private readonly Button _fakeButton = new();

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
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var machineDescription = systemConfig.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

        // Check if this file is a favorite
        var isFavorite = favoritesManager.FavoriteList
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
        var fileNameWithExtension = Path.GetFileName(filePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

        var contextMenu = new ContextMenu();

        // Launch Game Context Menu
        var launchMenuItemIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
            Width = 16,
            Height = 16
        };
        var launchGame2 = (string)Application.Current.TryFindResource("LaunchGame") ?? "Launch Game";
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
        var addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
        var addToFavorites = new MenuItem
        {
            Header = addToFavorites2,
            Icon = addToFavoritesIcon
        };
        addToFavorites.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.AddToFavorites(systemName, fileNameWithExtension, favoritesManager, _fakeFileGrid, mainWindow);
        };

        // Remove From Favorites Context Menu
        var removeFromFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png")),
            Width = 16,
            Height = 16
        };
        var removeFromFavorites2 = (string)Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites";
        var removeFromFavorites = new MenuItem
        {
            Header = removeFromFavorites2,
            Icon = removeFromFavoritesIcon
        };
        removeFromFavorites.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, favoritesManager, _fakeFileGrid, mainWindow);
        };

        // Open Video Link Context Menu
        var openVideoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var openVideoLink = new MenuItem
        {
            Header = openVideoLink2,
            Icon = openVideoLinkIcon
        };
        openVideoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenVideoLink(systemName, fileNameWithoutExtension, machines, settings);
        };

        // Open Info Link Context Menu
        var openInfoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
            Width = 16,
            Height = 16
        };
        var openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var openInfoLink = new MenuItem
        {
            Header = openInfoLink2,
            Icon = openInfoLinkIcon
        };
        openInfoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenInfoLink(systemName, fileNameWithoutExtension, machines, settings);
        };

        // Open History Context Menu
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png")),
            Width = 16,
            Height = 16
        };
        var openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryWindow = new MenuItem
        {
            Header = openRomHistory2,
            Icon = openHistoryIcon
        };
        openHistoryWindow.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenHistoryWindow(systemName, fileNameWithoutExtension, systemConfig, machines);
        };

        // Open Cover Context Menu
        var openCoverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        var cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var openCover = new MenuItem
        {
            Header = cover2,
            Icon = openCoverIcon
        };
        openCover.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCover(systemName, fileNameWithoutExtension, systemConfig);
        };

        // Open Title Snapshot Context Menu
        var openTitleSnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var openTitleSnapshot = new MenuItem
        {
            Header = titleSnapshot2,
            Icon = openTitleSnapshotIcon
        };
        openTitleSnapshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenTitleSnapshot(systemName, fileNameWithoutExtension);
        };

        // Open Gameplay Snapshot Context Menu
        var openGameplaySnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var openGameplaySnapshot = new MenuItem
        {
            Header = gameplaySnapshot2,
            Icon = openGameplaySnapshotIcon
        };
        openGameplaySnapshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenGameplaySnapshot(systemName, fileNameWithoutExtension);
        };

        // Open Cart Context Menu
        var openCartIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
            Width = 16,
            Height = 16
        };
        var cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var openCart = new MenuItem
        {
            Header = cart2,
            Icon = openCartIcon
        };
        openCart.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCart(systemName, fileNameWithoutExtension);
        };

        // Open Video Context Menu
        var openVideoIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var openVideo = new MenuItem
        {
            Header = video2,
            Icon = openVideoIcon
        };
        openVideo.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.PlayVideo(systemName, fileNameWithoutExtension);
        };

        // Open Manual Context Menu
        var openManualIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
            Width = 16,
            Height = 16
        };
        var manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var openManual = new MenuItem
        {
            Header = manual2,
            Icon = openManualIcon
        };
        openManual.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenManual(systemName, fileNameWithoutExtension);
        };

        // Open Walkthrough Context Menu
        var openWalkthroughIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
            Width = 16,
            Height = 16
        };
        var walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var openWalkthrough = new MenuItem
        {
            Header = walkthrough2,
            Icon = openWalkthroughIcon
        };
        openWalkthrough.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenWalkthrough(systemName, fileNameWithoutExtension);
        };

        // Open Cabinet Context Menu
        var openCabinetIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
            Width = 16,
            Height = 16
        };
        var cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var openCabinet = new MenuItem
        {
            Header = cabinet2,
            Icon = openCabinetIcon
        };
        openCabinet.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCabinet(systemName, fileNameWithoutExtension);
        };

        // Open Flyer Context Menu
        var openFlyerIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
            Width = 16,
            Height = 16
        };
        var flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var openFlyer = new MenuItem
        {
            Header = flyer2,
            Icon = openFlyerIcon
        };
        openFlyer.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenFlyer(systemName, fileNameWithoutExtension);
        };

        // Open PCB Context Menu
        var openPcbIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
            Width = 16,
            Height = 16
        };
        var pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var openPcb = new MenuItem
        {
            Header = pCb2,
            Icon = openPcbIcon
        };
        openPcb.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenPcb(systemName, fileNameWithoutExtension);
        };

        // Take Screenshot Context Menu
        var takeScreenshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
        var takeScreenshot = new MenuItem
        {
            Header = takeScreenshot2,
            Icon = takeScreenshotIcon
        };

        takeScreenshot.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            MessageBoxLibrary.TakeScreenShotMessageBox();

            _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig, _fakeButton, mainWindow);
            await GameLauncher.HandleButtonClick(filePath, emulatorComboBox, systemComboBox, systemConfigs, settings, mainWindow);
        };

        // Delete Game Context Menu
        var deleteGameIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/delete.png")),
            Width = 16,
            Height = 16
        };
        var deleteGame2 = (string)Application.Current.TryFindResource("DeleteGame") ?? "Delete Game";
        var deleteGame = new MenuItem
        {
            Header = deleteGame2,
            Icon = deleteGameIcon
        };

        deleteGame.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();

            DoYouWanToDeleteMessageBox();
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

        void DoYouWanToDeleteMessageBox()
        {
            var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _fakeButton, _fakeFileGrid, mainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    var formattedException = $"Error deleting the file.\n\n" +
                                             $"Exception type: {ex.GetType().Name}\n" +
                                             $"Exception details: {ex.Message}";
                    LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                }

                RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, favoritesManager, _fakeFileGrid, mainWindow);
            }
        }
    }

    public void HandleSelectionChanged(GameListViewItem selectedItem)
    {
        if (selectedItem == null) return;

        var filePath = selectedItem.FilePath;
        var selectedSystem = systemComboBox.SelectedItem as string;
        var systemConfig = systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
        if (systemConfig == null) return;

        // Get the preview image path
        var previewImagePath = GetPreviewImagePath(filePath, systemConfig);

        // Set the preview image if a valid path is returned
        if (!string.IsNullOrEmpty(previewImagePath))
        {
            try
            {
                var imageBytes = File.ReadAllBytes(previewImagePath);
                var memoryStream = new MemoryStream(imageBytes);

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
                var errorMessage = $"An error occurred while setting up the preview image in the GameListFactory class.\n\n" +
                                   $"Exception type: {ex.GetType().Name}\n" +
                                   $"Exception details: {ex.Message}";
                LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
            }
        }
        else
        {
            // Clear the image if no preview is available
            mainWindow.PreviewImage.Source = null;

            // Notify user
            MessageBoxLibrary.DefaultImageNotFoundMessageBox();
        }
    }

    private static string GetPreviewImagePath(string filePath, SystemConfig systemConfig)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var imageFolder = systemConfig.SystemImageFolder;

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
            var imagePath = Path.Combine(imageFolder, $"{fileNameWithoutExtension}{extension}");
            if (File.Exists(imagePath))
            {
                return imagePath;
            }
        }

        // load default image
        // If no specific image found, try the user-defined default image in SystemImageFolder
        var userDefinedDefaultImagePath = Path.Combine(imageFolder, "default.png");
        if (File.Exists(userDefinedDefaultImagePath))
        {
            return userDefinedDefaultImagePath;
        }

        // If user-defined default image isn't found, fallback to the global default image
        var globalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");

        // Return empty if no image is found (not even a default image)
        return File.Exists(globalDefaultImagePath) ? globalDefaultImagePath : string.Empty;
    }

    private string GetMachineDescription(string fileName)
    {
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        return machine?.Description ?? string.Empty;
    }

    public async Task HandleDoubleClick(GameListViewItem selectedItem)
    {
        if (selectedItem == null) return;

        var selectedSystem = systemComboBox.SelectedItem as string;
        var systemConfig = systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

        if (systemConfig != null)
        {
            // Launch the game using the full file path stored in the selected item
            await GameLauncher.HandleButtonClick(selectedItem.FilePath, emulatorComboBox, systemComboBox, systemConfigs, settings, mainWindow);
        }
    }
}