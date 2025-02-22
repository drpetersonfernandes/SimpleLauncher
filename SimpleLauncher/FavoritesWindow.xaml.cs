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
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public partial class FavoritesWindow
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
    private readonly FavoritesManager _favoritesManager;
    private ObservableCollection<Favorite> _favoriteList;
    private readonly SettingsConfig _settings;
    private readonly List<SystemConfig> _systemConfigs;
    private readonly List<MameConfig> _machines;
    private readonly MainWindow _mainWindow;

    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public FavoritesWindow(SettingsConfig settings, List<SystemConfig> systemConfigs, List<MameConfig> machines, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        InitializeComponent();

        _settings = settings;
        _systemConfigs = systemConfigs;
        _machines = machines;
        _mainWindow = mainWindow;
        _favoritesManager = favoritesManager;

        App.ApplyThemeToWindow(this);
        LoadFavorites();
        Closing += Favorites_Closing;
    }

    private static void Favorites_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule == null) return;
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
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        return systemConfig == null ? Path.Combine(baseDirectory, "images", "default.png") : FindCoverImagePath(systemName, fileName, baseDirectory, systemConfig.SystemImageFolder);
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList; // Keep the instance in sync
            _favoritesManager.SaveFavorites(); // Save using the existing instance

            PlayClick.PlayClickSound();
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
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                if (CheckFilenameOfSelectedFavorite(selectedFavorite)) return;
                Debug.Assert(selectedFavorite.FileName != null);

                var fileNameWithExtension = selectedFavorite.FileName;
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);

                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
                if (CheckForSystemConfig(systemConfig)) return;
                Debug.Assert(systemConfig?.SystemFolder != null);

                var filePath = GetFullPath(Path.Combine(systemConfig.SystemFolder, selectedFavorite.FileName));

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
                    PlayClick.PlayClickSound();
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
                    RightClickContextMenu.OpenHistoryWindow(selectedFavorite.SystemName, fileNameWithoutExtension, systemConfig, _machines);
                };

                // "Cover" MenuItem
                var coverIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png", UriKind.RelativeOrAbsolute)),
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png", UriKind.RelativeOrAbsolute)),
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

                    void DoYouWanToDeleteMessageBox()
                    {
                        var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                        if (result != MessageBoxResult.Yes) return;
                        try
                        {
                            RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _fakebutton, _fakeGameFileGrid, _mainWindow);
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
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"There was an error in the right-click context menu.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private static string GetFullPath(string path)
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
                MessageBoxLibrary.SelectGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"Error in the LaunchGame_Click method.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async Task LaunchGameFromFavorite(string fileName, string systemName)
    {
        try
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (await CheckSystemConfig(systemConfig)) return;

            var emulatorConfig = systemConfig?.Emulators.FirstOrDefault();
            if (await CheckEmulatorConfig(emulatorConfig)) return;

            Debug.Assert(systemConfig?.SystemFolder != null);
            var fullPath = GetFullPath(Path.Combine(systemConfig.SystemFolder, fileName));

            // Check if the favorite file exists
            if (await CheckFilePathDeleteFavoriteIfInvalid(fileName, systemName, fullPath)) return;

            var mockSystemComboBox = new ComboBox();
            var mockEmulatorComboBox = new ComboBox();

            mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
            mockSystemComboBox.SelectedItem = systemConfig.SystemName;

            mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
            Debug.Assert(emulatorConfig != null, nameof(emulatorConfig) + " != null");
            mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

            // Launch Game
            await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"There was an error launching the game from Favorites.\n\n" +
                                     $"File Path: {fileName}\n" +
                                     $"System Name: {systemName}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

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
            var formattedException = $"Error in the method MouseDoubleClick.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void SetPreviewImageOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

        var imagePath = selectedFavorite.CoverImage;
        PreviewImage.Source = File.Exists(imagePath)
            ? new BitmapImage(new Uri(imagePath, UriKind.Absolute))
            :
            // Set a default image if the selected image doesn't exist
            new BitmapImage(new Uri("pack://application:,,,/images/default.png"));
    }

    private void DeleteFavoriteWithDelButton(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;

        PlayClick.PlayClickSound();

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

    private static string FindCoverImagePath(string systemName, string fileName, string baseDirectory, string systemImageFolder)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }

        var globalDirectory = Path.Combine(baseDirectory, "images", systemName);
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // First try to find the image in the specific directory
        if (TryFindImage(systemImageFolder, out var foundImagePath))
        {
            return foundImagePath;
        }

        // If not found, try the global directory
        return TryFindImage(globalDirectory, out foundImagePath)
            ? foundImagePath
            :

            // If not found, use default image
            Path.Combine(baseDirectory, "images", "default.png");

        // Search for the image file
        bool TryFindImage(string directory, out string foundPath)
        {
            foreach (var extension in imageExtensions)
            {
                var imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                if (!File.Exists(imagePath)) continue;
                foundPath = imagePath;
                return true;
            }

            foundPath = null;
            return false;
        }
    }

    private bool GetSystemConfigOfSelectedFavorite(Favorite selectedFavorite, out SystemConfig systemConfig)
    {
        systemConfig = _systemConfigs?.FirstOrDefault(config =>
            config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemConfig != null) return false;

        // Notify developer
        const string formattedException = "systemConfig is null.";
        Exception exception = new(formattedException);
        LogErrors.LogErrorAsync(exception, formattedException).Wait(TimeSpan.FromSeconds(2));

        // Notify user
        MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

        return true;
    }

    private static bool CheckForSystemConfig(SystemConfig systemConfig)
    {
        if (systemConfig != null) return false;

        // Notify developer
        var formattedException = "systemConfig is null for the selected favorite";
        Exception ex = new(formattedException);
        LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

        // Notify user
        MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

        return true;
    }

    private static bool CheckFilenameOfSelectedFavorite(Favorite selectedFavorite)
    {
        if (selectedFavorite.FileName != null) return false;

        // Notify developer
        var formattedException = "Favorite filename is null";
        Exception ex = new(formattedException);
        LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

        // Notify user
        MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

        return true;
    }

    private async Task<bool> CheckFilePathDeleteFavoriteIfInvalid(string fileName, string systemName, string fullPath)
    {
        if (File.Exists(fullPath)) return false;

        // Auto remove the favorite from the list since the file no longer exists
        var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
        if (favoriteToRemove != null)
        {
            _favoriteList.Remove(favoriteToRemove);
            _favoritesManager.FavoriteList = _favoriteList;
            _favoritesManager.SaveFavorites();
        }

        // Notify developer
        var formattedException = $"Favorite file does not exist: {fullPath}";
        Exception exception = new(formattedException);
        await LogErrors.LogErrorAsync(exception, formattedException);

        // Notify user
        MessageBoxLibrary.GameFileDoesNotExistMessageBox();

        return true;
    }

    private static async Task<bool> CheckEmulatorConfig(SystemConfig.Emulator emulatorConfig)
    {
        if (emulatorConfig != null) return false;

        // Notify developer
        var formattedException = $"emulatorConfig is null.";
        Exception ex = new(formattedException);
        await LogErrors.LogErrorAsync(ex, formattedException);

        // Notify user
        MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

        return true;
    }

    private static async Task<bool> CheckSystemConfig(SystemConfig systemConfig)
    {
        if (systemConfig != null) return false;

        // Notify developer
        var formattedException = $"systemConfig is null.";
        Exception ex = new(formattedException);
        await LogErrors.LogErrorAsync(ex, formattedException);

        // Notify user
        MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

        return true;
    }
}