using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CleanFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.RetroAchievements;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using WindowScreenshot = SimpleLauncher.Services.TakeScreenshot.WindowScreenshot;

namespace SimpleLauncher.UiHelpers;

internal static class ContextMenuFunctions
{
    internal static void AddToFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects)
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

                playSoundEffects.PlayNotificationSound();

                // Save the updated favorites list using the injected instance
                favoritesManager.SaveFavorites();

                // Dynamic UI Update for both Grid and List views
                if (gameFileGrid != null) // GridView is active
                {
                    var key = $"{systemName}|{fileNameWithExtension}";
                    var button = gameFileGrid.Children.OfType<Button>()
                        .FirstOrDefault(b => b.Tag is GameButtonTag tag && string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase));

                    if (button is { Content: Grid { DataContext: GameButtonViewModel viewModel } })
                    {
                        viewModel.IsFavorite = true;
                    }
                }
                else // ListView is active (or called from another window)
                {
                    var gameItem = mainWindow.GameListItems.FirstOrDefault(g => Path.GetFileName(g.FilePath).Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase));

                    gameItem?.IsFavorite = true;
                }

                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FileAddedToFavorites") ?? "File added to favorites.", mainWindow);
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
        }
    }

    public static void RemoveFromFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects)
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

            playSoundEffects.PlayTrashSound();

            // Save the updated favorites list using the injected instance
            favoritesManager.SaveFavorites();

            // Dynamic UI Update Logic for both Grid and List views
            if (gameFileGrid != null) // GridView is active
            {
                var key = $"{systemName}|{fileNameWithExtension}";
                var button = gameFileGrid.Children.OfType<Button>()
                    .FirstOrDefault(b => b.Tag is GameButtonTag tag && string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase));

                if (button is { Content: Grid { DataContext: GameButtonViewModel viewModel } })
                {
                    viewModel.IsFavorite = false;
                }
            }
            else // ListView is active (or called from another window)
            {
                var gameItem = mainWindow.GameListItems
                    .FirstOrDefault(g => Path.GetFileName(g.FilePath).Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase));

                gameItem?.IsFavorite = false;
            }

            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FileRemovedFromFavorites") ?? "File removed from favorites.", mainWindow);
            MessageBoxLibrary.FileRemovedFromFavoritesMessageBox(fileNameWithExtension);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while removing a game from favorites.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }

    public static void OpenVideoLink(string systemName, string fileNameWithoutExtension, List<MameManager> machines, SettingsManager settings, MainWindow mainWindow)
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
        // Catch Win32Exception specifically for "No application associated" error
        catch (Win32Exception ex) when (ex.Message.Contains("No hay ninguna aplicación asociada", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("No application is associated", StringComparison.OrdinalIgnoreCase))
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Win32Exception: No default application configured for opening web links (Video Link).");
            MessageBoxLibrary.NoDefaultBrowserConfiguredMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem opening the Video Link.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ErrorOpeningVideoLink") ?? "Error opening video link.", mainWindow);
            MessageBoxLibrary.ErrorOpeningVideoLinkMessageBox();
        }
    }

    public static void OpenInfoLink(string systemName, string fileNameWithoutExtension, List<MameManager> machines, SettingsManager settings, MainWindow mainWindow)
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
        // Catch Win32Exception specifically for "No application associated" error
        catch (Win32Exception ex) when (ex.Message.Contains("No hay ninguna aplicación asociada", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("No application is associated", StringComparison.OrdinalIgnoreCase))
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Win32Exception: No default application configured for opening web links (Info Link).");
            MessageBoxLibrary.NoDefaultBrowserConfiguredMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem opening the Info Link.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ErrorOpeningInfoLink") ?? "Error opening info link.", mainWindow);
            MessageBoxLibrary.ProblemOpeningInfoLinkMessageBox();
        }
    }

    public static void OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, SystemManager systemManager, List<MameManager> machines, MainWindow mainWindow)
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ErrorOpeningROMHistory") ?? "Error opening ROM history.", mainWindow);
            MessageBoxLibrary.CouldNotOpenHistoryWindowMessageBox();
        }
    }

    public static async Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManager systemManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects)
    {
        string tempExtractionPath = null;
        try
        {
            var settings = App.ServiceProvider.GetRequiredService<SettingsManager>();

            if (string.IsNullOrWhiteSpace(settings.RaApiKey) || string.IsNullOrWhiteSpace(settings.RaUsername))
            {
                MessageBoxLibrary.AddRaLogin();
                UpdateStatusBar.UpdateContent("Missing credentials for RetroAchievements", mainWindow);

                playSoundEffects.PlayNotificationSound();

                // Open RetroAchievements Settings Window
                var raSettingsWindow = new RetroAchievementsSettingsWindow(settings)
                {
                    Owner = mainWindow // Set owner to main window
                };
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

            // Disable Hash calculation for systems that Group Files by Folder
            if (systemManager.GroupByFolder)
            {
                MessageBoxLibrary.SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolder();
                DebugLogger.Log("[RA Service] 'Simple Launcher' does not support RetroAchievements hash of systems Grouped by Folder.");
                DebugLogger.Log("[RA Service] Please edit the system settings and disable the 'Group Files by Folder' option.");
                return;
            }

            if (!File.Exists(filePath))
            {
                DebugLogger.Log($"[RA Service] File not found at {filePath}");
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RA Service] File not found at {filePath}");
                MessageBoxLibrary.CouldNotFindAFileMessageBox();

                UpdateStatusBar.UpdateContent("Error launching the RetroAchievement for this game.", mainWindow);

                return;
            }

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                DebugLogger.Log("[RA Service] FileNameWithoutExtension is null or empty.");
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "[RA Service] FileNameWithoutExtension is null or empty.");
                MessageBoxLibrary.ErrorMessageBox();
                UpdateStatusBar.UpdateContent("Error launching the RetroAchievement for this game.", mainWindow);

                return;
            }

            if (string.IsNullOrWhiteSpace(systemName))
            {
                DebugLogger.Log("[RA Service] SystemName is null or empty.");
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "[RA Service] SystemName is null or empty.");

                var messageBoxResult = MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    playSoundEffects.PlayNotificationSound();
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...", mainWindow);
                    var retroAchievementsWindow = new RetroAchievementsWindow();
                    retroAchievementsWindow.Show();
                }

                UpdateStatusBar.UpdateContent("Error launching the RetroAchievement for this game.", mainWindow);

                return;
            }

            // Set loading indicator
            mainWindow.SetUiLoadingState(true, (string)Application.Current.TryFindResource("PreparingRetroAchievements") ?? "Preparing RetroAchievements...");

            // --- Delegate hashing logic to RetroAchievementsHasherTool ---
            var raHashResult = await RetroAchievementsHasherTool.GetGameHashForRetroAchievementsAsync(filePath, systemName, systemManager.FileFormatsToLaunch);

            UpdateStatusBar.UpdateContent("Calculating the hash of the selected game", mainWindow);

            var hash = raHashResult.Hash;
            tempExtractionPath = raHashResult.TempExtractionPath;

            // Prioritize checking if a hash was successfully obtained.
            if (string.IsNullOrEmpty(hash))
            {
                DebugLogger.Log($"[RA Service] Failed to get hash for '{fileNameWithoutExtension}' (System: {systemName}). Reason: {raHashResult.ExtractionErrorMessage}");

                // Check if the failure was due to "system not supported"
                if (raHashResult.ExtractionErrorMessage?.Contains("not supported for RetroAchievements hashing", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var messageBoxResult = MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        playSoundEffects.PlayNotificationSound();
                        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...", mainWindow);
                        var retroAchievementsWindow = new RetroAchievementsWindow();
                        retroAchievementsWindow.Show();
                    }

                    UpdateStatusBar.UpdateContent($"System '{systemName}' is not supported by RetroAchievements.", mainWindow);
                }
                // Check if the failure was due to an actual extraction issue (and not just "system not supported")
                else if (!raHashResult.IsExtractionSuccessful)
                {
                    MessageBoxLibrary.ExtractionFailedMessageBox(); // Inform user about extraction failure
                    UpdateStatusBar.UpdateContent("Error extracting the file for hashing", mainWindow);
                }
                else // A generic hashing failure not covered by the above
                {
                    var messageBoxResult = MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        playSoundEffects.PlayNotificationSound();
                        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...", mainWindow);
                        var retroAchievementsWindow = new RetroAchievementsWindow();
                        retroAchievementsWindow.Show();
                    }

                    UpdateStatusBar.UpdateContent($"Failed to get hash for '{fileNameWithoutExtension}' (System: {systemName}).", mainWindow);
                }

                return; // Exit as we cannot proceed without a valid hash
            }

            // If we reach here, a hash was successfully obtained. Proceed with lookup.
            DebugLogger.Log($"[RA Service] Successfully obtained hash: {hash}");
            UpdateStatusBar.UpdateContent($"Successfully obtained hash for {fileNameWithoutExtension}", mainWindow);

            // Use the lookup method from RetroAchievementsManager
            var matchedGame = raManager.GetGameInfoByHash(hash);

            if (matchedGame != null)
            {
                DebugLogger.Log($"[RA Service] Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");
                UpdateStatusBar.UpdateContent($"Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})", mainWindow);

                // Ensure this is run on the UI thread as it creates a new window
                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    var achievementsWindow = new RetroAchievementsForAGameWindow(matchedGame.Id, fileNameWithoutExtension)
                    {
                        Owner = mainWindow // Set owner
                    };
                    achievementsWindow.Show();
                });
            }
            else
            {
                DebugLogger.Log($"[RA Service] No match found for hash: {hash}");
                UpdateStatusBar.UpdateContent($"No match found for hash: {hash}", mainWindow);

                var messageBoxResult = MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    playSoundEffects.PlayNotificationSound();
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...", mainWindow);
                    var retroAchievementsWindow = new RetroAchievementsWindow();
                    retroAchievementsWindow.Show();
                }
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");
            DebugLogger.Log($"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");

            MessageBoxLibrary.CouldNotOpenAchievementsWindowMessageBox();
        }
        finally
        {
            // Ensure loading indicator is hidden
            mainWindow.SetUiLoadingState(false);

            // --- Remove temporary extraction folder ---
            if (!string.IsNullOrEmpty(tempExtractionPath))
            {
                CleanFolder.CleanupTempDirectory(tempExtractionPath);
                DebugLogger.Log($"[RA Service] Cleaned up temporary extraction folder: {tempExtractionPath}");
            }
        }
    }

    public static void OpenCover(string systemName, string fileNameWithoutExtension, SystemManager systemManager, MainWindow mainWindow)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = systemManager.SystemImageFolder;

        // Ensure the systemImageFolder considers both absolute and relative paths
        // Resolve the path using PathHelper
        var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

        var globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningCoverImage") ?? "Opening cover image...", mainWindow);

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

    public static void OpenTitleSnapshot(string systemName, string fileNameWithoutExtension)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningTitleSnapshot") ?? "Opening title snapshot...", Application.Current.MainWindow as MainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGameplaySnapshot") ?? "Opening gameplay snapshot...", Application.Current.MainWindow as MainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningCartImage") ?? "Opening cart image...", Application.Current.MainWindow as MainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("PlayingVideo") ?? "Playing video...", Application.Current.MainWindow as MainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningManual") ?? "Opening manual...", Application.Current.MainWindow as MainWindow);
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningWalkthrough") ?? "Opening walkthrough...", Application.Current.MainWindow as MainWindow);
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningCabinetImage") ?? "Opening cabinet image...", Application.Current.MainWindow as MainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningFlyerImage") ?? "Opening flyer image...", Application.Current.MainWindow as MainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningPCBImage") ?? "Opening PCB image...", Application.Current.MainWindow as MainWindow);
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

    public static async Task TakeScreenshotOfSelectedWindow(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager selectedSystemManager, SettingsManager settings, Button button, MainWindow mainWindow, GamePadController gamePadController, GameLauncher gameLauncher, PlaySoundEffects playSoundEffects)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TakingScreenshot") ?? "Taking screenshot...", mainWindow);
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"[TakeScreenshotOfSelectedWindow] Could not create the system image folder: {systemImageFolder}");
            }

            // Capture initial window count before launch
            var initialWindows = WindowManager.GetOpenWindows();
            var initialCount = initialWindows.Count;
            DebugLogger.Log($"[Screenshot] Initial window count: {initialCount}");

            // Launch game
            _ = gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, mainWindow, gamePadController);

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

            WindowScreenshot.Rectangle rectangle;

            // Try to get the client area dimensions
            if (!WindowScreenshot.GetClientAreaRect(hWnd, out var clientRect))
            {
                // If the client area fails, fall back to the full window dimensions
                if (!WindowScreenshot.GetWindowRect(hWnd, out rectangle))
                {
                    throw new InvalidOperationException("Failed to retrieve window dimensions.");
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

            playSoundEffects.PlayShutterSound();

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
                    if (button.Content is Grid grid)
                    {
                        // Find the Image control within the button's template
                        if (grid.Children.OfType<Border>().FirstOrDefault()?.Child is Image imageControl)
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
                    const string contextMessage = "[TakeScreenshotOfSelectedWindow] Failed to update button image after screenshot.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
                const string contextMessage = "[TakeScreenshotOfSelectedWindow] There was a problem loading the Game Files.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "[TakeScreenshotOfSelectedWindow] There was a problem saving the screenshot.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotSaveScreenshotMessageBox();
        }
    }

    public static async Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow, PlaySoundEffects playSoundEffects)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("DeletingGame") ?? "Deleting game...", mainWindow);
        if (File.Exists(filePath))
        {
            try
            {
                DeleteFiles.TryDeleteFile(filePath);

                playSoundEffects.PlayTrashSound();

                // Invalidate the game file caches in the main window
                mainWindow.InvalidateGameFileCaches();

                // Notify user
                MessageBoxLibrary.FileSuccessfullyDeletedMessageBox(fileNameWithExtension);

                // Reload the current Game List to reflect the deletion
                try
                {
                    await mainWindow.LoadGameFilesAsync();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "There was a problem loading the Game Files after deletion.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        else
        {
            // Notify developer
            var contextMessage = $"The file '{fileNameWithExtension}' could not be found for deletion.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
        }
    }

    public static async Task DeleteCoverImageAsync(string fileNameWithoutExtension, string selectedSystemName, SystemManager selectedSystemManager, SettingsManager contextSettings, MainWindow mainWindow, PlaySoundEffects playSoundEffects)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("DeletingCoverImage") ?? "Deleting cover image...", mainWindow);
        var coverPath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager, contextSettings);

        try
        {
            playSoundEffects.PlayTrashSound();

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.FileCouldNotBeDeletedMessageBox(coverPath);
        }
    }
}