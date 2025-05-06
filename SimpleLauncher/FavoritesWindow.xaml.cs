using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SimpleLauncher.Services;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public partial class FavoritesWindow
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
    private readonly FavoritesManager _favoritesManager;
    private ObservableCollection<Favorite> _favoriteList;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemConfigs;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;

    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public FavoritesWindow(SettingsManager settings, List<SystemManager> systemConfigs, List<MameManager> machines, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        InitializeComponent();

        _settings = settings;
        _systemConfigs = systemConfigs;
        _machines = machines;
        _mainWindow = mainWindow;
        _favoritesManager = favoritesManager;

        App.ApplyThemeToWindow(this);
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        var favoritesConfig = FavoritesManager.LoadFavorites();
        _favoriteList = [];
        foreach (var favorite in favoritesConfig.FavoriteList)
        {
            // Find machine description if available
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(favorite.FileName),
                    StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? string.Empty;

            // Retrieve the system configuration for the favorite
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(favorite.SystemName, StringComparison.OrdinalIgnoreCase));

            // Get the default emulator, e.g., the first one in the list
            var defaultEmulator = systemConfig?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

            var favoriteItem = new Favorite
            {
                FileName = favorite.FileName,
                SystemName = favorite.SystemName,
                MachineDescription = machineDescription,
                DefaultEmulator = defaultEmulator,
                CoverImage = GetCoverImagePath(favorite.SystemName, favorite.FileName)
            };
            _favoriteList.Add(favoriteItem);
        }

        FavoritesDataGrid.ItemsSource = _favoriteList;
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var defaultImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemConfig == null)
        {
            return defaultImagePath;
        }
        else
        {
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemConfig);
        }
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList; // Keep the instance in sync
            _favoritesManager.SaveFavorites(); // Save using the existing instance

            PlayClick.PlayTrashSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }

    private void FavoritesWindowRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            if (selectedFavorite.FileName == null)
            {
                // Notify developer
                const string contextMessage = "Favorite filename is null";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null for the selected favorite";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var fileNameWithExtension = selectedFavorite.FileName;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);
            var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemConfig.SystemFolder);
            var filePath = PathHelper.CombineAndResolveRelativeToCurrentDirectory(systemFolderPath, selectedFavorite.FileName);

            AddRightClickContextMenuFavoritesWindow(fileNameWithExtension, selectedFavorite, fileNameWithoutExtension, systemConfig, filePath);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private void AddRightClickContextMenuFavoritesWindow(string fileNameWithExtension, Favorite selectedFavorite, string fileNameWithoutExtension, SystemManager systemManager, string filePath)
    {
        var contextMenu = new ContextMenu();

        // "Launch Selected Game" MenuItem
        var launchIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png", UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var launchSelectedGame2 = (string)Application.Current.TryFindResource("LaunchSelectedGame") ?? "Launch Selected Game";
        var launchMenuItem = new MenuItem
        {
            Header = launchSelectedGame2,
            Icon = launchIcon
        };
        launchMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            _ = LaunchGameFromFavorite(fileNameWithExtension, selectedFavorite.SystemName);
        };

        // "Remove from Favorites" MenuItem
        var removeIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png", UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var removeFromFavorites2 = (string)Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites";
        var removeMenuItem = new MenuItem
        {
            Header = removeFromFavorites2,
            Icon = removeIcon
        };
        removeMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayTrashSound();
            RemoveFavoriteFromXmlAndEmptyPreviewImage(selectedFavorite);
        };

        // "Open Video Link" MenuItem
        var videoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png", UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var videoLinkMenuItem = new MenuItem
        {
            Header = openVideoLink2,
            Icon = videoLinkIcon
        };
        videoLinkMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenVideoLink(selectedFavorite.SystemName, fileNameWithoutExtension, _machines, _settings);
        };

        // "Open Info Link" MenuItem
        var infoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png", UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var infoLinkMenuItem = new MenuItem
        {
            Header = openInfoLink2,
            Icon = infoLinkIcon
        };
        infoLinkMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenInfoLink(selectedFavorite.SystemName, fileNameWithoutExtension, _machines, _settings);
        };

        // "Open ROM History" MenuItem
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png", UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryMenuItem = new MenuItem
        {
            Header = openRomHistory2,
            Icon = openHistoryIcon
        };
        openHistoryMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenRomHistoryWindow(selectedFavorite.SystemName, fileNameWithoutExtension, systemManager, _machines);
        };

        // "Cover" MenuItem
        var coverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        var cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var coverMenuItem = new MenuItem
        {
            Header = cover2,
            Icon = coverIcon
        };
        coverMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();

            if (GetSystemConfigOfSelectedFavorite(selectedFavorite, out var systemConfig1)) return;

            RightClickContextMenu.OpenCover(selectedFavorite.SystemName, fileNameWithoutExtension, systemConfig1);
        };

        // "Title Snapshot" MenuItem
        var titleSnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var titleSnapshotMenuItem = new MenuItem
        {
            Header = titleSnapshot2,
            Icon = titleSnapshotIcon
        };
        titleSnapshotMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenTitleSnapshot(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Gameplay Snapshot" MenuItem
        var gameplaySnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var gameplaySnapshotMenuItem = new MenuItem
        {
            Header = gameplaySnapshot2,
            Icon = gameplaySnapshotIcon
        };
        gameplaySnapshotMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenGameplaySnapshot(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Cart" MenuItem
        var cartIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
            Width = 16,
            Height = 16
        };
        var cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var cartMenuItem = new MenuItem
        {
            Header = cart2,
            Icon = cartIcon
        };
        cartMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCart(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Video" MenuItem
        var videoIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var videoMenuItem = new MenuItem
        {
            Header = video2,
            Icon = videoIcon
        };
        videoMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.PlayVideo(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Manual" MenuItem
        var manualIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
            Width = 16,
            Height = 16
        };
        var manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var manualMenuItem = new MenuItem
        {
            Header = manual2,
            Icon = manualIcon
        };
        manualMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenManual(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Walkthrough" MenuItem
        var walkthroughIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
            Width = 16,
            Height = 16
        };
        var walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var walkthroughMenuItem = new MenuItem
        {
            Header = walkthrough2,
            Icon = walkthroughIcon
        };
        walkthroughMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenWalkthrough(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Cabinet" MenuItem
        var cabinetIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
            Width = 16,
            Height = 16
        };
        var cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var cabinetMenuItem = new MenuItem
        {
            Header = cabinet2,
            Icon = cabinetIcon
        };
        cabinetMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCabinet(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "Flyer" MenuItem
        var flyerIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
            Width = 16,
            Height = 16
        };
        var flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var flyerMenuItem = new MenuItem
        {
            Header = flyer2,
            Icon = flyerIcon
        };
        flyerMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenFlyer(selectedFavorite.SystemName, fileNameWithoutExtension);
        };

        // "PCB" MenuItem
        var pcbIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
            Width = 16,
            Height = 16
        };
        var pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var pcbMenuItem = new MenuItem
        {
            Header = pCb2,
            Icon = pcbIcon
        };
        pcbMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenPcb(selectedFavorite.SystemName, fileNameWithoutExtension);
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

        takeScreenshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();

            // Notify user
            MessageBoxLibrary.TakeScreenShotMessageBox();

            if (GetSystemConfigOfSelectedFavorite(selectedFavorite, out var systemConfig1)) return;

            _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig1, _fakebutton, _mainWindow);

            _ = LaunchGameFromFavorite(fileNameWithExtension, selectedFavorite.SystemName);
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
            return;

            void DoYouWanToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _mainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error deleting the file.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                }

                RemoveFavoriteFromXmlAndEmptyPreviewImage(selectedFavorite);
            }
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
        contextMenu.Items.Add(takeScreenshot);
        contextMenu.Items.Add(deleteGame);
        contextMenu.IsOpen = true;
    }

    private async void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                PlayClick.PlayClickSound();
                await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
            }
            else
            {
                // Notify user
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the LaunchGame_Click method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async Task LaunchGameFromFavorite(string fileName, string systemName)
    {
        try
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
            if (emulatorConfig == null)
            {
                // Notify developer
                const string contextMessage = "emulatorConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var partialPath = PathHelper.CombineAndResolveRelativeToCurrentDirectory(systemConfig.SystemFolder, fileName);
            var fullPath = PathHelper.ResolveRelativeToAppDirectory(partialPath);

            if (!File.Exists(fullPath))
            {
                // Auto remove the favorite from the list since the file no longer exists
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
                if (favoriteToRemove != null)
                {
                    _favoriteList.Remove(favoriteToRemove);
                    _favoritesManager.FavoriteList = _favoriteList;
                    _favoritesManager.SaveFavorites();
                }

                // Notify developer
                var contextMessage = $"Favorite file does not exist: {fullPath}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.GameFileDoesNotExistMessageBox();

                return;
            }

            var mockSystemComboBox = new ComboBox();
            var mockEmulatorComboBox = new ComboBox();
            mockSystemComboBox.ItemsSource = _systemConfigs.Select(static config => config.SystemName).ToList();
            mockSystemComboBox.SelectedItem = systemConfig.SystemName;
            mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
            mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

            // Launch Game
            await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"There was an error launching the game from Favorites.\n" +
                                 $"File Path: {fileName}\n" +
                                 $"System Name: {systemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void RemoveFavoriteFromXmlAndEmptyPreviewImage(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        _favoritesManager.FavoriteList = _favoriteList;
        _favoritesManager.SaveFavorites();

        PreviewImage.Source = null;
    }

    private async void LaunchGameWithDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            PlayClick.PlayClickSound();
            await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method MouseDoubleClick.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async void SetPreviewImageOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                return;
            }

            var imagePath = selectedFavorite.CoverImage;

            // Use the new ImageLoader to load the image
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            // Assign the loaded image to the PreviewImage control
            PreviewImage.Source = loadedImage;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChanged method.");
        }
    }

    private void DeleteFavoriteWithDelButton(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;

        PlayClick.PlayTrashSound();

        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList;
            _favoritesManager.SaveFavorites();
        }
        else
        {
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }

    private bool GetSystemConfigOfSelectedFavorite(Favorite selectedFavorite, out SystemManager systemManager)
    {
        systemManager = _systemConfigs?.FirstOrDefault(config =>
            config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemManager != null) return false;

        // Notify developer
        const string contextMessage = "systemManager is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

        return true;
    }
}
