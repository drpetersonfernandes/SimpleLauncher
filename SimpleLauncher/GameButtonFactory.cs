using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

internal class GameButtonFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemConfig> systemConfigs,
    List<MameConfig> machines,
    SettingsConfig settings,
    FavoritesManager favoritesManager,
    WrapPanel gameFileGrid,
    MainWindow mainWindow)
{
    private const string DefaultImagePath = "default.png";
    public int ImageHeight { get; set; } = settings.ThumbnailSize;
    private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemConfig systemConfig)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var fileNameWithExtension = Path.GetFileName(filePath);
        fileNameWithoutExtension = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);

        var imagePath = DetermineImagePath(fileNameWithoutExtension, systemConfig.SystemName, systemConfig);
        // Determine if it's a default image (isDefaultImage is a bool)
        var isDefaultImage = imagePath.EndsWith(DefaultImagePath);

        // Create the view model and determine the initial favorite state:
        var viewModel = new GameButtonViewModel
        {
            IsFavorite = favoritesManager.FavoriteList.Any(f =>
                f.FileName.Equals(Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase) &&
                f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase))
        };

        // Create a container for text that will hold two rows
        var textPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Always show the filename on the first row.
        var filenameTextBlock = new TextBlock
        {
            Text = fileNameWithoutExtension,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 13,
            ToolTip = fileNameWithoutExtension,
            TextWrapping = TextWrapping.Wrap
        };
        textPanel.Children.Add(filenameTextBlock);

        // For MAME systems, add a second row for the description if available.
        if (systemConfig.SystemIsMame)
        {
            var machine = machines.FirstOrDefault(
                m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
            {
                var descriptionTextBlock = new TextBlock
                {
                    Text = machine.Description,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Normal,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    FontSize = 11,
                    ToolTip = machine.Description,
                    TextWrapping = TextWrapping.Wrap
                };
                textPanel.Children.Add(descriptionTextBlock);
            }
        }

        // Calculate dimensions based on the user-selected aspect ratio
        // Base size is determined from ImageHeight (plus some padding)
        double baseSize = ImageHeight + 50;
        double aspectWidth;
        double aspectHeight;

        // Use the ButtonAspectRatio value from settings:
        switch (settings.ButtonAspectRatio)
        {
            case "Wider":
                aspectWidth = 1.5;
                aspectHeight = 1.0;
                break;
            case "Taller":
                aspectWidth = 1.0;
                aspectHeight = 1.3;
                break;
            default: // "Square" or any unrecognized value
                aspectWidth = 1.1;
                aspectHeight = 1.0;
                break;
        }

        // Calculate the height for the image area only based on the aspect ratio.
        var imageAreaHeight = baseSize * (aspectHeight / aspectWidth);

        // Create a grid with two rows:
        // Row 0: fixed height for the image container.
        // Row 1: auto-sized for the text.
        var grid = new Grid
        {
            Width = baseSize
            // Notice: NOT setting a fixed Height for the grid,
            // so that the text row (Row 1) can expand.
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageAreaHeight) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Wrap the image in a Border that fixes the image area size.
        var imageContainer = new Border
        {
            Width = baseSize,
            Height = imageAreaHeight,
            Child = image
        };
        Grid.SetRow(imageContainer, 0);
        grid.Children.Add(imageContainer);

        await LoadImageAsync(image, null, imagePath);

        // If the game is a favorite, add a star overlay.
        // Create the star overlay image.
        var starImage = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/star.png")),
            Width = 22,
            Height = 22,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(5)
        };
        // Bind its Visibility to IsFavorite
        var binding = new Binding("IsFavorite")
        {
            Converter = new BooleanToVisibilityConverter()
        };
        starImage.SetBinding(UIElement.VisibilityProperty, binding);
        // Add the star overlay to the grid.
        grid.Children.Add(starImage);

        // Set the DataContext of the grid to the view model.
        grid.DataContext = viewModel;

        // Create a container for the text.
        var textContainer = new Border
        {
            Child = textPanel,
            Padding = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(textContainer, 1);
        grid.Children.Add(textContainer);

        var button = new Button
        {
            Content = grid,
            Width = baseSize,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5),
            Padding = new Thickness(0, 10, 0, 0)
        };

        // Create a unique key for the favorite status
        var key = $"{systemName}|{Path.GetFileNameWithoutExtension(filePath)}";

        // Create the composite tag object
        var tag = new GameButtonTag
        {
            IsDefaultImage = isDefaultImage,
            Key = key
        };

        // Assign it to the button's Tag property
        button.Tag = tag;

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
                RightClickContextMenu.AddToFavorites(systemName, fileNameWithExtension, favoritesManager, gameFileGrid, mainWindow);
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
                RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, favoritesManager, gameFileGrid, mainWindow);
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
            var deleteGame2 = (string)Application.Current.TryFindResource("DeleteGame") ?? "Delete Game";
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

            if (result != MessageBoxResult.Yes) return;
            try
            {
                RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, button, gameFileGrid, mainWindow);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error deleting the file.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
            }

            RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, favoritesManager, gameFileGrid, mainWindow);
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
            var imagePath = Path.Combine(baseImageDirectory, $"{fileNameWithoutExtension}{ext}");
            if (File.Exists(imagePath))
                return imagePath;
        }

        // Try to find default.png in the SystemImageFolder if specified, otherwise use the global default
        var defaultImagePath = Path.Combine(baseImageDirectory, "default.png");

        // Fall back to the global default image path if no specific or system default image exists
        return File.Exists(defaultImagePath) ? defaultImagePath : Path.Combine(_baseDirectory, "images", DefaultImagePath);
    }

    public static async Task LoadImageAsync(Image imageControl, Button button, string imagePath)
    {
        var imageFileName = Path.GetFileName(imagePath);

        ArgumentNullException.ThrowIfNull(imageControl);

        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException(@"Invalid image path.", nameof(imagePath));
        try
        {
            BitmapImage bitmapImage = null;

            await Task.Run(() =>
            {
                // Read the image into a memory stream to prevent file locks
                var imageData = File.ReadAllBytes(imagePath);

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
        var fallbackImagePath = defaultImagePath;

        // If the specific default image doesn't exist, try the global default image
        if (!File.Exists(fallbackImagePath))
        {
            fallbackImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", DefaultImagePath);
        }

        if (File.Exists(fallbackImagePath))
        {
            try
            {
                var imageData = File.ReadAllBytes(fallbackImagePath);
                using var ms = new MemoryStream(imageData);
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
                const string contextMessage = "Fail to load the fallback image in the method LoadFallbackImage.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
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