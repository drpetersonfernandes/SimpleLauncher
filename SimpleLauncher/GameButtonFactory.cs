using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

internal class GameButtonFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemConfig> systemConfigs,
    List<MameConfig> machines,
    SettingsConfig settings,
    FavoritesConfig favoritesConfig,
    WrapPanel gameFileGrid,
    MainWindow mainWindow)
{
    private const string DefaultImagePath = "default.png";
    public int ImageHeight { get; set; } = settings.ThumbnailSize;
    private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private readonly FavoritesManager _favoritesManager = new();

    public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemConfig systemConfig)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string fileNameWithExtension = Path.GetFileName(filePath);
        fileNameWithoutExtension = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

        string imagePath = DetermineImagePath(fileNameWithoutExtension, systemConfig.SystemName, systemConfig);
        bool isDefaultImage = imagePath.EndsWith(DefaultImagePath);

        // Check if the game is a favorite
        var isFavorite = favoritesConfig.FavoriteList.Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                               && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

        var textBlock = new TextBlock
        {
            Text = fileNameWithoutExtension,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 13,
            ToolTip = fileNameWithoutExtension
        };

        if (systemConfig.SystemIsMame)
        {
            var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            if (machine != null)
            {
                var descriptionTextBlock = new TextBlock
                {
                    Text = machine.Description,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    FontSize = 12,
                    ToolTip = machine.Description
                };
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(descriptionTextBlock);
            }
        }

        var grid = new Grid
        {
            Width = ImageHeight + 50,
            Height = ImageHeight + 50
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = ImageHeight + 50,
            Height = ImageHeight + 50,
            MaxHeight = ImageHeight + 50
        };

        grid.Children.Add(stackPanel);

        var button = new Button
        {
            Content = grid,
            Width = ImageHeight + 50,
            Height = ImageHeight + 50,
            MaxHeight = ImageHeight + 50,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5),
            Padding = new Thickness(0,10,0,0)
        };

        var image = new Image
        {
            Height = ImageHeight,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        await LoadImageAsync(image, button, imagePath);

        if (isFavorite)
        {
            var startImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/star.png")),
                Width = 22,
                Height = 22,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5)
            };
            grid.Children.Add(startImage);
        }

        button.PreviewMouseLeftButtonDown += (_, args) =>
        {
            if (args.OriginalSource is Image img && (img.Name == "videoIcon" || img.Name == "infoIcon"))
            {
                args.Handled = true;
            }
        };

        stackPanel.Children.Add(image);
        stackPanel.Children.Add(textBlock);

        if (isDefaultImage)
        {
            button.Tag = "DefaultImage";
        }

        button.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            await GameLauncher.HandleButtonClick(filePath, emulatorComboBox, systemComboBox, systemConfigs, settings, mainWindow);
        };
        
        // Right click context menu
        return AddRightClickContextMenu();

        Button AddRightClickContextMenu()
        {
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
                RightClickContextMenu.AddToFavorites(systemName, fileNameWithExtension, _favoritesManager, gameFileGrid, mainWindow);
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
                RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, _favoritesManager, gameFileGrid, mainWindow);
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
                RightClickContextMenu.OpenVideoLink(systemName, fileNameWithoutExtension, machines, settings);
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
                RightClickContextMenu.OpenInfoLink(systemName, fileNameWithoutExtension, machines, settings);
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
                RightClickContextMenu.OpenHistoryWindow(systemName, fileNameWithoutExtension, systemConfig, machines);
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
                RightClickContextMenu.OpenCover(systemName, fileNameWithoutExtension, systemConfig);
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
                RightClickContextMenu.OpenTitleSnapshot(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenGameplaySnapshot(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenCart(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.PlayVideo(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenManual(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenWalkthrough(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenCabinet(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenFlyer(systemName, fileNameWithoutExtension);
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
                RightClickContextMenu.OpenPcb(systemName, fileNameWithoutExtension);
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
            
                // Notify user
                MessageBoxLibrary.TakeScreenShotMessageBox();
                
                _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig, button, mainWindow);
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
            
                DoYouWantToDeleteMessageBox();
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
            button.ContextMenu = contextMenu;
            return button;
        }

        void DoYouWantToDeleteMessageBox()
        {
            var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, button, gameFileGrid, mainWindow);
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
                RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, _favoritesManager, gameFileGrid, mainWindow);
            }
        }
    }
    
    private string DetermineImagePath(string fileNameWithoutExtension, string systemName, SystemConfig systemConfig)
    {
        string baseImageDirectory;
        if (string.IsNullOrEmpty(systemConfig?.SystemImageFolder))
        {
            baseImageDirectory = Path.Combine(_baseDirectory, "images", systemName);
        }
        else
        {
            baseImageDirectory = Path.IsPathRooted(systemConfig.SystemImageFolder)
                ? systemConfig.SystemImageFolder // If already absolute
                : Path.Combine(_baseDirectory, systemConfig.SystemImageFolder); // Make it absolute
        }

        // Extensions to check
        string[] extensions = [".png", ".jpg", ".jpeg"];

        // Check each extension for a valid image file
        foreach (var ext in extensions)
        {
            string imagePath = Path.Combine(baseImageDirectory, $"{fileNameWithoutExtension}{ext}");
            if (File.Exists(imagePath))
                return imagePath;
        }

        // Try to find default.png in the SystemImageFolder if specified, otherwise use the global default
        string defaultImagePath = Path.Combine(baseImageDirectory, "default.png");
        if (File.Exists(defaultImagePath))
        {
            return defaultImagePath;
        }

        // Fall back to the global default image path if no specific or system default image exists
        return Path.Combine(_baseDirectory, "images", DefaultImagePath);
    }

    public static async Task LoadImageAsync(Image imageControl, Button button, string imagePath)
    {
        string imageFileName = Path.GetFileName(imagePath);
        
        ArgumentNullException.ThrowIfNull(imageControl);

        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException(@"Invalid image path.", nameof(imagePath));
        try
        {
            BitmapImage bitmapImage = null;

            await Task.Run(() =>
            {
                // Read the image into a memory stream to prevent file locks
                byte[] imageData = File.ReadAllBytes(imagePath);

                using var ms = new MemoryStream(imageData);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad; // Ensures the image is loaded into memory
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze(); // Makes the image thread-safe
                bitmapImage = bi;
            });

            // Assign the loaded image to the image control on the UI thread
            imageControl.Dispatcher.Invoke(() => imageControl.Source = bitmapImage);
        }
        catch (Exception)
        {
            // If an exception occurs (e.g., the image is corrupt), will load the default image
            // This uses the dispatcher to ensure UI elements are accessed on the UI thread
            imageControl.Dispatcher.Invoke(() => LoadFallbackImage(imageControl, button, DefaultImagePath));
            
            // Notify user
            MessageBoxLibrary.UnableToLoadImageMessageBox(imageFileName);
        }
    }

    private static void LoadFallbackImage(Image imageControl, Button button, string defaultImagePath)
    {
        string fallbackImagePath = defaultImagePath;

        // If the specific default image doesn't exist, try the global default image
        if (!File.Exists(fallbackImagePath))
        {
            fallbackImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", DefaultImagePath);
        }

        if (File.Exists(fallbackImagePath))
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(fallbackImagePath);
                using MemoryStream ms = new MemoryStream(imageData);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                imageControl.Source = bitmapImage; // Assign the fallback image
                button.Tag = "DefaultImage"; // Tagging the button to indicate a default image is used
            }
            catch (Exception ex)
            {
                // Notify developer
                var formattedException = $"Fail to load the fallback image in the method LoadFallbackImage.\n\n" +
                                         $"Exception type: {ex.GetType().Name}\n" +
                                         $"Exception details: {ex.Message}";
                LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));
            }
        }
        else
        {
            // Notify user
            // If even the global default image is not found, ask user to reinstall 'Simple Launcher'
            MessageBoxLibrary.DefaultImageNotFoundMessageBox();
        }
    }
}