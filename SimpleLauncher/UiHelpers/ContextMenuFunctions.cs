using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.ViewModels;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher.UiHelpers;

public static class ContextMenuFunctions
{
    public static void AddToFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        try
        {
            // Add the new favorite if it doesn't already exist
            if (!favoritesManager.FavoriteList.Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                        && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase)))
            {
                favoritesManager.FavoriteList.Add(new Favorite
                {
                    FileName = fileNameWithExtension,
                    SystemName = systemName
                });

                // Save the updated favorites list using the injected instance
                favoritesManager.SaveFavorites();

                // Dynamic UI Update for both Grid and List views
                if (gameFileGrid != null) // GridView is active
                {
                    var key = $"{systemName}|{fileNameWithExtension}";
                    var button = gameFileGrid.Children.OfType<Button>()
                        .FirstOrDefault(b => b.Tag is GameButtonTag tag &&
                                             string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase));

                    if (button is { Content: Grid { DataContext: GameButtonViewModel viewModel } })
                    {
                        viewModel.IsFavorite = true;
                    }
                }
                else // ListView is active (or called from another window)
                {
                    var gameItem = mainWindow.GameListItems
                        .FirstOrDefault(g => Path.GetFileName(g.FilePath).Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase));

                    if (gameItem != null)
                    {
                        gameItem.IsFavorite = true;
                    }
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

    public static void RemoveFromFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        try
        {
            // Find the favorite to remove
            var favoriteToRemove = favoritesManager.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                                     && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove == null)
            {
                return;
            }

            favoritesManager.FavoriteList.Remove(favoriteToRemove);

            // Save the updated favorites list using the injected instance
            favoritesManager.SaveFavorites();

            // Dynamic UI Update Logic for both Grid and List views
            if (gameFileGrid != null) // GridView is active
            {
                var key = $"{systemName}|{fileNameWithExtension}";
                var button = gameFileGrid.Children.OfType<Button>()
                    .FirstOrDefault(b => b.Tag is GameButtonTag tag &&
                                         string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase));

                if (button is { Content: Grid { DataContext: GameButtonViewModel viewModel } })
                {
                    viewModel.IsFavorite = false;
                }
            }
            else // ListView is active (or called from another window)
            {
                var gameItem = mainWindow.GameListItems
                    .FirstOrDefault(g => Path.GetFileName(g.FilePath).Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase));

                if (gameItem != null)
                {
                    gameItem.IsFavorite = false;
                }
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

    public static async Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManager systemManager, MainWindow mainWindow)
    {
        string tempExtractionPath = null;
        try
        {
            // Get services from the service provider
            var settings = App.ServiceProvider.GetRequiredService<SettingsManager>();

            if (string.IsNullOrWhiteSpace(settings.RaApiKey) || string.IsNullOrWhiteSpace(settings.RaUsername))
            {
                MessageBoxLibrary.AddRaLogin();

                // Open RetroAchievements Settings Window
                var raSettingsWindow = new RetroAchievementsSettingsWindow(settings);
                raSettingsWindow.Owner = mainWindow; // Set owner to main window
                raSettingsWindow.ShowDialog();

                // If user didn't save credentials, or saved empty ones, return
                if (string.IsNullOrWhiteSpace(settings.RaApiKey) || string.IsNullOrWhiteSpace(settings.RaUsername))
                {
                    return;
                }
            }

            var raManager = App.ServiceProvider.GetRequiredService<RetroAchievementsManager>();

            DebugLogger.Log($"[RA Service] Original system name: {systemManager.SystemName}");
            var systemName = RetroAchievementsSystemMatcher.GetBestMatchSystemName(systemManager.SystemName);
            DebugLogger.Log($"[RA Service] Resolved system name: {systemName}");

            if (!File.Exists(filePath))
            {
                DebugLogger.Log($"[RA Service] File not found at {filePath}");
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] File not found at {filePath}");
                MessageBoxLibrary.CouldNotFindAFileMessageBox();
                return;
            }

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                DebugLogger.Log("[RA Service] FileNameWithoutExtension is null or empty.");
                _ = LogErrors.LogErrorAsync(null, "[RA Service] FileNameWithoutExtension is null or empty.");
                MessageBoxLibrary.ErrorMessageBox();
                return;
            }

            if (string.IsNullOrWhiteSpace(systemName))
            {
                DebugLogger.Log("[RA Service] SystemName is null or empty.");
                _ = LogErrors.LogErrorAsync(null, "[RA Service] SystemName is null or empty.");
                MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                return;
            }

            // Set loading indicator
            mainWindow.IsLoadingGames = true; // This is a DependencyProperty, safe to set from any thread.

            // --- Delegate hashing logic to RetroAchievementsHasherTool ---
            var raHashResult = await RetroAchievementsHasherTool.GetGameHashForRetroAchievementsAsync(filePath, systemName, systemManager.FileFormatsToLaunch);

            var hash = raHashResult.Hash;
            tempExtractionPath = raHashResult.TempExtractionPath;

            // Check for extraction failure
            if (!raHashResult.IsExtractionSuccessful)
            {
                DebugLogger.Log($"[RA Service] Extraction failed for '{fileNameWithoutExtension}': {raHashResult.ExtractionErrorMessage}");
                MessageBoxLibrary.ExtractionFailedMessageBox(); // Inform user about extraction failure
                return; // Exit as we cannot proceed without a valid extracted file or hash
            }

            if (string.IsNullOrEmpty(hash))
            {
                DebugLogger.Log($"[RA Service] Failed to get hash for '{fileNameWithoutExtension}' (System: {systemName}).");
                MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                return;
            }

            DebugLogger.Log($"[RA Service] Successfully obtained hash: {hash}");

            // Use the lookup method from RetroAchievementsManager
            var matchedGame = raManager.GetGameInfoByHash(hash);

            if (matchedGame != null)
            {
                DebugLogger.Log($"[RA Service] Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");
                // Ensure this is run on the UI thread as it creates a new window
                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    var achievementsWindow = new RetroAchievementsWindow(matchedGame.Id, fileNameWithoutExtension);
                    achievementsWindow.Owner = mainWindow; // Set owner
                    achievementsWindow.Show();
                });
            }
            else
            {
                DebugLogger.Log($"[RA Service] No match found for hash: {hash}");
                MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");
            DebugLogger.Log($"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");

            MessageBoxLibrary.CouldNotOpenAchievementsWindowMessageBox();
        }
        finally
        {
            // Ensure loading indicator is hidden
            mainWindow.IsLoadingGames = false; // This is a DependencyProperty, safe to set from any thread.

            // --- Remove temporary extraction folder ---
            if (!string.IsNullOrEmpty(tempExtractionPath))
            {
                CleanFolder.CleanupTempDirectory(tempExtractionPath);
                DebugLogger.Log($"[RA Service] Cleaned up temporary extraction folder: {tempExtractionPath}");
            }
        }
    }

    public static void OpenCover(string systemName, string fileNameWithoutExtension, SystemManager systemManager)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = systemManager.SystemImageFolder;

        // Ensure the systemImageFolder considers both absolute and relative paths
        // Resolve the path using PathHelper
        var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

        var globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        // Image extensions to look for
        var imageExtensions = GetImageExtensions.GetExtensions();

        // Try to find the image in the systemImageFolder directory first
        // Then search inside the globalImageDirectory
        if (TryFindImage(resolvedSystemImageFolder, out var foundImagePath) || TryFindImage(globalImageDirectory, out foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImagePath(foundImagePath);
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
            foundPath = null;
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return false;
            }

            foreach (var extension in imageExtensions)
            {
                var imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                if (!File.Exists(imagePath)) continue;

                foundPath = imagePath;
                return true;
            }

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
            imageViewerWindow.LoadImagePath(titleSnapshotPath);
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
            imageViewerWindow.LoadImagePath(gameplaySnapshotPath);
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
            imageViewerWindow.LoadImagePath(cartPath);
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
            imageViewerWindow.LoadImagePath(cabinetPath);
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
            imageViewerWindow.LoadImagePath(flyerPath);
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
            imageViewerWindow.LoadImagePath(pcbPath);
            imageViewerWindow.Show();
            return;
        }

        // Notify user
        MessageBoxLibrary.ThereIsNoPcbMessageBox();
    }

    public static async Task TakeScreenshotOfSelectedWindow(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager selectedSystemManager, SettingsManager settings, Button button, MainWindow mainWindow)
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

            var systemImageFolder = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.SystemImageFolder);

            if (string.IsNullOrEmpty(systemImageFolder))
            {
                // Fallback to default if resolution fails or path is empty
                systemImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedSystemName);
            }

            try
            {
                Directory.CreateDirectory(systemImageFolder);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Could not create the system image folder: {systemImageFolder}");
            }

            // Capture initial window count before launch
            var initialWindows = WindowManager.GetOpenWindows();
            var initialCount = initialWindows.Count;
            DebugLogger.Log($"[Screenshot] Initial window count: {initialCount}");

            // Launch game
            _ = GameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, mainWindow);

            // Minimum wait time to process startup)
            await Task.Delay(2000);

            // Poll for new windows
            var maxWaitTime = TimeSpan.FromSeconds(30); // Max 30 seconds
            var pollInterval = TimeSpan.FromMilliseconds(500); // Poll every 500ms
            var stopwatch = Stopwatch.StartNew();
            var newWindowDetected = false;

            while (stopwatch.Elapsed < maxWaitTime && !newWindowDetected)
            {
                await Task.Delay(pollInterval);

                var currentWindows = WindowManager.GetOpenWindows();
                if (currentWindows.Count > initialCount)
                {
                    // New window(s) appeared - assume game/emulator launched
                    DebugLogger.Log($"[Screenshot] New window detected. Current count: {currentWindows.Count} (initial: {initialCount})");
                    newWindowDetected = true;
                    break;
                }

                // Optional: Log progress every few polls
                if (stopwatch.Elapsed.TotalSeconds % 5 < pollInterval.TotalMilliseconds / 1000.0)
                {
                    DebugLogger.Log($"[Screenshot] Polling... Elapsed: {stopwatch.Elapsed.TotalSeconds:F1}s / {maxWaitTime.TotalSeconds}s");
                }
            }

            stopwatch.Stop();

            if (!newWindowDetected)
            {
                // Timeout - no new windows appeared
                DebugLogger.Log($"[Screenshot] Timeout after {stopwatch.Elapsed.TotalSeconds:F1}s. No new windows detected.");
                MessageBoxLibrary.GameLaunchTimeoutMessageBox();
                return;
            }

            // Proceed with window selection (original logic)
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

            // Add a check for invalid dimensions (i.e., a minimized window)
            if (width <= 0 || height <= 0)
            {
                // Notify the user that they can't screenshot a minimized window.
                MessageBoxLibrary.CannotScreenshotMinimizedWindowMessageBox();
                DebugLogger.Log("Cannot take a screenshot of a minimized window.");

                return; // Exit the method gracefully
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
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

    public static async Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow)
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

    public static async Task DeleteCoverImageAsync(string fileNameWithoutExtension,
        string selectedSystemName,
        SystemManager selectedSystemManager,
        SettingsManager contextSettings,
        MainWindow mainWindow)
    {
        var coverPath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager, contextSettings);

        try
        {
            PlaySoundEffects.PlayTrashSound();
            if ((Path.GetFileNameWithoutExtension(coverPath) == fileNameWithoutExtension) & (Path.GetFileNameWithoutExtension(coverPath) != "default"))
            {
                DeleteFiles.TryDeleteFile(coverPath);
            }

            await Task.Delay(400);

            if (!File.Exists(coverPath))
            {
                // Notify user
                MessageBoxLibrary.FileSuccessfullyDeletedMessageBox(coverPath);

                // Reload the current Game List
                await mainWindow.LoadGameFilesAsync();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"An error occurred while trying to delete the game cover '{coverPath}'.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(coverPath);
        }
    }
}