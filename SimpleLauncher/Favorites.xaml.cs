using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
    private readonly FavoritesManager _favoritesManager;
    private ObservableCollection<Favorite> _favoriteList;
    private readonly SettingsConfig _settings;
    private readonly List<SystemConfig> _systemConfigs;
    private readonly List<MameConfig> _machines;
    private readonly MainWindow _mainWindow;

    public Favorites(SettingsConfig settings, List<SystemConfig> systemConfigs, List<MameConfig> machines, MainWindow mainWindow)
    {
        InitializeComponent();
 
        // Apply theme to the window
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
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        if (systemConfig == null)
        {
            return Path.Combine(baseDirectory, "images", "default.png");
        }
        return FindCoverImagePath(systemName, fileName, baseDirectory, systemConfig.SystemImageFolder);
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
            SelectAFavoriteToRemoveMessageBox();
        }

        void SelectAFavoriteToRemoveMessageBox()
        {
            string pleaseselectafavoritetoremove2 = (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ?? "Please select a favorite to remove.";
            string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            MessageBox.Show(pleaseselectafavoritetoremove2,
                warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                    
                    // Notify user
                    FavoriteFilenameIsNullMessageBox();
                    
                    return;

                    void FavoriteFilenameIsNullMessageBox()
                    {
                        string therewasanerrorloadingtheinfo2 = (string)Application.Current.TryFindResource("Therewasanerrorloadingtheinfo") ?? "There was an error loading the info for this favorite.";
                        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                        MessageBox.Show($"{therewasanerrorloadingtheinfo2}\n\n" +
                                        $"{theerrorwasreportedtothedeveloper2}",
                            error2, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                string fileNameWithExtension = selectedFavorite.FileName;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);
                
                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
                if (systemConfig == null)
                {
                    // Notify developer
                    string formattedException = $"systemConfig is null for the selected favorite";
                    Exception ex = new(formattedException);
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    SystemConfigIsNullMessageBox();
                    void SystemConfigIsNullMessageBox()
                    {
                        string therewasanerrorloadingthesystem2 = (string)Application.Current.TryFindResource("Therewasanerrorloadingthesystem") ?? "There was an error loading the system configuration for this favorite.";
                        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                        MessageBox.Show($"{therewasanerrorloadingthesystem2}\n\n" +
                                        $"{theerrorwasreportedtothedeveloper2}",
                            error2, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
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
                    OpenVideoLink(selectedFavorite.SystemName, selectedFavorite.FileName, selectedFavorite.MachineDescription);
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
                    OpenInfoLink(selectedFavorite.SystemName, selectedFavorite.FileName, selectedFavorite.MachineDescription);
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
                    OpenRomHistoryWindow(selectedFavorite.SystemName, fileNameWithoutExtension, systemConfig);
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
                    OpenCover(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenTitleSnapshot(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenGameplaySnapshot(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenCart(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    PlayVideo(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenManual(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenWalkthrough(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenCabinet(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenFlyer(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    OpenPcb(selectedFavorite.SystemName, selectedFavorite.FileName);
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
                    TakeScreenShotMessageBox();
                    void TakeScreenShotMessageBox()
                    {
                        string thegamewilllaunchnow2 = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
                        string setthegamewindowto2 = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
                        string youshouldchangetheemulatorparameters2 = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
                        string aselectionwindowwillopeninSimpleLauncherallowingyou2 = (string)Application.Current.TryFindResource("AselectionwindowwillopeninSimpleLauncherallowingyou") ?? "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.";
                        string assoonasyouselectawindow2 = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
                        MessageBox.Show($"{thegamewilllaunchnow2}\n\n" +
                                        $"{setthegamewindowto2}\n\n" +
                                        $"{youshouldchangetheemulatorparameters2}\n\n" +
                                        $"{aselectionwindowwillopeninSimpleLauncherallowingyou2}\n\n" +
                                        $"{assoonasyouselectawindow2}",
                            takeScreenshot2, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    _ = TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig.SystemName);
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
                        string areyousureyouwanttodeletethefile2 = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethefile") ?? "Are you sure you want to delete the file";
                        string thisactionwilldelete2 = (string)Application.Current.TryFindResource("Thisactionwilldelete") ?? "This action will delete the file from the HDD and cannot be undone.";
                        string confirmDeletion2 = (string)Application.Current.TryFindResource("ConfirmDeletion") ?? "Confirm Deletion";
                        var result = MessageBox.Show($"{areyousureyouwanttodeletethefile2} \"{fileNameWithExtension}\"?\n\n" +
                                                     $"{thisactionwilldelete2}",
                            confirmDeletion2, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            DeleteFile(filePath, fileNameWithExtension);
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
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            ContextMenuErrorMessageBox();
            void ContextMenuErrorMessageBox()
            {
                string therewasanerrorintherightclick2 = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{therewasanerrorintherightclick2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                SelectGameToLaunchMessageBox();
                void SelectGameToLaunchMessageBox()
                {
                    string pleaseselectagametolaunch2 = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
                    string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
                    MessageBox.Show(pleaseselectagametolaunch2,
                        info2, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error in the LaunchGame_Click method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
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
                CouldNotLaunchThisGameMessageBox();

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
                CouldNotLaunchThisGameMessageBox();

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
                // Notify developer
                string formattedException = $"Favorite file does not exist: {fullPath}";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                    
                // Auto remove the favorite from the list since the file no longer exists
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
                if (favoriteToRemove != null)
                {
                    _favoriteList.Remove(favoriteToRemove);
                    _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                }

                // Notify user
                GameFileDoesNotExistMessageBox();
                void GameFileDoesNotExistMessageBox()
                {
                    string thegamefiledoesnotexist2 = (string)Application.Current.TryFindResource("Thegamefiledoesnotexist") ?? "The game file does not exist!";
                    string thefavoritehasbeenremovedfromthelist2 = (string)Application.Current.TryFindResource("Thefavoritehasbeenremovedfromthelist") ?? "The favorite has been removed from the list.";
                    string fileNotFound2 = (string)Application.Current.TryFindResource("FileNotFound") ?? "File Not Found";
                    MessageBox.Show($"{thegamefiledoesnotexist2}\n\n{thefavoritehasbeenremovedfromthelist2}",
                        fileNotFound2, MessageBoxButton.OK, MessageBoxImage.Information);
                }

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
            CouldNotLaunchThisGameMessageBox();
        }
    }
        
    private void RemoveFromFavorites(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
            
        PreviewImage.Source = null;
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
            // Notify developer
            string formattedException = $"There was a problem opening the Video Link.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            CouldNotOpenVideoLinkMessageBox();
            void CouldNotOpenVideoLinkMessageBox()
            {
                string therewasaproblemopeningtheVideoLink2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideoLink") ?? "There was a problem opening the Video Link.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{therewasaproblemopeningtheVideoLink2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            // Notify developer
            string formattedException = $"There was a problem opening the Info Link.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            CouldNotOpenInfoLinkMessageBox();
            void CouldNotOpenInfoLinkMessageBox()
            {
                string therewasaproblemopeningtheInfoLink2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheInfoLink") ?? "There was a problem opening the Info Link.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{therewasaproblemopeningtheInfoLink2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
        
    private void OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig)
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
            // Notify developer
            string contextMessage = $"There was a problem opening the History window.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            CouldNotOpenHistoryWindowMessageBox();
            void CouldNotOpenHistoryWindowMessageBox()
            {
                string therewasaproblemopeningtheHistorywindow2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistorywindow") ?? "There was a problem opening the History window.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{therewasaproblemopeningtheHistorywindow2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OpenCover(string systemName, string fileName)
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs?.FirstOrDefault(config =>
                config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (systemConfig == null)
            {
                // Notify developer
                const string formattedException = "systemConfig is null.";
                Exception exception = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                // Notify user
                ErrorOpeningCoverImageMessageBox();
                void ErrorOpeningCoverImageMessageBox()
                {
                    string therewasanerrortryingtoopen2 = (string)Application.Current.TryFindResource("Therewasanerrortryingtoopen") ?? "There was an error trying to open the cover image.";
                    string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                    MessageBox.Show($"{therewasanerrortryingtoopen2}\n\n" +
                                    $"{theerrorwasreportedtothedeveloper2}",
                        error2, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }

            try
            {
                string imagePath = FindCoverImagePath(systemName, fileName, baseDirectory, systemConfig.SystemImageFolder);
                if (!imagePath.EndsWith("default.png"))
                {
                    var imageViewerWindow = new ImageViewerWindow();
                    imageViewerWindow.LoadImage(imagePath);
                    imageViewerWindow.Show();
                }
                else
                {
                    // Notify user
                    ThereIsNoCoverMessageBox();
                    void ThereIsNoCoverMessageBox()
                    {
                        string thereisnocoverassociated2 = (string)Application.Current.TryFindResource("Thereisnocoverassociated") ?? "There is no cover associated with this favorite.";
                        string covernotfound2 = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";
                        MessageBox.Show(thereisnocoverassociated2,
                            covernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (IOException ex)
            {
                // Notify developer
                string formattedException = $"IOException.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                Exception exception = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                // Notify user
                ErrorOpeningCoverImageMessageBox();
                void ErrorOpeningCoverImageMessageBox()
                {
                    string therewasanerrortryingtoopenthe2 = (string)Application.Current.TryFindResource("Therewasanerrortryingtoopenthe") ?? "There was an error trying to open the cover image.";
                    string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                    MessageBox.Show($"{therewasanerrortryingtoopenthe2}\n\n" +
                                    $"{theerrorwasreportedtothedeveloper2}",
                        error2, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"There was an error in the method OpenCover.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Exception exception = new(formattedException);
            Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
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

        // Notify user
        ThereIsNoTitleSnapshotMessageBox();
        void ThereIsNoTitleSnapshotMessageBox()
        {
            string thereisnotitlesnapshot2 = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot2") ?? "There is no title snapshot associated with this favorite.";
            string titleSnapshotnotfound2 = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ?? "Title Snapshot not found";
            MessageBox.Show(thereisnotitlesnapshot2,
                titleSnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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

        // Notify user
        NoGameplaySnapshotMessageBox();
        void NoGameplaySnapshotMessageBox()
        {
            string thereisnogameplaysnapshotassociated2 = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshotassociated") ?? "There is no gameplay snapshot associated with this favorite.";
            string gameplaySnapshotnotfound2 = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";
            MessageBox.Show(thereisnogameplaysnapshotassociated2,
                gameplaySnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
        // Notify user
        ThereIsNoCartMessageBox();
        void ThereIsNoCartMessageBox()
        {
            string thereisnocartassociatedwith2 = (string)Application.Current.TryFindResource("Thereisnocartassociatedwith") ?? "There is no cart associated with this favorite.";
            string cartnotfound2 = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";
            MessageBox.Show(thereisnocartassociatedwith2,
                cartnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
        ThereIsNoVideoFileMessageBox();
        void ThereIsNoVideoFileMessageBox()
        {
            string thereisnovideofileassociated2 = (string)Application.Current.TryFindResource("Thereisnovideofileassociated") ?? "There is no video file associated with this favorite.";
            string videonotfound2 = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";
            MessageBox.Show(thereisnovideofileassociated2,
                videonotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
                    // Notify developer
                    string formattedException = $"Failed to open the manual.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    Exception exception = new(formattedException);
                    Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    CouldNotOpenManualMessageBox();
                    void CouldNotOpenManualMessageBox()
                    {
                        string failedtoopenthemanualfor2 = (string)Application.Current.TryFindResource("Failedtoopenthemanualfor") ?? "Failed to open the manual for this favorite.";
                        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                        MessageBox.Show($"{failedtoopenthemanualfor2}\n\n" +
                                        $"{theerrorwasreportedtothedeveloper2}",
                            error2, MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return;
                }
            }
        }
        
        ThereIsNoManualMessageBox();
        void ThereIsNoManualMessageBox()
        {
            string thereisnomanualassociatedwith2 = (string)Application.Current.TryFindResource("Thereisnomanualassociatedwith") ?? "There is no manual associated with this favorite.";
            string manualnotfound2 = (string)Application.Current.TryFindResource("Manualnotfound") ?? "Manual not found";
            MessageBox.Show(thereisnomanualassociatedwith2,
                manualnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
                    // Notify developer
                    string formattedException = $"Failed to open the walkthrough file.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    CouldNotOpenWalkthroughMessageBox();
                    void CouldNotOpenWalkthroughMessageBox()
                    {
                        string failedtoopenthewalkthroughfile2 = (string)Application.Current.TryFindResource("Failedtoopenthewalkthroughfile") ?? "Failed to open the walkthrough file.";
                        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
                        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                        MessageBox.Show($"{failedtoopenthewalkthroughfile2}\n\n" +
                                        $"{theerrorwasreportedtothedeveloper2}",
                            error2, MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return;
                }
            }
        }

        ThereIsNoWalkthroughMessageBox();
        void ThereIsNoWalkthroughMessageBox()
        {
            string thereisnowalkthroughfileassociated2 = (string)Application.Current.TryFindResource("Thereisnowalkthroughfileassociated") ?? "There is no walkthrough file associated with this favorite.";
            string walkthroughnotfound2 = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";
            MessageBox.Show(thereisnowalkthroughfileassociated2,
                walkthroughnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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

        ThereIsNoCabinetMessageBox();
        void ThereIsNoCabinetMessageBox()
        {
            string thereisnocabinetfileassociated2 = (string)Application.Current.TryFindResource("Thereisnocabinetfileassociated") ?? "There is no cabinet file associated with this favorite.";
            string cabinetnotfound2 = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";
            MessageBox.Show(thereisnocabinetfileassociated2,
                cabinetnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
        ThereIsNoFlyerMessageBox();
        void ThereIsNoFlyerMessageBox()
        {
            string thereisnoflyerfileassociatedwith2 = (string)Application.Current.TryFindResource("Thereisnoflyerfileassociatedwith") ?? "There is no flyer file associated with this favorite.";
            string flyernotfound2 = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";
            MessageBox.Show(thereisnoflyerfileassociatedwith2,
                flyernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
        ThereIsNoPcbMessageBox();
        void ThereIsNoPcbMessageBox()
        {
            string thereisnoPcBfileassociated2 = (string)Application.Current.TryFindResource("ThereisnoPCBfileassociated") ?? "There is no PCB file associated with this favorite.";
            string pCBnotfound2 = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";
            MessageBox.Show(thereisnoPcBfileassociated2,
                pCBnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    private async Task TakeScreenshotOfSelectedWindow(string fileNameWithoutExtension, string systemName)
    {
        try
        {
            // Clear the PreviewImage
            PreviewImage.Source = null;
            
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                string formattedException = "systemConfig is null.";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                // Notify user
                TakeScreenShotErrorMessageBox();

                return;
            }

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
                //MessageBox.Show("No window selected for the screenshot.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                
            // Notify the user
            ScreenshotSavedMessageBox();
            void ScreenshotSavedMessageBox()
            {
                string screenshotsavedsuccessfullyat2 = (string)Application.Current.TryFindResource("Screenshotsavedsuccessfullyat") ?? "Screenshot saved successfully at:";
                string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
                MessageBox.Show($"{screenshotsavedsuccessfullyat2}\n\n" +
                                $"{screenshotPath}",
                    success2, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadFavorites();

        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error in the TakeScreenshotOfSelectedWindow.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            TakeScreenShotErrorMessageBox();
        }
        void TakeScreenShotErrorMessageBox()
        {
            string simpleLaunchercouldnottakethescreenshot2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnottakethescreenshot") ?? "'Simple Launcher' could not take the screenshot.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";
            MessageBox.Show($"{simpleLaunchercouldnottakethescreenshot2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void DeleteFile(string filePath, string fileNameWithExtension)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                    
                PlayClick.PlayTrashSound();

                // Notify user
                FileDeletedMessageBox();
                void FileDeletedMessageBox()
                {
                    string thefile2 = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
                    string hasbeensuccessfullydeleted2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
                    string fileDeleted2 = (string)Application.Current.TryFindResource("FileDeleted") ?? "File Deleted";
                    MessageBox.Show($"{thefile2} \"{fileNameWithExtension}\" {hasbeensuccessfullydeleted2}",
                        fileDeleted2, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                string errorMessage = $"An error occurred while trying to delete the file \"{fileNameWithExtension}\"." +
                                      $"Exception type: {ex.GetType().Name}\n" +
                                      $"Exception details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                CouldNotDeleteTheFileMessageBox();
            }
        }
        else
        {
            // Notify developer
            string errorMessage = "The file could not be found.\n\n" +
                                  $"File: {filePath}";
            Exception ex = new(errorMessage);
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            CouldNotDeleteTheFileMessageBox();
        }
        void CouldNotDeleteTheFileMessageBox()
        {
            string anerroroccurredwhiletrying2 = (string)Application.Current.TryFindResource("Anerroroccurredwhiletrying") ?? "An error occurred while trying to delete the file.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{anerroroccurredwhiletrying2}" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
            CouldNotLaunchThisGameMessageBox();
        }
    }
    
    private void CouldNotLaunchThisGameMessageBox()
    {
        string simpleLaunchercouldnotlaunchthisgame2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunchthisgame") ?? "'Simple Launcher' could not launch this game.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{simpleLaunchercouldnotlaunchthisgame2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
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
                SelectAFavoriteToRemoveMessageBox();
                void SelectAFavoriteToRemoveMessageBox()
                {
                    string pleaseselectafavoritetoremove2 = (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ?? "Please select a favorite to remove.";
                    string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
                    MessageBox.Show(pleaseselectafavoritetoremove2,
                        warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

}