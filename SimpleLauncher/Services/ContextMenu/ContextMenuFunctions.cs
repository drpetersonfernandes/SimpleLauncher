using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameItemFactory;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.TakeScreenshot;
using SimpleLauncher.Services.WpfServices;
using Image = System.Windows.Controls.Image;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using WindowScreenshot = SimpleLauncher.Models.WindowScreenshot;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

namespace SimpleLauncher.Services.ContextMenu;

/// <summary>
/// Implements context menu actions for game items such as favorites, media viewing, screenshots, and deletion.
/// </summary>
public class ContextMenuFunctions : IContextMenuFunctions
{
    private readonly IDebugLogger _debugLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextMenuFunctions"/> class.
    /// </summary>
    /// <param name="debugLogger">The logger used to record debug information.</param>
    public ContextMenuFunctions(IDebugLogger debugLogger)
    {
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
    }

    /// <summary>
    /// Adds a game to the favorites list and updates the UI accordingly.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithExtension">The file name of the game with its extension.</param>
    /// <param name="gameFileGrid">The wrap panel containing game buttons, or null if using list view.</param>
    /// <param name="favoritesManager">The favorites manager instance.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task AddToFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Removes a game from the favorites list and updates the UI accordingly.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithExtension">The file name of the game with its extension.</param>
    /// <param name="gameFileGrid">The wrap panel containing game buttons, or null if using list view.</param>
    /// <param name="favoritesManager">The favorites manager instance.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task RemoveFromFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens a video link for the specified game in the default browser.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="machines">The collection of MAME machine entries for description lookup.</param>
    /// <param name="settings">The application settings containing the video URL template.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task OpenVideoLink(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, SettingsManager.SettingsManager settings, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens an information link for the specified game in the default browser.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="machines">The collection of MAME machine entries for description lookup.</param>
    /// <param name="settings">The application settings containing the info URL template.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task OpenInfoLink(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, SettingsManager.SettingsManager settings, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the ROM history window for the specified game.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="machines">The collection of MAME machine entries for description lookup.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the RetroAchievements window for the specified game, performing hash calculation and game lookup.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="systemManager">The system manager for the selected system.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="loadingStateProvider">The loading state provider for showing/hiding overlays.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManager.SystemManager systemManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

            var debugLogger = App.ServiceProvider.GetRequiredService<IDebugLogger>();
            debugLogger.Log($"[RA Service] Original system name: {systemManager.SystemName}");
            var systemMatcher = App.ServiceProvider.GetRequiredService<IRetroAchievementsSystemMatcher>();
            var systemName = systemMatcher.GetBestMatchSystemName(systemManager.SystemName);
            debugLogger.Log($"[RA Service] Resolved system name: {systemName}");

            // Check if system is supported for RetroAchievements
            var raHasherTool = App.ServiceProvider.GetRequiredService<IRetroAchievementsHasherTool>();
            if (!raHasherTool.IsSystemSupportedForHashing(systemManager.SystemName))
            {
                _debugLogger.Log($"[RA Service] System '{systemManager.SystemName}' is not supported for RetroAchievements.");

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
                _debugLogger.Log("[RA Service] 'Simple Launcher' does not support RetroAchievements hash of systems Grouped by Folder.");
                _debugLogger.Log("[RA Service] Please edit the system settings and disable the 'Group Files by Folder' option.");
                return;
            }

