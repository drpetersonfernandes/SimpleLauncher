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

public partial class Favorites
{
    private readonly FavoritesManager _favoritesManager = new();
    private ObservableCollection<Favorite> _favoriteList;
    private readonly SettingsConfig _settings;
    private readonly List<SystemConfig> _systemConfigs;
    private readonly List<MameConfig> _machines;
    private readonly MainWindow _mainWindow;
    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public Favorites(SettingsConfig settings, List<SystemConfig> systemConfigs, List<MameConfig> machines, MainWindow mainWindow)
    {
        InitializeComponent();
 
        _settings = settings;
        _systemConfigs = systemConfigs;
        _machines = machines;
        _mainWindow = mainWindow;
        
        App.ApplyThemeToWindow(this);

        LoadFavorites();
            
        // Attach event handler
        Closing += Favorites_Closing; 
    }

    // Restart 'Simple Launcher'
    private static void Favorites_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
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
        _favoriteList = [];
        foreach (var favorite in favoritesConfig.FavoriteList)
        {
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(favorite.FileName),
                    StringComparison.OrdinalIgnoreCase));
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
        
        string GetCoverImagePath(string systemName, string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                return Path.Combine(baseDirectory, "images", "default.png");
            }
            return FindCoverImagePath(systemName, fileName, baseDirectory, systemConfig.SystemImageFolder);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                
            PlayClick.PlayClickSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }
    
    private void FavoritesDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                if (selectedFavorite.FileName == null)
                {
                    // Notify developer
                    string formattedException = $"Favorite filename is null";
                    Exception ex = new(formattedException);
                    LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));
                    
                    // Notify user
                    MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
                    
                    return;
                }
                
                string fileNameWithExtension = selectedFavorite.FileName;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);
                
                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
                if (systemConfig == null)
                {
                    // Notify developer
                    string formattedException = $"systemConfig is null for the selected favorite";
                    Exception ex = new(formattedException);
                    LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
                    
                    return;
                }

                string filePath = GetFullPath(Path.Combine(systemConfig.SystemFolder, selectedFavorite.FileName));
                
                var contextMenu = new ContextMenu();

                // "Launch Selected Game" MenuItem
                var launchIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string launchSelectedGame2 = (string)Application.Current.TryFindResource("LaunchSelectedGame") ?? "Launch Selected Game";
                var launchMenuItem = new MenuItem
                {
                    Header = launchSelectedGame2,
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
                string removeFromFavorites2 = (string)Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites";
                var removeMenuItem = new MenuItem
                {
                    Header = removeFromFavorites2,
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
                string openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
                var videoLinkMenuItem = new MenuItem
                {
                    Header = openVideoLink2,
                    Icon = videoLinkIcon
                };
                videoLinkMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenVideoLink(selectedFavorite.SystemName, selectedFavorite.FileName, _machines, _settings);
                };

                // "Open Info Link" MenuItem
                var infoLinkIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
                var infoLinkMenuItem = new MenuItem
                {
                    Header = openInfoLink2,
                    Icon = infoLinkIcon
                };
                infoLinkMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenInfoLink(selectedFavorite.SystemName, selectedFavorite.FileName, _machines, _settings);
                };

                // "Open ROM History" MenuItem
                var openHistoryIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
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
                string cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
                var coverMenuItem = new MenuItem
                {
                    Header = cover2,
                    Icon = coverIcon
                };
                coverMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    
                    if (GetSystemConfigOfSelectedFavorite(selectedFavorite, out var systemConfig1)) return;
                    RightClickContextMenu.OpenCover(selectedFavorite.SystemName, selectedFavorite.FileName, systemConfig1);
                };

                // "Title Snapshot" MenuItem
                var titleSnapshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
                var titleSnapshotMenuItem = new MenuItem
                {
                    Header = titleSnapshot2,
                    Icon = titleSnapshotIcon
                };
                titleSnapshotMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenTitleSnapshot(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Gameplay Snapshot" MenuItem
                var gameplaySnapshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
                var gameplaySnapshotMenuItem = new MenuItem
                {
                    Header = gameplaySnapshot2,
                    Icon = gameplaySnapshotIcon
                };
                gameplaySnapshotMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenGameplaySnapshot(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Cart" MenuItem
                var cartIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
                var cartMenuItem = new MenuItem
                {
                    Header = cart2,
                    Icon = cartIcon
                };
                cartMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenCart(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Video" MenuItem
                var videoIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
                var videoMenuItem = new MenuItem
                {
                    Header = video2,
                    Icon = videoIcon
                };
                videoMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.PlayVideo(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Manual" MenuItem
                var manualIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
                var manualMenuItem = new MenuItem
                {
                    Header = manual2,
                    Icon = manualIcon
                };
                manualMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenManual(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Walkthrough" MenuItem
                var walkthroughIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
                var walkthroughMenuItem = new MenuItem
                {
                    Header = walkthrough2,
                    Icon = walkthroughIcon
                };
                walkthroughMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenWalkthrough(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Cabinet" MenuItem
                var cabinetIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
                var cabinetMenuItem = new MenuItem
                {
                    Header = cabinet2,
                    Icon = cabinetIcon
                };
                cabinetMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenCabinet(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "Flyer" MenuItem
                var flyerIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
                var flyerMenuItem = new MenuItem
                {
                    Header = flyer2,
                    Icon = flyerIcon
                };
                flyerMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenFlyer(selectedFavorite.SystemName, selectedFavorite.FileName);
                };

                // "PCB" MenuItem
                var pcbIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
                var pcbMenuItem = new MenuItem
                {
                    Header = pCb2,
                    Icon = pcbIcon
                };
                pcbMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RightClickContextMenu.OpenPcb(selectedFavorite.SystemName, selectedFavorite.FileName);
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

                takeScreenshot.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    
                    // Notify user
                    MessageBoxLibrary.TakeScreenShotMessageBox();

                    if (GetSystemConfigOfSelectedFavorite(selectedFavorite, out var systemConfig1)) return;
                    _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig1, _fakebutton, _mainWindow);
                    
                    _ = LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
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
                                RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _fakebutton, _fakeGameFileGrid, _mainWindow);
                            }
                            catch (Exception ex)
                            {
                                // Notify developer
                                string formattedException = $"Error deleting the file.\n\n" +
                                                            $"Exception type: {ex.GetType().Name}\n" +
                                                            $"Exception details: {ex.Message}";
                                LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));
                                
                                // Notify user
                                MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                            }
                            RemoveFromFavorites(selectedFavorite);
                        }
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
            string formattedException = $"There was an error in the right-click context menu.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
        
        string GetFullPath(string path)
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
            string formattedException = $"Error in the LaunchGame_Click method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox();
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
                string formattedException = $"systemConfig is null.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox();

                return;
            }

            var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
            if (emulatorConfig == null)
            {
                // Notify developer
                string formattedException = $"emulatorConfig is null.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox();

                return;
            }

            string fullPath = GetFullPath(Path.Combine(systemConfig.SystemFolder, fileName));

            // Get the full path
            string GetFullPath(string path)
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
                
            // Check if the file exists
            if (!File.Exists(fullPath))
            {
                // Auto remove the favorite from the list since the file no longer exists
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
                if (favoriteToRemove != null)
                {
                    _favoriteList.Remove(favoriteToRemove);
                    _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                }
                
                // Notify developer
                string formattedException = $"Favorite file does not exist: {fullPath}";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);

                // Notify user
                MessageBoxLibrary.GameFileDoesNotExistMessageBox();

                return;
            }

            var mockSystemComboBox = new ComboBox();
            var mockEmulatorComboBox = new ComboBox();

            mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
            mockSystemComboBox.SelectedItem = systemConfig.SystemName;

            mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
            mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

            // Launch Game
            await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"There was an error launching the game from Favorites.\n\n" +
                                        $"File Path: {fileName}\n" +
                                        $"System Name: {systemName}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox();
        }
    }
        
    private void RemoveFromFavorites(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });

        PreviewImage.Source = null;
    }

    private async void FavoritesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                PlayClick.PlayClickSound();
                await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error in the method MouseDoubleClick.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox();
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
            PlayClick.PlayClickSound();

            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                _favoriteList.Remove(selectedFavorite);
                _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
            }
            else
            {
                MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
            }
        }
    }
    
    private static string FindCoverImagePath(string systemName, string fileName, string baseDirectory, string systemImageFolder)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
    
        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }
    
        string globalDirectory = Path.Combine(baseDirectory, "images", systemName);
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // First try to find the image in the specific directory
        if (TryFindImage(systemImageFolder, out var foundImagePath))
        {
            return foundImagePath;
        }
        // If not found, try the global directory
        if (TryFindImage(globalDirectory, out foundImagePath))
        {
            return foundImagePath;
        }

        // If not found, use default image
        return Path.Combine(baseDirectory, "images", "default.png");

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
    }
    
    private bool GetSystemConfigOfSelectedFavorite(Favorite selectedFavorite, out SystemConfig systemConfig)
    {
        systemConfig = _systemConfigs?.FirstOrDefault(config =>
            config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemConfig == null)
        {
            // Notify developer
            const string formattedException = "systemConfig is null.";
            Exception exception = new(formattedException);
            LogErrors.LogErrorAsync(exception, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

            return true;
        }

        return false;
    }
}