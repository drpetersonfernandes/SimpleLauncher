using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

        await LoadImageAsync(image, button, imagePath, DefaultImagePath);

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

        // Context menu
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
            AddToFavorites(systemName, fileNameWithExtension);
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
            RemoveFromFavorites(systemName, fileNameWithExtension);
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
            OpenVideoLink(systemName, fileNameWithoutExtension);
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
            OpenInfoLink(systemName, fileNameWithoutExtension);
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
            OpenHistoryWindow(systemName, fileNameWithoutExtension, systemConfig);
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
            OpenCover(systemName, fileNameWithoutExtension, systemConfig);
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
            OpenTitleSnapshot(systemName, fileNameWithoutExtension);
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
            OpenGameplaySnapshot(systemName, fileNameWithoutExtension);
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
            OpenCart(systemName, fileNameWithoutExtension);
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
            PlayVideo(systemName, fileNameWithoutExtension);
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
            OpenManual(systemName, fileNameWithoutExtension);
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
            OpenWalkthrough(systemName, fileNameWithoutExtension);
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
            OpenCabinet(systemName, fileNameWithoutExtension);
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
            OpenFlyer(systemName, fileNameWithoutExtension);
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
            OpenPcb(systemName, fileNameWithoutExtension);
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
                
            _ = TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig, button);
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
            void DoYouWantToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DeleteFile(filePath, fileNameWithExtension, button);
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        string formattedException = $"Error deleting the file.\n\n" +
                                                    $"Exception type: {ex.GetType().Name}\n" +
                                                    $"Exception details: {ex.Message}";
                        Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                                
                        // Notify user
                        MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                    }
                    RemoveFromFavorites2(systemName, fileNameWithExtension);
                }
            }
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

    private void AddToFavorites(string systemName, string fileNameWithExtension)
    {
        try
        {
            // Load existing favorites
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            // Add the new favorite if it doesn't already exist
            if (!favorites.FavoriteList.Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                 && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase)))
            {
                favorites.FavoriteList.Add(new Favorite
                {
                    FileName = fileNameWithExtension, // Use the file name with an extension
                    SystemName = systemName
                });

                // Save the updated favorites list
                _favoritesManager.SaveFavorites(favorites);

                // Update the button's content to add the favorite icon dynamically
                var button = gameFileGrid.Children.OfType<Button>()
                    .FirstOrDefault(b => ((TextBlock)((StackPanel)((Grid)b.Content).Children[0]).Children[1]).Text.Equals(Path.GetFileNameWithoutExtension(fileNameWithExtension), StringComparison.OrdinalIgnoreCase));

                if (button != null)
                {
                    var grid = (Grid)button.Content;
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
            }
            else
            {
                // Notify user
                MessageBoxLibrary.GameIsAlreadyInFavoritesMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while adding a game to the favorites.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
        }
    }
        
    private void RemoveFromFavorites(string systemName, string fileNameWithExtension)
    {
        try
        {
            // Load existing favorites
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            // Find the favorite to remove
            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);

                // Save the updated favorites list
                _favoritesManager.SaveFavorites(favorites);

                // Update the button's content to remove the favorite icon dynamically
                var button = gameFileGrid.Children.OfType<Button>()
                    .FirstOrDefault(b => ((TextBlock)((StackPanel)((Grid)b.Content).Children[0]).Children[1]).Text.Equals(Path.GetFileNameWithoutExtension(fileNameWithExtension), StringComparison.OrdinalIgnoreCase));

                if (button != null)
                {
                    var grid = (Grid)button.Content;
                    var favoriteIcon = grid.Children.OfType<Image>().FirstOrDefault(img => img.Source.ToString().Contains("star.png"));
                    if (favoriteIcon != null)
                    {
                        grid.Children.Remove(favoriteIcon);
                    }
                }
            }
            else
            {
                // Notify user
                MessageBoxLibrary.FileIsNotInFavoritesMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while removing a game from favorites.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }
    
    private void RemoveFromFavorites2(string systemName, string fileNameWithExtension)
    {
        try
        {
            // Load existing favorites
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            // Find the favorite to remove
            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);
                
                // Save the updated favorites list
                _favoritesManager.SaveFavorites(favorites);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error in the method RemoveFromFavorites2.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }

    private void OpenVideoLink(string systemName, string fileNameWithoutExtension)
    {
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        string searchUrl = $"{settings.VideoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
            string contextMessage = $"There was a problem opening the Video Link.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorOpeningVideoLinkMessageBox();
        }
    }

    private void OpenInfoLink(string systemName, string fileNameWithoutExtension)
    {
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        string searchUrl = $"{settings.InfoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
            string contextMessage = $"There was a problem opening the Info Link.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ProblemOpeningInfoLinkMessageBox();
        }
    }
        
    private void OpenHistoryWindow(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig)
    {
        string romName = fileNameWithoutExtension.ToLowerInvariant();
           
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
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
            MessageBoxLibrary.ProblemOpeningHistoryWindowMessageBox();
        }
    }

    private void OpenCover(string systemName, string fileName, SystemConfig systemConfig)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string systemImageFolder = systemConfig.SystemImageFolder;
        
        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }
        
        string globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        // Image extensions to look for
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // Try to find the image in the systemImageFolder directory first
        // Then search inside the globalImageDirectory
        if (TryFindImage(systemImageFolder, out string foundImagePath) || TryFindImage(globalImageDirectory, out foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(foundImagePath);
            imageViewerWindow.Show();
        }
        else
        {
            // Notify user
            MessageBoxLibrary.ThereIsNoCoverMessageBox();
        }

        return;

        // Function to search for the file in a given directory
        bool TryFindImage(string directory, out string foundPath)
        {
            foreach (var extension in imageExtensions)
            {
                string imagePath = Path.Combine(directory, fileName + extension);
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

    private static void OpenTitleSnapshot(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
        string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in titleSnapshotExtensions)
        {
            string titleSnapshotPath = Path.Combine(titleSnapshotDirectory, fileName + extension);
            if (File.Exists(titleSnapshotPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(titleSnapshotPath);
                imageViewerWindow.Show();
                return;
            }
        }
        
        // Notify user
        MessageBoxLibrary.ThereIsNoTitleSnapshotMessageBox();
    }

    private void OpenGameplaySnapshot(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
        string[] gameplaySnapshotExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in gameplaySnapshotExtensions)
        {
            string gameplaySnapshotPath = Path.Combine(gameplaySnapshotDirectory, fileName + extension);
            if (File.Exists(gameplaySnapshotPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(gameplaySnapshotPath);
                imageViewerWindow.Show();
                return;
            }
        }
        
        // Notify user
        MessageBoxLibrary.ThereIsNoGameplaySnapshotMessageBox();
    }

    private void OpenCart(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
        string[] cartExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in cartExtensions)
        {
            string cartPath = Path.Combine(cartDirectory, fileName + extension);
            if (File.Exists(cartPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(cartPath);
                imageViewerWindow.Show();
                return;
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoCartMessageBox();
    }

    private void PlayVideo(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string videoDirectory = Path.Combine(baseDirectory, "videos", systemName);
        string[] videoExtensions = [".mp4", ".avi", ".mkv"];

        foreach (var extension in videoExtensions)
        {
            string videoPath = Path.Combine(videoDirectory, fileName + extension);
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

        // Notify user
        MessageBoxLibrary.ThereIsNoVideoFileMessageBox();
    }

    private void OpenManual(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string manualDirectory = Path.Combine(baseDirectory, "manuals", systemName);
        string[] manualExtensions = [".pdf"];

        foreach (var extension in manualExtensions)
        {
            string manualPath = Path.Combine(manualDirectory, fileName + extension);
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
                    string contextMessage = $"There was a problem opening the manual.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.CouldNotOpenManualMessageBox();

                    return;
                }
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoManualMessageBox();
    }

    private void OpenWalkthrough(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string walkthroughDirectory = Path.Combine(baseDirectory, "walkthrough", systemName);
        string[] walkthroughExtensions = [".pdf"];

        foreach (var extension in walkthroughExtensions)
        {
            string walkthroughPath = Path.Combine(walkthroughDirectory, fileName + extension);
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
                    string contextMessage = $"There was a problem opening the walkthrough.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.CouldNotOpenWalkthroughMessageBox();

                    return;
                }
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoWalkthroughMessageBox();
    }

    private void OpenCabinet(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
        string[] cabinetExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in cabinetExtensions)
        {
            string cabinetPath = Path.Combine(cabinetDirectory, fileName + extension);
            if (File.Exists(cabinetPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(cabinetPath);
                imageViewerWindow.Show();
                return;
            }
        }
        
        // Notify user
        MessageBoxLibrary.ThereIsNoCabinetMessageBox();
    }

    private void OpenFlyer(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
        string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in flyerExtensions)
        {
            string flyerPath = Path.Combine(flyerDirectory, fileName + extension);
            if (File.Exists(flyerPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(flyerPath);
                imageViewerWindow.Show();
                return;
            }
        }
        
        // Notify user
        MessageBoxLibrary.ThereIsNoFlyerMessageBox();
    }

    private void OpenPcb(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
        string[] pcbExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in pcbExtensions)
        {
            string pcbPath = Path.Combine(pcbDirectory, fileName + extension);
            if (File.Exists(pcbPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(pcbPath);
                imageViewerWindow.Show();
                return;
            }
        }
        
        // Notify user
        MessageBoxLibrary.ThereIsNoPcbMessageBox();
    }
        
    private async Task TakeScreenshotOfSelectedWindow(string fileNameWithoutExtension, SystemConfig systemConfig, Button button)
    {
        try
        {
            string systemName = systemConfig.SystemName;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string systemImageFolder = systemConfig.SystemImageFolder;

            if (string.IsNullOrEmpty(systemImageFolder))
            {
                systemImageFolder = Path.Combine(baseDirectory, "images", systemName);
                Directory.CreateDirectory(systemImageFolder);
            }

            // Wait for the Game or Emulator to launch
            await Task.Delay(4000);
                
            // Get the list of open windows
            var openWindows = WindowManager.GetOpenWindows();

            // Show the selection dialog
            var dialog = new WindowSelectionDialog(openWindows);
            if (dialog.ShowDialog() != true || dialog.SelectedWindowHandle == IntPtr.Zero)
            {
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
            
            // Update the button's image
            if (button.Content is Grid grid)
            {
                var stackPanel = grid.Children.OfType<StackPanel>().FirstOrDefault();
                var imageControl = stackPanel?.Children.OfType<Image>().FirstOrDefault();
                if (imageControl != null)
                {
                    // Reload the image without a file lock
                    await LoadImageAsync(imageControl, button, screenshotPath, DefaultImagePath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"There was a problem saving the screenshot.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.CouldNotSaveScreenshotMessageBox();
        }
    }

    private void DeleteFile(string filePath, string fileNameWithExtension, Button button)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                    
                PlayClick.PlayTrashSound();

                // Notify user
                MessageBoxLibrary.FileSuccessfullyDeletedMessageBox(fileNameWithExtension);

                // Remove the button from the UI
                gameFileGrid.Children.Remove(button);
            }
            catch (Exception ex)
            {
                // Notify developer
                string errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'." +
                                      $"Exception type: {ex.GetType().Name}\n" +
                                      $"Exception details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        else
        {
            // Notify developer
            string errorMessage = $"The file '{fileNameWithExtension}' could not be found.";
            Exception ex = new FileNotFoundException(errorMessage);
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
        }
    }

    private static async Task LoadImageAsync(Image imageControl, Button button, string imagePath, string defaultImagePath)
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
            imageControl.Dispatcher.Invoke(() => LoadFallbackImage(imageControl, button, defaultImagePath));
            
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
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
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