using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
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
    private readonly MainWindow _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow)); // Add null-check

    private Button _button;
    public int ImageHeight { get; set; } = settings.ThumbnailSize;

    public async Task<Button> CreateGameButtonAsync(string filePath, string systemName, SystemManager systemManager)
    {
        var absoluteFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);
        var fileNameWithExtension = PathHelper.GetFileName(absoluteFilePath);
        var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(absoluteFilePath);
        var selectedSystemName = systemName;
        var selectedSystemManager = systemManager ?? throw new ArgumentNullException(nameof(systemManager));

        var imagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager, _settings);
        var (loadedImage, isDefaultImage) = await ImageLoader.LoadImageAsync(imagePath);

        // Create the view model and determine the initial favorite state:
        var viewModel = new GameButtonViewModel
        {
            IsFavorite = _favoritesManager.FavoriteList.Any(f =>
                f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                f.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase))
        };

        // Placeholder logic for achievements
        viewModel.HasAchievements = true;

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

        // Determine accessible name for the main game button
        var accessibleGameName = fileNameWithoutExtension;

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
                accessibleGameName = machine.Description; // Use description for accessible name if available
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
        grid.Children.Add(starImage); // Add the star overlay to the grid.

        var context = new RightClickContext(
            absoluteFilePath,
            fileNameWithExtension,
            fileNameWithoutExtension,
            selectedSystemName,
            selectedSystemManager,
            _machines,
            _favoritesManager,
            _settings,
            _emulatorComboBox,
            null,
            null,
            _gameFileGrid,
            null,
            _mainWindow
        );

        const double overlayButtonWidth = 22;
        const double overlayButtonHeight = 22;
        const double overlayButtonSpacing = 5; // Vertical spacing between buttons
        double currentVerticalOffset = 5; // Initial top margin for the first button

        if (_settings.OverlayRetroAchievementButton == true)
        {
            // Add the RetroAchievements trophy icon overlay
            var trophyButton = new Button
            {
                Width = overlayButtonWidth,
                Height = overlayButtonHeight,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, currentVerticalOffset, 5, 0), // Use dynamic offset
                Cursor = Cursors.Hand,
                ToolTip = (string)Application.Current.TryFindResource("ViewAchievements") ?? "View Achievements", // Localized ToolTip
                Style = (Style)Application.Current.FindResource("MahApps.Styles.Button.Chromeless")
            };
            // Set AutomationProperties.Name for screen readers
            AutomationProperties.SetName(trophyButton, (string)trophyButton.ToolTip);

            var trophyImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/trophy.png")),
                Stretch = Stretch.Uniform
            };
            trophyButton.Content = trophyImage;

            trophyButton.Click += async (s, e) =>
            {
                try
                {
                    // Prevent the main button's click event from firing
                    e.Handled = true;

                    PlaySoundEffects.PlayNotificationSound();

                    // Show loading indicator immediately by setting the property (UI updates via binding)
                    if (context.MainWindow != null)
                    {
                        context.MainWindow.IsLoadingGames = true;
                    }

                    try
                    {
                        await ContextMenuFunctions.OpenRetroAchievementsWindowAsync(absoluteFilePath, fileNameWithoutExtension, selectedSystemManager, _mainWindow);
                    }
                    catch (Exception ex)
                    {
                        // Hide loading indicator on error
                        if (context.MainWindow != null)
                        {
                            context.MainWindow.IsLoadingGames = false;
                        }

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ex, $"Error opening achievements for {fileNameWithoutExtension}");

                        // Notify user
                        MessageBoxLibrary.CouldNotOpenAchievementsWindowMessageBox();
                    }
                    finally
                    {
                        // Ensure loading indicator is hidden after async work (success or error)
                        if (context.MainWindow != null)
                        {
                            context.MainWindow.IsLoadingGames = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, "Error opening Retro Achievements Window.");
                    DebugLogger.Log($"Error opening Retro Achievements Window: {ex.Message}");
                }
            };

            grid.Children.Add(trophyButton);
            currentVerticalOffset += overlayButtonHeight + overlayButtonSpacing; // Update offset for next button
        }

        if (_settings.OverlayOpenVideoButton == true)
        {
            // Add the Video Link icon overlay
            var videoLinkButton = new Button
            {
                Width = overlayButtonWidth,
                Height = overlayButtonHeight,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, currentVerticalOffset, 5, 0), // Use dynamic offset
                Cursor = Cursors.Hand,
                ToolTip = (string)Application.Current.TryFindResource("ViewVideo") ?? "View Video", // Localized ToolTip
                Style = (Style)Application.Current.FindResource("MahApps.Styles.Button.Chromeless")
            };
            // Set AutomationProperties.Name for screen readers
            AutomationProperties.SetName(videoLinkButton, (string)videoLinkButton.ToolTip);

            var videoLinkImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
                Stretch = Stretch.Uniform
            };
            videoLinkButton.Content = videoLinkImage;

            videoLinkButton.Click += (s, e) =>
            {
                try
                {
                    // Prevent the main button's click event from firing
                    e.Handled = true;

                    PlaySoundEffects.PlayNotificationSound();

                    // Show loading indicator immediately by setting the property (UI updates via binding)
                    if (context.MainWindow != null)
                    {
                        context.MainWindow.IsLoadingGames = true;
                    }

                    try
                    {
                        ContextMenuFunctions.OpenVideoLink(selectedSystemName, fileNameWithoutExtension, _machines, _settings);
                    }
                    catch (Exception ex)
                    {
                        // Hide loading indicator on error
                        if (context.MainWindow != null)
                        {
                            context.MainWindow.IsLoadingGames = false;
                        }

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ex, $"Error opening video link for {fileNameWithoutExtension}");

                        // Notify user
                        MessageBoxLibrary.ErrorOpeningVideoLinkMessageBox();
                    }
                    finally
                    {
                        // Ensure loading indicator is hidden after async work (success or error)
                        if (context.MainWindow != null)
                        {
                            context.MainWindow.IsLoadingGames = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, "Error opening the video Link.");
                    DebugLogger.Log($"Error opening the video link: {ex.Message}");
                }
            };

            grid.Children.Add(videoLinkButton);
            currentVerticalOffset += overlayButtonHeight + overlayButtonSpacing; // Update offset for next button
        }

        if (_settings.OverlayOpenInfoButton == true)
        {
            // Add the Info Link icon overlay
            var infoLinkButton = new Button
            {
                Width = overlayButtonWidth,
                Height = overlayButtonHeight,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, currentVerticalOffset, 5, 0), // Use dynamic offset
                Cursor = Cursors.Hand,
                ToolTip = (string)Application.Current.TryFindResource("ViewInfo") ?? "View Info", // Localized ToolTip
                Style = (Style)Application.Current.FindResource("MahApps.Styles.Button.Chromeless")
            };
            // Set AutomationProperties.Name for screen readers
            AutomationProperties.SetName(infoLinkButton, (string)infoLinkButton.ToolTip);

            var infoLinkImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
                Stretch = Stretch.Uniform
            };
            infoLinkButton.Content = infoLinkImage;

            infoLinkButton.Click += (s, e) =>
            {
                try
                {
                    // Prevent the main button's click event from firing
                    e.Handled = true;

                    PlaySoundEffects.PlayNotificationSound();

                    // Show loading indicator immediately by setting the property (UI updates via binding)
                    if (context.MainWindow != null)
                    {
                        context.MainWindow.IsLoadingGames = true;
                    }

                    try
                    {
                        ContextMenuFunctions.OpenInfoLink(selectedSystemName, fileNameWithoutExtension, _machines, _settings);
                    }
                    catch (Exception ex)
                    {
                        // Hide loading indicator on error
                        if (context.MainWindow != null)
                        {
                            context.MainWindow.IsLoadingGames = false;
                        }

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ex, $"Error opening info link for {fileNameWithoutExtension}");

                        // Notify user
                        MessageBoxLibrary.ProblemOpeningInfoLinkMessageBox();
                    }
                    finally
                    {
                        // Ensure loading indicator is hidden after async work (success or error)
                        if (context.MainWindow != null)
                        {
                            context.MainWindow.IsLoadingGames = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, "Error opening the info Link.");
                    DebugLogger.Log($"Error opening the info link: {ex.Message}");
                }
            };

            grid.Children.Add(infoLinkButton);
            // No need to update currentVerticalOffset here as it's the last button.
        }

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
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5),
            Padding = new Thickness(0, 5, 0, 0)
        };

        // Set AutomationProperties.Name for the main game button for screen readers
        AutomationProperties.SetName(_button, accessibleGameName);
        AutomationProperties.SetHelpText(_button, (string)Application.Current.TryFindResource("LaunchGame") ?? "Launch Game");

        // Apply the 3D style from MainWindow's resources
        _button.SetResourceReference(FrameworkElement.StyleProperty, "GameButtonStyle");

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

        // Assign click handler AFTER context is created ***
        // Lambda can safely capture 'context'.
        _button.Click += async (sender, e) =>
        {
            try
            {
                if (sender is not Button clickedButton) return;

                // Prevent multiple clicks while launching
                if (!clickedButton.IsEnabled) return;

                _mainWindow.SetGameButtonsEnabled(false); // Disable all game buttons

                var selectedEmulatorName = _emulatorComboBox.SelectedItem as string; // Update value to get current selected emulator
                if (string.IsNullOrEmpty(selectedEmulatorName))
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(null, "selectedEmulatorName is null or empty.");

                    // Notify user
                    MessageBoxLibrary.EmulatorNameIsRequiredMessageBox();

                    _mainWindow.SetGameButtonsEnabled(true); // Re-enable buttons on error
                    return;
                }

                try
                {
                    PlaySoundEffects.PlayNotificationSound();
                    await GameLauncher.HandleButtonClickAsync(absoluteFilePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
                }
                finally
                {
                    _mainWindow.SetGameButtonsEnabled(true); // Re-enable all game buttons
                }
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, "Error launching the game.");
                DebugLogger.Log($"Error launching the game: {ex.Message}");
            }
        };

        context.Button = _button;

        return ContextMenu.AddRightClickReturnButton(context);
    }
}