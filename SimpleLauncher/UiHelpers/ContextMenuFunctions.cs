using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            // Load existing favorites
            var favorites = FavoritesManager.LoadFavorites();

            // Find the favorite to remove
            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (favoriteToRemove == null)
            {
                return;
            }

            favorites.FavoriteList.Remove(favoriteToRemove);

            // Save the updated favorites list
            favoritesManager.FavoriteList = favorites.FavoriteList;
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

    [SuppressMessage("ReSharper", "RedundantEmptySwitchSection")]
    public static async Task OpenRetroAchievementsWindow(string filePath, string fileNameWithoutExtension, SystemManager systemManager, MainWindow mainWindow)
    {
        string tempExtractionPath = null;

        try
        {
            DebugLogger.Log($"[RA Service] Original system name: {systemManager.SystemName}");
            var systemName = RetroAchievementsSystemMatcher.GetBestMatchSystemName(systemManager.SystemName);
            DebugLogger.Log($"[RA Service] Resolved system name: {systemName}");

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            var raManager = App.ServiceProvider.GetRequiredService<RetroAchievementsManager>();

            if (!File.Exists(filePath))
            {
                DebugLogger.Log($"[RA Service] File not found at {filePath}");
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] File not found at {filePath}");
                return;
            }

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                DebugLogger.Log("[RA Service] FileNameWithoutExtension is null or empty.");
                _ = LogErrors.LogErrorAsync(null, "[RA Service] FileNameWithoutExtension is null or empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(systemName))
            {
                DebugLogger.Log("[RA Service] SystemName is null or empty.");
                _ = LogErrors.LogErrorAsync(null, "[RA Service] SystemName is null or empty.");
                return;
            }

            // Local function to handle finding a game by hash and opening the window
            void FindAndOpenAchievementsWindowByHash(string hash)
            {
                if (string.IsNullOrEmpty(hash))
                {
                    DebugLogger.Log("[RA Service] Hash is null or empty.");
                    MessageBoxLibrary.ErrorMessageBox();
                    return;
                }

                var matchedGame = raManager.AllGames.FirstOrDefault(game => game.Hashes.Contains(hash, StringComparer.OrdinalIgnoreCase));

                if (matchedGame != null)
                {
                    DebugLogger.Log($"[RA Service] Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");
                    var achievementsWindow = new RetroAchievementsWindow(matchedGame.Id, fileNameWithoutExtension);
                    achievementsWindow.Show();
                }
                else
                {
                    DebugLogger.Log($"[RA Service] No match found for hash: {hash}");
                    MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                }
            }

            // --- Define System Categories ---
            List<string> systemWithSimpleHash =
            [
                "amstrad cpc", "apple ii", "atari 2600", "atari jaguar", "wonderswan", "colecovision",
                "vectrex", "magnavox odyssey 2", "intellivision", "msx", "game boy", "game boy advance", "game boy color",
                "pokemon mini", "virtual boy", "neo geo pocket", "32x", "game gear", "master system", "genesis/mega drive",
                "sg-1000", "wasm-4", "watara supervision", "mega duck"
            ];

            List<string> systemWithComplexHash =
            [
                "3do interactive multiplayer", "arduboy", "atari jaguar cd", "pc engine cd/turbografx-cd",
                "pc-fx", "gamecube", "nintendo ds", "neo geo cd", "dreamcast", "saturn", "sega cd",
                "playstation", "playstation 2", "playstation portable"
            ];

            List<string> systemWithFileNameHash = ["arcade"];
            List<string> systemWithByteSwappingHash = ["nintendo 64"];
            List<string> systemWithHeaderCheckHash =
            [
                "atari 7800", "atari lynx", "famicom disk system", "nintendo entertainment system", "pc engine/turbografx-16",
                "supergrafx", "super nintendo entertainment system"
            ];

            // --- Determine Hashing Type ---
            string hashCalculationType;
            if (systemWithSimpleHash.Contains(systemName, StringComparer.OrdinalIgnoreCase))
            {
                hashCalculationType = "Simple";
            }
            else if (systemWithComplexHash.Contains(systemName, StringComparer.OrdinalIgnoreCase))
            {
                hashCalculationType = "Complex";
            }
            else if (systemWithFileNameHash.Contains(systemName, StringComparer.OrdinalIgnoreCase))
            {
                hashCalculationType = "HashFileName";
            }
            else if (systemWithByteSwappingHash.Contains(systemName, StringComparer.OrdinalIgnoreCase))
            {
                hashCalculationType = "HashWithByteSwapping";
            }
            else if (systemWithHeaderCheckHash.Contains(systemName, StringComparer.OrdinalIgnoreCase))
            {
                hashCalculationType = "HashWithHeaderCheck";
            }
            else
            {
                hashCalculationType = "None";
            }

            // --- Pre-processing: Extract if necessary ---
            var fileToProcess = filePath; // By default, process the original file
            var isCompressed = fileExtension is ".zip" or ".7z" or ".rar";
            var requiresExtraction = hashCalculationType is "Simple" or "HashWithByteSwapping" or "HashWithHeaderCheck";

            if (isCompressed && requiresExtraction)
            {
                DebugLogger.Log($"[RA Service] Compressed file detected for hashing: {filePath}. Extracting...");
                var extractor = new ExtractCompressedFile();
                tempExtractionPath = await extractor.ExtractWithSevenZipSharpToTempAsync(filePath);

                if (string.IsNullOrEmpty(tempExtractionPath))
                {
                    _ = LogErrors.LogErrorAsync(null, $"[RA Service] Failed to extract archive for hashing: {filePath}");
                    DebugLogger.Log($"[RA Service] Failed to extract archive for hashing: {filePath}");

                    MessageBoxLibrary.ExtractionFailedMessageBox();

                    return;
                }

                string foundRomFile = null;
                if (systemManager.FileFormatsToLaunch is { Count: > 0 })
                {
                    foreach (var format in systemManager.FileFormatsToLaunch)
                    {
                        var searchPattern = $"*.{format.TrimStart('.')}";
                        var files = Directory.GetFiles(tempExtractionPath, searchPattern, SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            foundRomFile = files[0];
                            DebugLogger.Log($"[RA Service] Found file to hash after extraction: {foundRomFile}");
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(foundRomFile))
                {
                    var allExtractedFiles = Directory.GetFiles(tempExtractionPath, "*", SearchOption.AllDirectories);
                    if (allExtractedFiles.Length > 0)
                    {
                        foundRomFile = allExtractedFiles[0];
                        DebugLogger.Log($"[RA Service] No specific launch format file found. Picking first extracted file: {foundRomFile}");
                    }
                }

                if (string.IsNullOrEmpty(foundRomFile))
                {
                    DebugLogger.Log($"[RA Service] Could not find any suitable file to hash after extracting {filePath}.");
                    MessageBoxLibrary.CouldNotFindAFileMessageBox();
                    return;
                }

                fileToProcess = foundRomFile; // Update the path to the extracted file
            }

            // --- Perform Hashing ---
            switch (hashCalculationType)
            {
                case "Simple":
                {
                    var hash = await RetroAchievementsFileHasher.CalculateStandardMd5Async(fileToProcess);
                    FindAndOpenAchievementsWindowByHash(hash);
                    break;
                }

                case "Complex":
                {
                    var matchedGame = raManager.AllGames.FirstOrDefault(game => game.Title.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
                    if (matchedGame != null)
                    {
                        DebugLogger.Log($"[RA Service] Found game match by filename: {matchedGame.Title} (ID: {matchedGame.Id})");
                        var achievementsWindow = new RetroAchievementsWindow(matchedGame.Id, fileNameWithoutExtension);
                        achievementsWindow.Show();
                    }
                    else
                    {
                        DebugLogger.Log($"[RA Service] No filename match found for game: {fileNameWithoutExtension}");
                        MessageBoxLibrary.GameNotSupportedByRetroAchievementsMessageBox();
                    }

                    break;
                }

                case "HashFileName":
                {
                    var hash = RetroAchievementsFileHasher.CalculateFilenameHash(fileToProcess);
                    FindAndOpenAchievementsWindowByHash(hash);
                    DebugLogger.Log($"[RA Service] Calculated hash for filename: {hash}");
                    break;
                }

                case "HashWithByteSwapping":
                {
                    var hash = await RetroAchievementsFileHasher.CalculateHashWithByteSwappingAsync(fileToProcess);
                    FindAndOpenAchievementsWindowByHash(hash);
                    DebugLogger.Log($"[RA Service] Calculated hash for byte swapping: {hash}");
                    break;
                }

                case "HashWithHeaderCheck":
                {
                    string hash = null;
                    switch (systemName.ToLowerInvariant())
                    {
                        case "atari 7800":
                            var header7800 = new byte[] { 0x01, 0x41, 0x54, 0x41, 0x52, 0x49, 0x37, 0x38, 0x30, 0x30 }; // \1ATARI7800
                            hash = await RetroAchievementsFileHasher.CalculateMd5WithHeaderCheckAsync(fileToProcess, 128, header7800);
                            DebugLogger.Log($"[RA Service] Calculated hash with header check for Atari 7800: {hash}");
                            break;
                        case "atari lynx":
                            var headerLynx = "LYNX\0"u8.ToArray(); // LYNX\0
                            hash = await RetroAchievementsFileHasher.CalculateMd5WithHeaderCheckAsync(fileToProcess, 64, headerLynx);
                            DebugLogger.Log($"[RA Service] Calculated hash with header check for Atari Lynx: {hash}");
                            break;
                        case "famicom disk system":
                            var headerFds = new byte[] { 0x46, 0x44, 0x53, 0x1a }; // FDS\1a
                            hash = await RetroAchievementsFileHasher.CalculateMd5WithHeaderCheckAsync(fileToProcess, 16, headerFds);
                            DebugLogger.Log($"[RA Service] Calculated hash with header check for Famicom Disk System: {hash}");
                            break;
                        case "nintendo entertainment system":
                            var headerNes = new byte[] { 0x4E, 0x45, 0x53, 0x1a }; // NES\1a
                            hash = await RetroAchievementsFileHasher.CalculateMd5WithHeaderCheckAsync(fileToProcess, 16, headerNes);
                            DebugLogger.Log($"[RA Service] Calculated hash with header check for Nintendo Entertainment System: {hash}");
                            break;
                        case "pc engine/turbografx-16":
                        case "supergrafx":
                            var fileInfoPc = new FileInfo(fileToProcess);
                            hash = fileInfoPc.Length % 131072 == 512
                                ? await RetroAchievementsFileHasher.CalculateMd5WithOffsetAsync(fileToProcess, 512)
                                : await RetroAchievementsFileHasher.CalculateStandardMd5Async(fileToProcess);
                            DebugLogger.Log($"[RA Service] Calculated hash for PC Engine/TurboGrafx-16/SuperGrafx: {hash}");
                            break;
                        case "super nintendo entertainment system":
                            var fileInfoSnes = new FileInfo(fileToProcess);
                            hash = fileInfoSnes.Length % 8192 == 512
                                ? await RetroAchievementsFileHasher.CalculateMd5WithOffsetAsync(fileToProcess, 512)
                                : await RetroAchievementsFileHasher.CalculateStandardMd5Async(fileToProcess);
                            DebugLogger.Log($"[RA Service] Calculated hash for Super Nintendo/Super Famicom/Satellaview/Sufami Turbo: {hash}");
                            break;
                    }

                    FindAndOpenAchievementsWindowByHash(hash);
                    break;
                }

                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            // Ensure loading indicator is hidden (redundant but safe if caller fails)
            mainWindow.IsLoadingGames = false;

            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");
            DebugLogger.Log($"[RA Service] An unexpected error occurred while processing achievements for {fileNameWithoutExtension}.");

            MessageBoxLibrary.CouldNotOpenAchievementsWindowMessageBox();
        }
        finally
        {
            // Ensure loading indicator is hidden (redundant but safe)
            mainWindow.IsLoadingGames = false;

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

            // Resolve the systemImageFolder using PathHelper
            var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemManager.SystemImageFolder);

            if (string.IsNullOrEmpty(resolvedSystemImageFolder))
            {
                // Fallback to default if resolution fails or path is empty
                resolvedSystemImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemName);
            }

            try
            {
                Directory.CreateDirectory(resolvedSystemImageFolder);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Could not create the system image folder: {resolvedSystemImageFolder}");
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

            // Add a check for invalid dimensions (i.e., a minimized window)
            if (width <= 0 || height <= 0)
            {
                // Notify the user that they can't screenshot a minimized window.
                MessageBoxLibrary.CannotScreenshotMinimizedWindowMessageBox();
                DebugLogger.Log("Cannot take a screenshot of a minimized window.");

                return; // Exit the method gracefully
            }

            var screenshotPath = Path.Combine(resolvedSystemImageFolder, $"{fileNameWithoutExtension}.png");

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
    public static async Task DeleteGame(string filePath, string fileNameWithExtension, MainWindow mainWindow)
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

    public static async Task DeleteCoverImage(
        string fileNameWithoutExtension,
        string selectedSystemName,
        SystemManager selectedSystemManager,
        MainWindow mainWindow)
    {
        var coverPath = FindCoverImageForDeletion.FindCoverImagePath(fileNameWithoutExtension, selectedSystemName, selectedSystemManager);

        try
        {
            PlaySoundEffects.PlayTrashSound();
            DeleteFiles.TryDeleteFile(coverPath);

            await Task.Delay(500);

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