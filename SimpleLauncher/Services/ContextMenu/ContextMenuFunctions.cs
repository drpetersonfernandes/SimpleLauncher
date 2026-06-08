using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.GameItemFactory;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.TakeScreenshot;
using SimpleLauncher.WpfServices;
using Image = System.Windows.Controls.Image;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using WindowScreenshot = SimpleLauncher.Models.WindowScreenshot;
using CoreMessageBoxResult = SimpleLauncher.Core.Interfaces.MessageBoxResult;

namespace SimpleLauncher.Services.ContextMenu;

internal static class ContextMenuFunctions
{
    internal static async Task AddToFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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
                await favoritesManager.SaveFavoritesAsync();

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
                mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("FileAddedToFavorites") ?? "File added to favorites.");
                await messageBox.FileAddedToFavoritesMessageBox(fileNameWithExtension);
            }
            else
            {
                // Notify user
                await messageBox.GameIsAlreadyInFavoritesMessageBox(fileNameWithExtension);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while adding a game to the favorites.";
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await messageBox.ErrorWhileAddingFavoritesMessageBox();
        }
    }

    public static async Task RemoveFromFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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
            await favoritesManager.SaveFavoritesAsync();

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
            mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("FileRemovedFromFavorites") ?? "File removed from favorites.");
            await messageBox.FileRemovedFromFavoritesMessageBox(fileNameWithExtension);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while removing a game from favorites.";
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await messageBox.ErrorWhileRemovingGameFromFavoriteMessageBox();
        }
    }

    public static async Task OpenVideoLink(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, SettingsManager.SettingsManager settings, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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
            logErrors.LogAndForget(ex, "Win32Exception: No default application configured for opening web links (Video Link).");
            await messageBox.NoDefaultBrowserConfiguredMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem opening the Video Link.";
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ErrorOpeningVideoLink") ?? "Error opening video link.");
            await messageBox.ErrorOpeningVideoLinkMessageBox();
        }
    }

    public static async Task OpenInfoLink(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, SettingsManager.SettingsManager settings, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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
            logErrors.LogAndForget(ex, "Win32Exception: No default application configured for opening web links (Info Link).");
            await messageBox.NoDefaultBrowserConfiguredMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem opening the Info Link.";
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ErrorOpeningInfoLink") ?? "Error opening info link.");
            await messageBox.ProblemOpeningInfoLinkMessageBox();
        }
    }

    public static async Task OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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
            var historyWindow = App.ServiceProvider.GetRequiredService<RomHistoryWindow>();
            historyWindow.Initialize(romName, systemName, searchTerm);
            historyWindow.Show();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was a problem opening the History window.";
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ErrorOpeningROMHistory") ?? "Error opening ROM history.");
            await messageBox.CouldNotOpenHistoryWindowMessageBox();
        }
    }

    public static async Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManager.SystemManager systemManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        string tempExtractionPath = null;
        try
        {
            var settings = App.ServiceProvider.GetRequiredService<SettingsManager.SettingsManager>();

            if (string.IsNullOrWhiteSpace(settings.RaApiKey) || string.IsNullOrWhiteSpace(settings.RaUsername))
            {
                await messageBox.AddRaLoginMessageBox();
                playSoundEffects.PlayNotificationSound();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent("Missing credentials for RetroAchievements");

                    // Open RetroAchievements Settings Window
                    var raSettingsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsSettingsWindow>();
                    raSettingsWindow.Owner = mainWindow;
                    raSettingsWindow.ShowDialog();
                });

                // If user didn't save credentials, or saved empty ones, return
                if (string.IsNullOrWhiteSpace(settings.RaApiKey) || string.IsNullOrWhiteSpace(settings.RaUsername))
                {
                    return;
                }
            }

            var raManager = App.ServiceProvider.GetRequiredService<RetroAchievementsManager>();

            DebugLogger.Log($"[RA Service] Original system name: {systemManager.SystemName}");
            var systemName = RetroAchievementsSystemMatcher.GetBestMatchSystemName(systemManager.SystemName, logErrors);
            DebugLogger.Log($"[RA Service] Resolved system name: {systemName}");

            // Check if system is supported for RetroAchievements
            if (!RetroAchievementsHasherTool.IsSystemSupportedForHashing(systemManager.SystemName))
            {
                DebugLogger.Log($"[RA Service] System '{systemManager.SystemName}' is not supported for RetroAchievements.");

                var messageBoxResult = await messageBox.GameNotSupportedByRetroAchievementsMessageBox();
                if (messageBoxResult == CoreMessageBoxResult.Yes)
                {
                    playSoundEffects.PlayNotificationSound();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...");
                        var retroAchievementsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsWindow>();
                        retroAchievementsWindow.Show();
                    });
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent($"System '{systemManager.SystemName}' is not supported by RetroAchievements.");
                });

                return;
            }

            // Disable Hash calculation for systems that Group Files by Folder
            if (systemManager.GroupByFolder)
            {
                await messageBox.SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBox();
                DebugLogger.Log("[RA Service] 'Simple Launcher' does not support RetroAchievements hash of systems Grouped by Folder.");
                DebugLogger.Log("[RA Service] Please edit the system settings and disable the 'Group Files by Folder' option.");
                return;
            }

            if (!File.Exists(filePath))
            {
                DebugLogger.Log($"[RA Service] File not found at {filePath}");
                logErrors.LogAndForget(null, $"[RA Service] File not found at {filePath}");

                await messageBox.CouldNotFindAFileMessageBox();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent("Error launching the RetroAchievement for this game.");
                });

                return;
            }

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                DebugLogger.Log("[RA Service] FileNameWithoutExtension is null or empty.");
                logErrors.LogAndForget(null, "[RA Service] FileNameWithoutExtension is null or empty.");
                await messageBox.ErrorMessageBox();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent("Error launching the RetroAchievement for this game.");
                });

                return;
            }

            if (string.IsNullOrWhiteSpace(systemName))
            {
                DebugLogger.Log("[RA Service] SystemName is null or empty.");
                logErrors.LogAndForget(null, "[RA Service] SystemName is null or empty.");

                var messageBoxResult = await messageBox.GameNotSupportedByRetroAchievementsMessageBox();
                if (messageBoxResult == CoreMessageBoxResult.Yes)
                {
                    playSoundEffects.PlayNotificationSound();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...");
                        var retroAchievementsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsWindow>();
                        retroAchievementsWindow.Show();
                    });
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent("Error launching the RetroAchievement for this game.");
                });

                return;
            }

            var preparingRaMsg = (string)Application.Current.TryFindResource("CalculatingGameHash") ?? "Calculating Game Hash... Please wait.";

            // Show loading overlay before starting the hash calculation
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                loadingStateProvider.SetLoadingState(true, preparingRaMsg);
                mainWindow.UpdateStatusBarService.UpdateContent(preparingRaMsg);
            }, System.Windows.Threading.DispatcherPriority.Render);

            // Allow the UI to render the overlay before starting CPU-intensive hash calculation
            await Task.Delay(100);

            // --- Delegate hashing logic to RetroAchievementsHasherTool ---
            var raHashResult = await RetroAchievementsHasherTool.GetGameHashForRetroAchievementsAsync(filePath, systemName, systemManager.FileFormatsToLaunch, loadingStateProvider, logErrors);

            if (raHashResult.ExtractionErrorMessage == "System selection cancelled by user.")
            {
                DebugLogger.Log("[RA Service] User cancelled RetroAchievements hashing.");
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateStatusBarService.UpdateContent("Calculating the hash of the selected game");
            });

            var hash = raHashResult.Hash;
            tempExtractionPath = raHashResult.TempExtractionPath;

            // Prioritize checking if a hash was successfully obtained.
            if (string.IsNullOrEmpty(hash))
            {
                DebugLogger.Log($"[RA Service] Failed to get hash for '{fileNameWithoutExtension}' (System: {systemName}). Reason: {raHashResult.ExtractionErrorMessage}");

                // Check if the failure was due to "system not supported"
                if (raHashResult.ExtractionErrorMessage?.Contains("not supported for RetroAchievements hashing", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var messageBoxResult = await messageBox.GameNotSupportedByRetroAchievementsMessageBox();
                    if (messageBoxResult == CoreMessageBoxResult.Yes)
                    {
                        playSoundEffects.PlayNotificationSound();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...");
                            var retroAchievementsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsWindow>();
                            retroAchievementsWindow.Show();
                        });
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.UpdateStatusBarService.UpdateContent($"System '{systemName}' is not supported by RetroAchievements.");
                    });
                }
                // Check if the failure was due to an actual extraction issue (and not just "system not supported")
                else if (!raHashResult.IsExtractionSuccessful)
                {
                    await messageBox.ExtractionFailedMessageBox(); // Inform user about extraction failure

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.UpdateStatusBarService.UpdateContent("Error extracting the file for hashing");
                    });
                }
                else // A generic hashing failure not covered by the above
                {
                    var messageBoxResult = await messageBox.GameNotSupportedByRetroAchievementsMessageBox();
                    if (messageBoxResult == CoreMessageBoxResult.Yes)
                    {
                        playSoundEffects.PlayNotificationSound();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...");
                            var retroAchievementsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsWindow>();
                            retroAchievementsWindow.Show();
                        });
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.UpdateStatusBarService.UpdateContent($"Failed to get hash for '{fileNameWithoutExtension}' (System: {systemName}).");
                    });
                }

                return; // Exit as we cannot proceed without a valid hash
            }

            // If we reach here, a hash was successfully obtained. Proceed with lookup.
            DebugLogger.Log($"[RA Service] Successfully obtained hash: {hash}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateStatusBarService.UpdateContent($"Successfully obtained hash for {fileNameWithoutExtension}");
            });

            // Use the lookup method from RetroAchievementsManager
            var matchedGame = raManager.GetGameInfoByHash(hash);

            if (matchedGame != null)
            {
                DebugLogger.Log($"[RA Service] Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent($"Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");
                });

                // Ensure this is run on the UI thread as it creates a new window
                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    var achievementsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsForAGameWindow>();
                    achievementsWindow.Owner = mainWindow;
                    achievementsWindow.Initialize(matchedGame.Id, fileNameWithoutExtension);
                    achievementsWindow.Show();
                });
            }
            else
            {
                DebugLogger.Log($"[RA Service] No match found for hash: {hash}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow.UpdateStatusBarService.UpdateContent($"No match found for hash: {hash}");
                });

                var messageBoxResult = await messageBox.GameNotSupportedByRetroAchievementsMessageBox();
                if (messageBoxResult == CoreMessageBoxResult.Yes)
                {
                    playSoundEffects.PlayNotificationSound();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...");
                        var retroAchievementsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsWindow>();
                        retroAchievementsWindow.Show();
                    });
                }
            }
        }
        catch (Exception ex)
        {
            logErrors.LogAndForget(ex, $"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");
            DebugLogger.Log($"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");
            await messageBox.CouldNotOpenAchievementsWindowMessageBox();
        }
        finally
        {
            // Ensure loading indicator is hidden
            (loadingStateProvider as Window)?.Dispatcher.Invoke(() => loadingStateProvider.SetLoadingState(false));

            // --- Remove temporary extraction folder ---
            if (!string.IsNullOrEmpty(tempExtractionPath))
            {
                await CleanTempFolder.CleanupTempDirectoryAsync(tempExtractionPath);
                DebugLogger.Log($"[RA Service] Cleaned up temporary extraction folder: {tempExtractionPath}");
            }
        }
    }

    public static Task OpenCover(string systemName, string fileNameWithoutExtension, SystemManager.SystemManager systemManager, MainWindow mainWindow, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = systemManager.SystemImageFolder;

        // Ensure the systemImageFolder considers both absolute and relative paths
        // Resolve the path using PathHelper
        var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

        var globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);

        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningCoverImage") ?? "Opening cover image...");

        // Image extensions to look for
        var imageExtensions = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        // Try to find the image in the systemImageFolder directory first
        // Then search inside the globalImageDirectory
        if (TryFindImage(resolvedSystemImageFolder, out var foundImagePath) || TryFindImage(globalImageDirectory, out foundImagePath))
        {
            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(foundImagePath);
            imageViewerWindow.Show();
        }
        else
        {
            // Notify user
            return messageBox.ThereIsNoCoverMessageBox();
        }

        return Task.CompletedTask;

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

    public static Task OpenTitleSnapshot(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningTitleSnapshot") ?? "Opening title snapshot...");
        var titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
        string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in titleSnapshotExtensions)
        {
            var titleSnapshotPath = Path.Combine(titleSnapshotDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(titleSnapshotPath)) continue;

            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(titleSnapshotPath);
            imageViewerWindow.Show();
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoTitleSnapshotMessageBox();
    }

