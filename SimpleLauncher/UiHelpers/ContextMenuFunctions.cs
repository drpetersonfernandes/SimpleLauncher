using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.ViewModels;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher.UiHelpers;

public static class ContextMenuFunctions
{
    // Use fileNameWithExtension
    public static void AddToFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow)
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
                    FileName = fileNameWithExtension,
                    SystemName = systemName
                });

                // Save the updated favorites list
                favoritesManager.FavoriteList = favorites.FavoriteList;
                favoritesManager.SaveFavorites();

                if (gameFileGrid != null)
                {
                    // Update the button's view model by locating the button using the composite tag.
                    try
                    {
                        // Build the key using systemName and file name WITH extension.
                        var key = $"{systemName}|{fileNameWithExtension}";

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
            const string contextMessage = "An error occurred while adding a game to the favorites.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
        }
    }

    // Use fileNameWithExtension
    public static void RemoveFromFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        try
        {
            // Load existing favorites
            var favorites = FavoritesManager.LoadFavorites();

            // Find the favorite to remove
            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove == null) return;

            favorites.FavoriteList.Remove(favoriteToRemove);

            // Save the updated favorites list
            favoritesManager.FavoriteList = favorites.FavoriteList;
            favoritesManager.SaveFavorites();

            if (gameFileGrid != null)
            {
                // Update the button's view model by locating the button using the composite tag.
                try
                {
                    // Build the key using systemName and file name WITH extension.
                    var key = $"{systemName}|{fileNameWithExtension}";
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
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while removing a game from favorites.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenVideoLink(string systemName, string fileNameWithoutExtension, List<MameManager> machines, SettingsManager settings)
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
            const string contextMessage = "There was a problem opening the Video Link.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorOpeningVideoLinkMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenInfoLink(string systemName, string fileNameWithoutExtension, List<MameManager> machines, SettingsManager settings)
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
            const string contextMessage = "There was a problem opening the Info Link.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ProblemOpeningInfoLinkMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, SystemManager systemManager, List<MameManager> machines)
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
            var historyWindow = new RomHistoryWindow(romName, systemName, searchTerm, systemManager);
            historyWindow.Show();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem opening the History window.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotOpenHistoryWindowMessageBox();
        }
    }

    // Use fileNameWithoutExtension
    public static void OpenCover(string systemName, string fileNameWithoutExtension, SystemManager systemManager)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = systemManager.SystemImageFolder;

        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null)
            {
                systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
            }
        }

        var globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        // Image extensions to look for
        var imageExtensions = GetImageExtensions.GetExtensions();

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
                if (!File.Exists(imagePath)) continue;

                foundPath = imagePath;
                return true;
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
        var gameplaySnapshotExtensions = GetImageExtensions.GetExtensions();

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
                const string contextMessage = "There was a problem opening the manual.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
                const string contextMessage = "There was a problem opening the walkthrough.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
        var cabinetExtensions = GetImageExtensions.GetExtensions();

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
        var pcbExtensions = GetImageExtensions.GetExtensions();

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
    public static async Task TakeScreenshotOfSelectedWindow(string fileNameWithoutExtension, SystemManager systemManager, Button button, MainWindow mainWindow)
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

            var systemName = systemManager.SystemName;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemImageFolder = systemManager.SystemImageFolder;

            if (string.IsNullOrEmpty(systemImageFolder))
            {
                systemImageFolder = Path.Combine(baseDirectory, "images", systemName);
                try
                {
                    Directory.CreateDirectory(systemImageFolder);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Could not create the system image folder.");
                }
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

            Services.WindowScreenshot.Rectangle rectangle;

            // Try to get the client area dimensions
            if (!WindowScreenshot.GetClientAreaRect(hWnd, out var clientRect))
            {
                // If the client area fails, fall back to the full window dimensions
                if (!WindowScreenshot.GetWindowRect(hWnd, out rectangle))
                {
                    throw new Exception("Failed to retrieve window dimensions.");
                }
            }
            else
            {
                // Successfully retrieved client area
                rectangle = clientRect;
            }

            var width = rectangle.Right - rectangle.Left;
            var height = rectangle.Bottom - rectangle.Top;

            var screenshotPath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}.png");

            // Capture the window into a bitmap
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        new Point(rectangle.Left, rectangle.Top),
                        Point.Empty,
                        new Size(width, height));
                }

                // Save the screenshot
                bitmap.Save(screenshotPath, ImageFormat.Png);
            }

            PlaySoundEffects.PlayShutterSound();

            // Wait
            await Task.Delay(1000);

            // Show the flash effect
            var flashWindow = new FlashOverlayWindow();
            await flashWindow.ShowFlashAsync();

            if (button != null)
            {
                // Update the button's image using the new ImageLoader
                try
                {
                    if (button?.Content is Grid grid)
                    {
                        // Find the Image control within the button's template

                        if (grid.Children.OfType<Border>()
                                .FirstOrDefault()?.Child is Image imageControl)
                        {
                            // Load the new screenshot image
                            var (loadedImage, _) = await ImageLoader.LoadImageAsync(screenshotPath);
                            imageControl.Source = loadedImage; // Assign the loaded image
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Failed to update button image after screenshot.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Do not notify the user
                }
            }

            // Reload the current Game List
            try
            {
                await mainWindow.LoadGameFilesAsync();
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "There was a problem loading the Game Files.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem saving the screenshot.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotSaveScreenshotMessageBox();
        }
    }

    // Use fileNameWithExtension
    public static async Task DeleteFile(string filePath, string fileNameWithExtension, MainWindow mainWindow)
    {
        if (File.Exists(filePath))
        {
            try
            {
                DeleteFiles.TryDeleteFile(filePath);

                PlaySoundEffects.PlayTrashSound();

                // Notify user
                MessageBoxLibrary.FileSuccessfullyDeletedMessageBox(fileNameWithExtension);

                // Reload the current Game List
                try
                {
                    await mainWindow.LoadGameFilesAsync();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "There was a problem loading the Game Files.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        else
        {
            // Notify developer
            var contextMessage = $"The file '{fileNameWithExtension}' could not be found.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
        }
    }
}