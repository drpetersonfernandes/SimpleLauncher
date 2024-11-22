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
                
            PlayClick.PlayClickSound();
            PreviewImage.Source = null;
        }
        else
        {
            MessageBox.Show("Please select a favorite to remove.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    string formattedException = $"There was an error in the FavoritesDataGrid_MouseRightButtonUp method.\n\n" +
                                                $"No FileName found for the selected favorite.";
                    Exception ex = new(formattedException);
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                   
                    MessageBox.Show("There was an error loading the system configuration for this favorite.\n\n" +
                                    "The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    return;
                }
                
                string fileNameWithExtension = selectedFavorite.FileName;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);
                
                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
                if (systemConfig == null)
                {
                    string formattedException = $"There was an error in the FavoritesDataGrid_MouseRightButtonUp method.\n\n" +
                                                $"No system configuration found for the selected favorite.";
                    Exception ex = new(formattedException);
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                   
                    MessageBox.Show("There was an error loading the system configuration for this favorite.\n\n" +
                                    "The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
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
                    OpenRomHistoryWindow(selectedFavorite.SystemName, fileNameWithoutExtension, systemConfig);
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
            
                // Take Screenshot Context Menu
                var takeScreenshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
                    Width = 16,
                    Height = 16
                };
                var takeScreenshot = new MenuItem
                {
                    Header = "Take Screenshot",
                    Icon = takeScreenshotIcon
                };
                takeScreenshot.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    MessageBox.Show(
                        "The game will launch now.\n\n" +
                        "Set the game window to non-fullscreen. This is important.\n\n" +
                        "You should change the emulator parameters to prevent the emulator from starting in fullscreen.\n\n" +
                        "A selection window will open in 'Simple Launcher,' allowing you to choose the desired window to capture.\n\n" +
                        "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.",
                        "Take Screenshot", MessageBoxButton.OK, MessageBoxImage.Information);
                
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
                var deleteGame = new MenuItem
                {
                    Header = "Delete Game",
                    Icon = deleteGameIcon
                };
                deleteGame.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    var result = MessageBox.Show($"Are you sure you want to delete the file \"{fileNameWithExtension}\"?\n\n" +
                                                 $"This action will delete the file from the HDD and cannot be undone.",
                        "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        DeleteFile(filePath, fileNameWithExtension);
                        RemoveFromFavorites(selectedFavorite);
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
            string formattedException =
                $"There was an error in the right-click context menu in the FavoritesDataGrid_MouseRightButtonUp method.\n\n" +
                $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show("There was an error in the right-click context menu.\n\n" +
                            "The error was reported to the developer, who will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Please select a game to launch.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"Error in the LaunchGame_Click method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);            }
    }

    private async Task LaunchGameFromFavorite(string fileName, string systemName)
    {
        try
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {

                string formattedException = $"There was an error in the Favorites window.\n\n" +
                                            $"No system configuration found for the selected favorite.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                   
                MessageBox.Show("There was an error loading the system configuration for this favorite.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
            if (emulatorConfig == null)
            {
                string formattedException = $"There was an error in the Favorites window.\n\n" +
                                            $"No emulator configuration found for the selected favorite.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                    
                MessageBox.Show("No emulator configuration found for the selected favorite.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string fullPath = GetFullPath(Path.Combine(systemConfig.SystemFolder, fileName));
                
            // Check if the file exists
            if (!File.Exists(fullPath))
            {
                string formattedException = $"There was an error in the Favorites window.\n\n" +
                                            $"The favorite file does not exist.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                    
                // Remove the favorite from the list since the file no longer exists
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
                if (favoriteToRemove != null)
                {
                    _favoriteList.Remove(favoriteToRemove);
                    _favoritesManager.SaveFavorites(new FavoritesConfig { FavoriteList = _favoriteList });
                }
                    
                MessageBox.Show("The game file does not exist!\n\n" +
                                "The favorite has been removed from the list.",
                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
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
            string formattedException = $"There was an error launching the game from Favorites.\n\n" +
                                        $"File Path: {fileName}\nSystem Name: {systemName}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            MessageBox.Show($"There was an error launching the game from Favorites.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
            string formattedException = $"There was a problem opening the Video Link.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"There was a problem opening the Video Link.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            string formattedException = $"There was a problem opening the Info Link.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"There was a problem opening the Info Link.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            string contextMessage = $"There was a problem opening the History window.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"There was a problem opening the History window.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            MessageBox.Show("There was an error trying to open the cover image.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string systemImageFolder = systemConfig.SystemImageFolder;
        string globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // Function to search for the image file
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

        // First try to find the image in the systemImageFolder directory
        // Then try to find in the globalImageDirectory
        if (TryFindImage(systemImageFolder, out string foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(foundImagePath);
            imageViewerWindow.Show();
        }
        // If not found, try the global directory
        else if (TryFindImage(globalImageDirectory, out foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(foundImagePath);
            imageViewerWindow.Show();
        }
        else
        {
            MessageBox.Show("There is no cover associated with this favorite.",
                "Cover not found", MessageBoxButton.OK, MessageBoxImage.Information);
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

        MessageBox.Show("There is no title snapshot associated with this favorite.",
            "Title Snapshot not found", MessageBoxButton.OK, MessageBoxImage.Information);
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

        MessageBox.Show("There is no gameplay snapshot associated with this favorite.",
            "Gameplay Snapshot not found", MessageBoxButton.OK, MessageBoxImage.Information);
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
        MessageBox.Show("There is no cart associated with this favorite.",
            "Cart not found", MessageBoxButton.OK, MessageBoxImage.Information);
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

        MessageBox.Show("There is no video file associated with this favorite.",
            "Video not found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    string formattedException = $"Failed to open the manual in the Favorites window\n\n" +
                                                $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                    Exception exception = new(formattedException);
                    Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                        
                    MessageBox.Show($"Failed to open the manual for this favorite.\n\n" +
                                    $"The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
        MessageBox.Show("There is no manual associated with this favorite.",
            "Manual not found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    string formattedException = $"Failed to open the walkthrough file in the Favorites window\n\n" +
                                                $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                        
                    MessageBox.Show($"Failed to open the walkthrough file.\n\n" +
                                    $"The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        MessageBox.Show("There is no walkthrough file associated with this favorite.",
            "Walkthrough not found", MessageBoxButton.OK, MessageBoxImage.Information);
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

        MessageBox.Show("There is no cabinet file associated with this favorite.",
            "Cabinet not found", MessageBoxButton.OK, MessageBoxImage.Information);
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
        MessageBox.Show("There is no flyer file associated with this favorite.",
            "Flyer not found", MessageBoxButton.OK, MessageBoxImage.Information);
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
        MessageBox.Show("There is no PCB file associated with this favorite.",
            "PCB not found", MessageBoxButton.OK, MessageBoxImage.Information);
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
                string formattedException = $"There was a problem getting the system configuration for the selected favorite in the Favorites window.";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                MessageBox.Show("There was an error trying to get the systemConfig in the TakeScreenshotOfSelectedWindow method.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
            // Show the flash effect
            var flashWindow = new FlashOverlayWindow();
            await flashWindow.ShowFlashAsync();
                
            // Notify the user of success
            MessageBox.Show($"Screenshot saved successfully at:\n{screenshotPath}",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadFavorites();

        }
        catch (Exception ex)
        {
            // Handle any errors
            MessageBox.Show($"Failed to save screenshot.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Send log to the developer
            string formattedException = $"There was an error in the TakeScreenshotOfSelectedWindow method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
        
    private void DeleteFile(string filePath, string fileNameWithExtension)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                    
                PlayClick.PlayTrashSound();
                
                MessageBox.Show($"The file \"{fileNameWithExtension}\" has been successfully deleted.",
                    "File Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"The file \"{fileNameWithExtension}\" could not be found.",
                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while trying to delete the file \"{fileNameWithExtension}\"." +
                            $"The error was reported to developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Notify developer
            string errorMessage = $"An error occurred while trying to delete the file \"{fileNameWithExtension}\"." +
                                  $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
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
            string formattedException = $"There was an error trying to launch a favorite using the MouseDoubleClick method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            MessageBox.Show($"There was an error trying to launch this favorite.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("Please select a favorite to remove.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
}