// Use fileNameWithoutExtension
    public static Task OpenGameplaySnapshot(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningGameplaySnapshot") ?? "Opening gameplay snapshot...");
        var gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
        var gameplaySnapshotExtensions = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        foreach (var extension in gameplaySnapshotExtensions)
        {
            var gameplaySnapshotPath = Path.Combine(gameplaySnapshotDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(gameplaySnapshotPath)) continue;

            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(gameplaySnapshotPath);
            imageViewerWindow.Show();
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoGameplaySnapshotMessageBox();
    }

// Use fileNameWithoutExtension
    public static Task OpenCart(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningCartImage") ?? "Opening cart image...");
        var cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
        string[] cartExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in cartExtensions)
        {
            var cartPath = Path.Combine(cartDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(cartPath)) continue;

            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(cartPath);
            imageViewerWindow.Show();
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoCartMessageBox();
    }

// Use fileNameWithoutExtension
    public static Task PlayVideo(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("PlayingVideo") ?? "Playing video...");
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
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoVideoFileMessageBox();
    }

// Use fileNameWithoutExtension
    public static async Task OpenManual(string systemName, string fileNameWithoutExtension, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningManual") ?? "Opening manual...");
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
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1155) // ERROR_NO_ASSOCIATION
            {
                // No application is associated with the file format
                // Notify developer
                const string contextMessage = "There was a problem opening the manual. No PDF viewer is installed.";
                logErrors.LogAndForget(ex, contextMessage);

                // Notify user
                await messageBox.NoPdfViewerInstalledMessageBox();

                return;
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "There was a problem opening the manual.";
                logErrors.LogAndForget(ex, contextMessage);

                // Notify user
                await messageBox.CouldNotOpenManualMessageBox();

                return;
            }
        }

        // Notify user
        await messageBox.ThereIsNoManualMessageBox();
    }

