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
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.ViewModel;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

internal class GameButtonFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemManager> systemConfigs,
    List<MameManager> machines,
    SettingsManager settings,
    FavoritesManager favoritesManager,
    WrapPanel gameFileGrid,
    MainWindow mainWindow)
{
    private readonly ComboBox _emulatorComboBox = emulatorComboBox;
    private readonly ComboBox _systemComboBox = systemComboBox;
    private readonly List<SystemManager> _systemConfigs = systemConfigs;
    private readonly List<MameManager> _machines = machines;
    private readonly SettingsManager _settings = settings;
    private readonly FavoritesManager _favoritesManager = favoritesManager;
    private readonly WrapPanel _gameFileGrid = gameFileGrid;
    private readonly MainWindow _mainWindow = mainWindow;
    public int ImageHeight { get; set; } = settings.ThumbnailSize;

    public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemManager systemManager)
    {
        var fileNameWithExtension = Path.GetFileName(filePath);
        var fileNameWithoutExtension = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(filePath));

        // Pass the original filename without extension for image lookup
        var imagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemManager.SystemName, systemManager);

        // Use the new ImageLoader to load the image and get the isDefault flag
        var (loadedImage, isDefaultImage) = await ImageLoader.LoadImageAsync(imagePath);

        // Create the view model and determine the initial favorite state:
        var viewModel = new GameButtonViewModel
        {
            IsFavorite = _favoritesManager.FavoriteList.Any(f =>
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
            Text = fileNameWithoutExtension, // Use TitleCase for display
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 13,
            ToolTip = fileNameWithoutExtension, // Use TitleCase for tooltip
            TextWrapping = TextWrapping.Wrap
        };
        textPanel.Children.Add(filenameTextBlock);

        // For MAME systems, add a second row for the description if available.
        if (systemManager.SystemIsMame)
        {
            // Use original filename without extension for MAME lookup
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
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
        switch (_settings.ButtonAspectRatio)
        {
            case "Wider":
                aspectWidth = 1.5;
                aspectHeight = 1.0;
                break;
            case "SuperWider":
                aspectWidth = 2.0;
                aspectHeight = 1.0;
                break;
            case "Taller":
                aspectWidth = 1.0;
                aspectHeight = 1.3;
                break;
            case "SuperTaller":
                aspectWidth = 1.0;
                aspectHeight = 1.6;
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
            // Notice: NOT setting a fixed Height for the grid
            // so that the text row (Row 1) can expand.
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageAreaHeight) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Source = loadedImage // Assign the loaded image here
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
        // FIX: Use fileNameWithExtension for the key to match the lookup logic
        var key = $"{systemName}|{fileNameWithExtension}";

        // Create the composite tag object
        var tag = new GameButtonTag
        {
            IsDefaultImage = isDefaultImage, // Use the flag returned by ImageLoader
            Key = key
        };

        // Assign it to the button's Tag property
        button.Tag = tag;

        button.Click += async (sender, _) =>
        {
            if (sender is not Button clickedButton) return;

            // Prevent multiple clicks while launching
            if (!clickedButton.IsEnabled) return;

            clickedButton.IsEnabled = false; // Disable the button

            try
            {
                PlayClick.PlayClickSound(); // Play sound *before* launching
                await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
            }
            finally
            {
                // Re-enable the button after the game process exits
                clickedButton.IsEnabled = true;
            }
        };

        return AddRightClickContextMenuGameButtonFactory(filePath, systemName, systemManager, fileNameWithExtension, fileNameWithoutExtension, button);
    }

    private Button AddRightClickContextMenuGameButtonFactory(string filePath, string systemName, SystemManager systemManager, string fileNameWithExtension, string fileNameWithoutExtension, Button button)
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
            await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
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
            // FIX: Pass the correct WrapPanel reference
            RightClickContextMenu.AddToFavorites(systemName, fileNameWithExtension, _favoritesManager, _gameFileGrid, _mainWindow);
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
            PlayClick.PlayTrashSound();
            // FIX: Pass the correct WrapPanel reference
            RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, _favoritesManager, _gameFileGrid, _mainWindow);
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
            RightClickContextMenu.OpenVideoLink(systemName, fileNameWithoutExtension, _machines, _settings);
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
            RightClickContextMenu.OpenInfoLink(systemName, fileNameWithoutExtension, _machines, _settings);
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
            RightClickContextMenu.OpenRomHistoryWindow(systemName, fileNameWithoutExtension, systemManager, _machines);
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
            RightClickContextMenu.OpenCover(systemName, fileNameWithoutExtension, systemManager);
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

            _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemManager, button, _mainWindow);
            await GameLauncher.HandleButtonClick(filePath, _emulatorComboBox, _systemComboBox, _systemConfigs, _settings, _mainWindow);
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

        void DoYouWantToDeleteMessageBox()
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

            RightClickContextMenu.RemoveFromFavorites(systemName, fileNameWithExtension, _favoritesManager, _gameFileGrid, _mainWindow);
        }
    }
}

