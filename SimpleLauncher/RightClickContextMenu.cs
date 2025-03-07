using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            var favorites = FavoritesManager.LoadFavorites();

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
                favoritesManager.FavoriteList = favorites.FavoriteList;
                favoritesManager.SaveFavorites();

                // Update the button's view model by locating the button using the composite tag.
                try
                {
                    // Build the key using systemName and file name without extension.
                    var key = $"{systemName}|{Path.GetFileNameWithoutExtension(fileNameWithExtension)}";

                    // Find the button by checking if its Tag is a GameButtonTag and comparing its Key.
                    var button = gameFileGrid.Children.OfType<Button>()
                        .FirstOrDefault(b => b.Tag is GameButtonTag tag &&
                                             string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase));

                    if (button is { Content: Grid { DataContext: GameButtonViewModel viewModel } })
                    {
                        viewModel.IsFavorite = true; // This automatically makes the star overlay visible.
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
            var contextMessage = $"An error occurred while adding a game to the favorites.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
            var favorites = FavoritesManager.LoadFavorites();

            // Find the favorite to remove
            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);

                // Save the updated favorites list
                favoritesManager.FavoriteList = favorites.FavoriteList;
                favoritesManager.SaveFavorites();

                // Update the button's view model by locating the button using the composite tag.
                try
                {
                    var key = $"{systemName}|{Path.GetFileNameWithoutExtension(fileNameWithExtension)}";
                    var button = gameFileGrid.Children.OfType<Button>()
                        .FirstOrDefault(b => b.Tag is GameButtonTag tag &&
                                             string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase));

                    if (button is { Content: Grid { DataContext: GameButtonViewModel viewModel } })
                    {
                        viewModel.IsFavorite = false; // This will collapse the star overlay.
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

                // Notify user
                MessageBoxLibrary.FileRemovedFromFavoritesMessageBox(fileNameWithExtension);
            }
            // else
            // {
            //     // Notify user
            //     MessageBoxLibrary.FileIsNotInFavoritesMessageBox(fileNameWithExtension);
            // }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"An error occurred while removing a game from favorites.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenVideoLink(string systemName, string fileNameWithoutExtension, List<MameConfig> machines, SettingsConfig settings)
    {
        // Attempt to find a matching machine description
        var searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        var searchUrl = $"{settings.VideoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
            var contextMessage = $"There was a problem opening the Video Link.\n\n" +
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
        var searchTerm = fileNameWithoutExtension;
        var machine = machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        var searchUrl = $"{settings.InfoUrl}{Uri.EscapeDataString($"{searchTerm} {systemName}")}";

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
            var contextMessage = $"There was a problem opening the Info Link.\n\n" +
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
        var romName = fileNameWithoutExtension.ToLowerInvariant();

        // Attempt to find a matching machine description
        var searchTerm = fileNameWithoutExtension;
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
            var contextMessage = $"There was a problem opening the History window.\n\n" +
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
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = systemConfig.SystemImageFolder;

        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }

        var globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        // Image extensions to look for
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // Try to find the image in the systemImageFolder directory first
        // Then search inside the globalImageDirectory
        if (TryFindImage(systemImageFolder, out var foundImagePath) || TryFindImage(globalImageDirectory, out foundImagePath))
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
                var imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
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
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
        string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in titleSnapshotExtensions)
        {
            var titleSnapshotPath = Path.Combine(titleSnapshotDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(titleSnapshotPath)) continue;
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(titleSnapshotPath);
            imageViewerWindow.Show();
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoTitleSnapshotMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenGameplaySnapshot(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
        string[] gameplaySnapshotExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in gameplaySnapshotExtensions)
        {
            var gameplaySnapshotPath = Path.Combine(gameplaySnapshotDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(gameplaySnapshotPath)) continue;
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(gameplaySnapshotPath);
            imageViewerWindow.Show();
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoGameplaySnapshotMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenCart(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
        string[] cartExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in cartExtensions)
        {
            var cartPath = Path.Combine(cartDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(cartPath)) continue;
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(cartPath);
            imageViewerWindow.Show();
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoCartMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void PlayVideo(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var videoDirectory = Path.Combine(baseDirectory, "videos", systemName);
        string[] videoExtensions = [".mp4", ".avi", ".mkv"];

        foreach (var extension in videoExtensions)
        {
            var videoPath = Path.Combine(videoDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(videoPath)) continue;
            Process.Start(new ProcessStartInfo
            {
                FileName = videoPath,
                UseShellExecute = true
            });
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoVideoFileMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenManual(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var manualDirectory = Path.Combine(baseDirectory, "manuals", systemName);
        string[] manualExtensions = [".pdf"];

        foreach (var extension in manualExtensions)
        {
            var manualPath = Path.Combine(manualDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(manualPath)) continue;
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
                var contextMessage = $"There was a problem opening the manual.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
                LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

                // Notify user
                MessageBoxLibrary.CouldNotOpenManualMessageBox();

                return;
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoManualMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenWalkthrough(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var walkthroughDirectory = Path.Combine(baseDirectory, "walkthrough", systemName);
        string[] walkthroughExtensions = [".pdf"];

        foreach (var extension in walkthroughExtensions)
        {
            var walkthroughPath = Path.Combine(walkthroughDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(walkthroughPath)) continue;
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
                var contextMessage = $"There was a problem opening the walkthrough.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
                LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

                // Notify user
                MessageBoxLibrary.CouldNotOpenWalkthroughMessageBox();

                return;
            }
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoWalkthroughMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenCabinet(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
        string[] cabinetExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in cabinetExtensions)
        {
            var cabinetPath = Path.Combine(cabinetDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(cabinetPath)) continue;
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(cabinetPath);
            imageViewerWindow.Show();
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoCabinetMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenFlyer(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
        string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in flyerExtensions)
        {
            var flyerPath = Path.Combine(flyerDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(flyerPath)) continue;
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(flyerPath);
            imageViewerWindow.Show();
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoFlyerMessageBox();
    }

    // Use fileNameWithoutExtension
    public static void OpenPcb(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
        string[] pcbExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in pcbExtensions)
        {
            var pcbPath = Path.Combine(pcbDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(pcbPath)) continue;
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(pcbPath);
            imageViewerWindow.Show();
            return;
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

            var systemName = systemConfig.SystemName;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemImageFolder = systemConfig.SystemImageFolder;

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
            var dialog = new WindowSelectionDialogWindow(openWindows);
            if (dialog.ShowDialog() != true || dialog.SelectedWindowHandle == IntPtr.Zero)
            {
                return;
            }

            var hWnd = dialog.SelectedWindowHandle;

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

            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;

            var screenshotPath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}.png");

            // Capture the window into a bitmap
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        new Point(rect.Left, rect.Top),
                        Point.Empty,
                        new Size(width, height));
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
            var contextMessage = $"There was a problem saving the screenshot.\n\n" +
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
                    var errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'." +
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
                var errorMessage = $"The file '{fileNameWithExtension}' could not be found.";
                Exception ex = new FileNotFoundException(errorMessage);
                LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));

                // Notify user
                MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"Generic error while trying to delete the file '{fileNameWithExtension}'." +
                               $"Exception type: {ex.GetType().Name}\n" +
                               $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
        }
    }
}