// Use fileNameWithoutExtension
    public static async Task OpenWalkthrough(string systemName, string fileNameWithoutExtension, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningWalkthrough") ?? "Opening walkthrough...");
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
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1155) // ERROR_NO_ASSOCIATION
            {
                // No application is associated with the file format
                // Notify developer
                const string contextMessage = "There was a problem opening the walkthrough. No PDF viewer is installed.";
                logErrors.LogAndForget(ex, contextMessage);

                // Notify user
                await messageBox.NoPdfViewerInstalledMessageBox();

                return;
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "There was a problem opening the walkthrough.";
                logErrors.LogAndForget(ex, contextMessage);

                // Notify user
                await messageBox.CouldNotOpenWalkthroughMessageBox();

                return;
            }
        }

        // Notify user
        await messageBox.ThereIsNoWalkthroughMessageBox();
    }

// Use fileNameWithoutExtension
    public static Task OpenCabinet(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningCabinetImage") ?? "Opening cabinet image...");
        var cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
        var cabinetExtensions = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        foreach (var extension in cabinetExtensions)
        {
            var cabinetPath = Path.Combine(cabinetDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(cabinetPath)) continue;

            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(cabinetPath);
            imageViewerWindow.Show();
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoCabinetMessageBox();
    }

// Use fileNameWithoutExtension
    public static Task OpenFlyer(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningFlyerImage") ?? "Opening flyer image...");
        var flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
        string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in flyerExtensions)
        {
            var flyerPath = Path.Combine(flyerDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(flyerPath)) continue;

            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(flyerPath);
            imageViewerWindow.Show();
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoFlyerMessageBox();
    }

// Use fileNameWithoutExtension
    public static Task OpenPcb(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningPCBImage") ?? "Opening PCB image...");
        var pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
        var pcbExtensions = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        foreach (var extension in pcbExtensions)
        {
            var pcbPath = Path.Combine(pcbDirectory, fileNameWithoutExtension + extension);
            if (!File.Exists(pcbPath)) continue;

            var imageViewerWindow = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            imageViewerWindow.LoadImagePath(pcbPath);
            imageViewerWindow.Show();
            return Task.CompletedTask;
        }

        // Notify user
        return messageBox.ThereIsNoPcbMessageBox();
    }

    public static async Task TakeScreenshotOfSelectedWindowAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager settings, Button button, MainWindow mainWindow, GamePadController gamePadController, GameLauncher.GameLauncher gameLauncher, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("TakingScreenshot") ?? "Taking screenshot...");
        try
        {
            // Clear the preview image
            try
            {
                mainWindow.PreviewImage.Source = null;
            }
            catch (Exception ex)
            {
                logErrors.LogAndForget(ex, "Error clearing preview image source before taking screenshot.");
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
                if (App.ServiceProvider != null)
                {
                    logErrors.LogAndForget(ex, $"[TakeScreenshotOfSelectedWindow] Could not create the system image folder: {systemImageFolder}");
                }
            }

            // Capture initial window count before launch
            var initialWindows = WindowManager.GetOpenWindows();
            var initialCount = initialWindows.Count;
            DebugLogger.Log($"[Screenshot] Initial window count: {initialCount}");

            // Launch game
            _ = gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, WpfWindowContext.FromMainWindow(mainWindow), gamePadController, loadingStateProvider);

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
                await messageBox.GameLaunchTimeoutMessageBox();
                return;
            }

            // Proceed with window selection (original logic)
            var openWindows = WindowManager.GetOpenWindows();

            // Show the selection dialog
            if (App.ServiceProvider != null)
            {
                var dialog = App.ServiceProvider.GetRequiredService<WindowSelectionDialogWindow>();
                dialog.Initialize(openWindows);
                if (dialog.ShowDialog() != true || dialog.SelectedWindowHandle == IntPtr.Zero)
                {
                    return;
                }

                var hWnd = dialog.SelectedWindowHandle;

                WindowScreenshot.Rectangle rectangle;

                // Try to get the client area dimensions
                if (!TakeScreenshot.WindowScreenshot.GetClientAreaRect(hWnd, out var clientRect))
                {
                    // If the client area fails, fall back to the full window dimensions
                    if (!TakeScreenshot.WindowScreenshot.GetWindowRect(hWnd, out rectangle))
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
                    await messageBox.CannotScreenshotMinimizedWindowMessageBox();
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
                var flashWindow = App.ServiceProvider.GetRequiredService<FlashOverlayWindow>();
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
                                var imageLoader = App.ServiceProvider.GetRequiredService<IImageLoader>();
                                var (imageStream, _) = await imageLoader.LoadImageAsync(screenshotPath);
                                imageControl.Source = imageStream.ToBitmapImage(); // Assign the loaded image
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        const string contextMessage = "[TakeScreenshotOfSelectedWindow] Failed to update button image after screenshot.";
                        logErrors.LogAndForget(ex, contextMessage);

                        // Do not notify the user
                    }
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
                logErrors.LogAndForget(ex, contextMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "[TakeScreenshotOfSelectedWindow] There was a problem saving the screenshot.";
            if (App.ServiceProvider != null)
            {
                logErrors.LogAndForget(ex, contextMessage);
            }

            // Notify user
            await messageBox.CouldNotSaveScreenshotMessageBox();
        }
    }

    public static async Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("DeletingGame") ?? "Deleting game...");
        if (File.Exists(filePath))
        {
            try
            {
                DeleteFiles.TryDeleteFile(filePath);

                playSoundEffects.PlayTrashSound();

                // Invalidate the game file caches in the main window
                await mainWindow.InvalidateGameFileCachesAsync();

                // Notify user
                await messageBox.FileSuccessfullyDeletedMessageBox(fileNameWithExtension);

                // Reload the current Game List to reflect the deletion
                try
                {
                    await mainWindow.LoadGameFilesAsync();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "There was a problem loading the Game Files after deletion.";
                    logErrors.LogAndForget(ex, contextMessage);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var errorMessage = $"An error occurred while trying to delete the file '{fileNameWithExtension}'.";
                logErrors.LogAndForget(ex, errorMessage);

                // Notify user
                await messageBox.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
            }
        }
        else
        {
            // Notify developer
            var contextMessage = $"The file '{fileNameWithExtension}' could not be found for deletion.\n\nFile path checked: {filePath}";
            logErrors.LogAndForget(null, contextMessage);

            // Notify user
            await messageBox.FileCouldNotBeDeletedMessageBox(fileNameWithExtension);
        }
    }

    public static async Task DeleteCoverImageAsync(string fileNameWithoutExtension, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager contextSettings, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IFindCoverImage findCoverImage, IMessageBoxLibraryService messageBox)
    {
        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("DeletingCoverImage") ?? "Deleting cover image...");
        var coverPath = findCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager, contextSettings);

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
                await messageBox.FileSuccessfullyDeletedMessageBox(coverPath);

                // Reload the current Game List
                await mainWindow.LoadGameFilesAsync();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"An error occurred while trying to delete the game cover '{coverPath}'.";
            logErrors.LogAndForget(ex, errorMessage);

            // Notify user
            await messageBox.FileCouldNotBeDeletedMessageBox(coverPath);
        }
    }
}