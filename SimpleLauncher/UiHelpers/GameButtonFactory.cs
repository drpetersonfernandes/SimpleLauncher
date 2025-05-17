using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.ViewModels;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher.UiHelpers;

public class GameButtonFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemManager> systemManagers,
    List<MameManager> machines,
    SettingsManager settings,
    FavoritesManager favoritesManager,
    WrapPanel gameFileGrid,
    MainWindow mainWindow)
{
    private readonly ComboBox _emulatorComboBox = emulatorComboBox;
    private readonly ComboBox _systemComboBox = systemComboBox;
    private readonly List<SystemManager> _systemManagers = systemManagers;
    private readonly List<MameManager> _machines = machines;
    private readonly SettingsManager _settings = settings;
    private readonly FavoritesManager _favoritesManager = favoritesManager;
    private readonly WrapPanel _gameFileGrid = gameFileGrid;
    private readonly MainWindow _mainWindow = mainWindow;

    private Button _button;
    public int ImageHeight { get; set; } = settings.ThumbnailSize;

    public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemManager systemManager)
    {
        var fileNameWithExtension = PathHelper.GetFileName(filePath);
        var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);
        var selectedSystemName = systemName;
        var selectedSystemManager = systemManager;

        // Pass the original filename without extension for image lookup
        var imagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager);

        // Use the new ImageLoader to load the image and get the isDefault flag
        var (loadedImage, isDefaultImage) = await ImageLoader.LoadImageAsync(imagePath);

        // Create the view model and determine the initial favorite state:
        var viewModel = new GameButtonViewModel
        {
            IsFavorite = _favoritesManager.FavoriteList.Any(f =>
                f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                f.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase))
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
        if (selectedSystemManager.SystemIsMame)
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

        _button = new Button
        {
            Content = grid,
            Width = baseSize,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5),
            Padding = new Thickness(0, 10, 0, 0)
        };

        // Create a unique key for the favorite status
        var key = $"{selectedSystemName}|{fileNameWithExtension}";

        // Create the composite tag object
        var tag = new GameButtonTag
        {
            IsDefaultImage = isDefaultImage, // Use the flag returned by ImageLoader
            Key = key
        };

        // Assign it to the button's Tag property
        _button.Tag = tag;

        _button.Click += async (sender, e) =>
        {
            if (sender is not Button clickedButton) return;

            // Prevent multiple clicks while launching
            if (!clickedButton.IsEnabled) return;

            clickedButton.IsEnabled = false; // Disable the button

            var selectedEmulatorName = _emulatorComboBox.SelectedItem as string; // Update value to get current selected emulator
            if (string.IsNullOrEmpty(selectedEmulatorName))
            {
                // Notify developer
                var ex = new Exception("selectedEmulatorName is null or empty.");
                _ = LogErrors.LogErrorAsync(ex, "selectedEmulatorName is null or empty.");

                // Notify user
                MessageBoxLibrary.EmulatorNameIsRequiredMessageBox();
            }

            try
            {
                PlayClick.PlayNotificationSound();
                await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
            }
            finally
            {
                // Re-enable the button after the game process exits
                clickedButton.IsEnabled = true;
            }
        };

        return ContextMenu.AddRightClickReturnButton(filePath, fileNameWithExtension, fileNameWithoutExtension, selectedSystemName,
            _emulatorComboBox, _favoritesManager, selectedSystemManager, _machines, _settings, _mainWindow, _gameFileGrid, _button);
    }
}