            if (!File.Exists(filePath))
            {
                _debugLogger.Log($"[RA Service] File not found at {filePath}");
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
                _debugLogger.Log("[RA Service] FileNameWithoutExtension is null or empty.");
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
                _debugLogger.Log("[RA Service] SystemName is null or empty.");
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
            var raHashResult = await raHasherTool.GetGameHashForRetroAchievementsAsync(filePath, systemName, systemManager.FileFormatsToLaunch, loadingStateProvider, logErrors);

            if (raHashResult.ExtractionErrorMessage == "System selection cancelled by user.")
            {
                _debugLogger.Log("[RA Service] User cancelled RetroAchievements hashing.");
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
                _debugLogger.Log($"[RA Service] Failed to get hash for '{fileNameWithoutExtension}' (System: {systemName}). Reason: {raHashResult.ExtractionErrorMessage}");

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
            _debugLogger.Log($"[RA Service] Successfully obtained hash: {hash}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateStatusBarService.UpdateContent($"Successfully obtained hash for {fileNameWithoutExtension}");
            });

            // Use the lookup method from RetroAchievementsManager
            var matchedGame = raManager.GetGameInfoByHash(hash);

            if (matchedGame != null)
            {
                _debugLogger.Log($"[RA Service] Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");

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
                _debugLogger.Log($"[RA Service] No match found for hash: {hash}");

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
            _debugLogger.Log($"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");
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
                _debugLogger.Log($"[RA Service] Cleaned up temporary extraction folder: {tempExtractionPath}");
            }
        }
    }

    /// <summary>
    /// Opens the cover image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="systemManager">The system manager for the selected system.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenCover(string systemName, string fileNameWithoutExtension, SystemManager.SystemManager systemManager, MainWindow mainWindow, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the title snapshot image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenTitleSnapshot(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the gameplay snapshot image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenGameplaySnapshot(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the cart image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenCart(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Plays the video file for the specified game using the default media player.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task PlayVideo(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the PDF manual for the specified game using the default PDF viewer.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task OpenManual(string systemName, string fileNameWithoutExtension, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the PDF walkthrough for the specified game using the default PDF viewer.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task OpenWalkthrough(string systemName, string fileNameWithoutExtension, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the cabinet image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenCabinet(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the flyer image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenFlyer(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Opens the PCB (printed circuit board) image for the specified game in an image viewer window.
    /// </summary>
    /// <param name="systemName">The name of the system the game belongs to.</param>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public Task OpenPcb(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox)
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

    /// <summary>
    /// Launches the specified game, waits for its window to appear, captures a screenshot, and saves it as the game's cover image.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="settings">The application settings.</param>
    /// <param name="button">The game button whose image should be updated, or null.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="gamePadController">The game pad controller.</param>
    /// <param name="gameLauncher">The game launcher service.</param>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="loadingStateProvider">The loading state provider for showing/hiding overlays.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task TakeScreenshotOfSelectedWindowAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager settings, Button button, MainWindow mainWindow, GamePadController gamePadController, GameLauncher.GameLauncher gameLauncher, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
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
            _debugLogger.Log($"[Screenshot] Initial window count: {initialCount}");

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
                    _debugLogger.Log($"[Screenshot] New window detected. Current count: {currentWindows.Count} (initial: {initialCount})");
                    newWindowDetected = true;
                    break;
                }

                // Optional: Log progress every few polls
                if (stopwatch.Elapsed.TotalSeconds % 5 < pollInterval.TotalMilliseconds / 1000.0)
                {
                    _debugLogger.Log($"[Screenshot] Polling... Elapsed: {stopwatch.Elapsed.TotalSeconds:F1}s / {maxWaitTime.TotalSeconds}s");
                }
            }

            stopwatch.Stop();

            if (!newWindowDetected)
            {
                // Timeout - no new windows appeared
                _debugLogger.Log($"[Screenshot] Timeout after {stopwatch.Elapsed.TotalSeconds:F1}s. No new windows detected.");
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
                    _debugLogger.Log("Cannot take a screenshot of a minimized window.");

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

    /// <summary>
    /// Deletes the specified game file from disk and reloads the game list.
    /// </summary>
    /// <param name="filePath">The full path to the game file to delete.</param>
    /// <param name="fileNameWithExtension">The file name of the game with its extension.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("DeletingGame") ?? "Deleting game...");
        if (File.Exists(filePath))
        {
            try
            {
                await DeleteFiles.TryDeleteFileAsync(filePath);

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

    /// <summary>
    /// Deletes the cover image for the specified game and reloads the game list.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The file name of the game without its extension.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="contextSettings">The application settings.</param>
    /// <param name="mainWindow">The main application window.</param>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="logErrors">The service used to log errors.</param>
    /// <param name="findCoverImage">The service used to locate cover images.</param>
    /// <param name="messageBox">The service used to display message boxes to the user.</param>
    public async Task DeleteCoverImageAsync(string fileNameWithoutExtension, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager contextSettings, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IFindCoverImageService findCoverImage, IMessageBoxLibraryService messageBox)
    {
        mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("DeletingCoverImage") ?? "Deleting cover image...");
        var coverPath = findCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager.SystemImageFolder);

        try
        {
            playSoundEffects.PlayTrashSound();

            if ((Path.GetFileNameWithoutExtension(coverPath) == fileNameWithoutExtension) & (Path.GetFileNameWithoutExtension(coverPath) != "default"))
            {
                await DeleteFiles.TryDeleteFileAsync(coverPath);
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