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
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public static class RightClickContextMenu
{
    // Use fileNameWithExtension
    public static void AddToFavorites(string systemName, string fileNameWithExtension, FavoritesManager favoritesManager, WrapPanel gameFileGrid, MainWindow mainWindow)
    {
        try
        {
            // Load existing favorites
            FavoritesConfig favorites = favoritesManager.LoadFavorites();

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
                favoritesManager.SaveFavorites(favorites);

                // Update the button's content to add the favorite icon dynamically
                try
                {
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
                catch (Exception)
                {
                    // ignore
                }
                
                // Find the GameListViewItem and update its IsFavorite property
                try
                {
                    var gameItem = mainWindow.GameListItems
                        .FirstOrDefault(g => g.FileName.Equals(Path.GetFileNameWithoutExtension(fileNameWithExtension), StringComparison.OrdinalIgnoreCase));
                
                    if (gameItem != null)
                    {
                        gameItem.IsFavorite = true;
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
                
                // Notify user
                MessageBoxLibrary.FileAddedToFavoritesMessageBox(fileNameWithExtension);
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
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
        }
    }

    // Use fileNameWithExtension
    public static void RemoveFromFavorites(string systemName, string fileNameWithExtension, FavoritesManager favoritesManager, WrapPanel gameFileGrid, MainWindow mainWindow)
    {
        try
        {
            // Load existing favorites
            FavoritesConfig favorites = favoritesManager.LoadFavorites();

            // Find the favorite to remove
            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);

                // Save the updated favorites list
                favoritesManager.SaveFavorites(favorites);

                // Update the button's content to remove the favorite icon dynamically
                try
                {
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
                catch (Exception)
                {
                    // ignore
                }
                
                // Find the GameListViewItem and update its IsFavorite property
                try
                {
                    var gameItem = mainWindow.GameListItems
                        .FirstOrDefault(g => g.FileName.Equals(Path.GetFileNameWithoutExtension(fileNameWithExtension), StringComparison.OrdinalIgnoreCase)); 
                    
                    if (gameItem != null)
                    {
                        gameItem.IsFavorite = false;
                    }
                }
                catch (Exception)
                {
                    // ignore
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
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenVideoLink(string systemName, string fileNameWithoutExtension, List<MameConfig> machines, SettingsConfig settings)
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
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorOpeningVideoLinkMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenInfoLink(string systemName, string fileNameWithoutExtension, List<MameConfig> machines, SettingsConfig settings)
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
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ProblemOpeningInfoLinkMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenHistoryWindow(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig, List<MameConfig> machines)
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
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.CouldNotOpenHistoryWindowMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenCover(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig)
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

    // Use fileNameWithoutExtension
    public static void OpenTitleSnapshot(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
        string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];

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
        MessageBoxLibrary.ThereIsNoTitleSnapshotMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenGameplaySnapshot(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
        string[] gameplaySnapshotExtensions = [".png", ".jpg", ".jpeg"];

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
        MessageBoxLibrary.ThereIsNoGameplaySnapshotMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenCart(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
        string[] cartExtensions = [".png", ".jpg", ".jpeg"];

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
        MessageBoxLibrary.ThereIsNoCartMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void PlayVideo(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string videoDirectory = Path.Combine(baseDirectory, "videos", systemName);
        string[] videoExtensions = [".mp4", ".avi", ".mkv"];

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

        // Notify user
        MessageBoxLibrary.ThereIsNoVideoFileMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenManual(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string manualDirectory = Path.Combine(baseDirectory, "manuals", systemName);
        string[] manualExtensions = [".pdf"];

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
                    string contextMessage = $"There was a problem opening the manual.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.CouldNotOpenManualMessageBox();

                    return;
                }
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoManualMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenWalkthrough(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string walkthroughDirectory = Path.Combine(baseDirectory, "walkthrough", systemName);
        string[] walkthroughExtensions = [".pdf"];

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
                    string contextMessage = $"There was a problem opening the walkthrough.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.CouldNotOpenWalkthroughMessageBox();

                    return;
                }
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoWalkthroughMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenCabinet(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
        string[] cabinetExtensions = [".png", ".jpg", ".jpeg"];

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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoCabinetMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenFlyer(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
        string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoFlyerMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenPcb(string systemName, string fileNameWithoutExtension)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
        string[] pcbExtensions = [".png", ".jpg", ".jpeg"];

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
        
        // Notify user
        MessageBoxLibrary.ThereIsNoPcbMessageBox();
    }

    // Use fileNameWithoutExtension
    public static async Task TakeScreenshotOfSelectedWindow(string fileNameWithoutExtension, SystemConfig systemConfig, Button button, MainWindow mainWindow)
    {
        try
        {
            // Clear the preview image
            try
            {
                mainWindow.PreviewImage.Source = null;
            }
            catch (Exception)
            {
                // ignore
            }
            
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
            try
            {
                if (button.Content is Grid grid)
                {
                    var stackPanel = grid.Children.OfType<StackPanel>().FirstOrDefault();
                    var imageControl = stackPanel?.Children.OfType<Image>().FirstOrDefault();
                    if (imageControl != null)
                    {
                        // Reload the image without a file lock
                        await GameButtonFactory.LoadImageAsync(imageControl, button, screenshotPath);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            // Reload the current Game List
            try
            {
                await mainWindow.LoadGameFilesAsync();
            }
            catch (Exception)
            {
                // ignore
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"There was a problem saving the screenshot.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.CouldNotSaveScreenshotMessageBox();
        }
    }

    // Use fileNameWithExtension
    public static async void DeleteFile(string filePath, string fileNameWithExtension, Button button, WrapPanel gameFileGrid, MainWindow mainWindow)
    {
        try
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
                    try
                    {
                        gameFileGrid.Children.Remove(button);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                
                    // Reload the current Game List
                    try
                    {
                        await mainWindow.LoadGameFilesAsync();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'." +
                                          $"Exception type: {ex.GetType().Name}\n" +
                                          $"Exception details: {ex.Message}";
                    LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
                
                    // Notify user
                    MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
                }
            }
            else
            {
                // Notify developer
                string errorMessage = $"The file '{fileNameWithExtension}' could not be found.";
                Exception ex = new FileNotFoundException(errorMessage);
                LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
            
                // Notify user
                MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Generic error while trying to delete the file '{fileNameWithExtension}'." +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
        }
    }
}