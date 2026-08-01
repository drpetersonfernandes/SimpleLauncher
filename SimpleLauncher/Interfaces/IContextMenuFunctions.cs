using System.Windows.Controls;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides context menu operations for game items such as favorites, media browsing, and deletion.
/// </summary>
public interface IContextMenuFunctions
{
    /// <summary>
    /// Adds the specified game to the user's favorites list.
    /// </summary>
    Task AddToFavoritesAsync(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Removes the specified game from the user's favorites list.
    /// </summary>
    Task RemoveFromFavoritesAsync(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens a video link for the specified game in the default browser.
    /// </summary>
    Task OpenVideoLinkAsync(string systemName, string fileNameWithoutExtension, IEnumerable<MameManagerService> machines, SettingsManagerService settings, MainWindow mainWindow, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens an informational link for the specified game in the default browser.
    /// </summary>
    Task OpenInfoLinkAsync(string systemName, string fileNameWithoutExtension, IEnumerable<MameManagerService> machines, SettingsManagerService settings, MainWindow mainWindow, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the ROM history window for the specified game.
    /// </summary>
    Task OpenRomHistoryWindowAsync(string systemName, string fileNameWithoutExtension, IEnumerable<MameManagerService> machines, MainWindow mainWindow, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the RetroAchievements window for the specified game.
    /// </summary>
    Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManagerService systemManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the cover image for the specified game.
    /// </summary>
    Task OpenCoverAsync(string systemName, string fileNameWithoutExtension, SystemManagerService systemManager, MainWindow mainWindow, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the title snapshot image for the specified game.
    /// </summary>
    Task OpenTitleSnapshotAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the gameplay snapshot image for the specified game.
    /// </summary>
    Task OpenGameplaySnapshotAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the cartridge image for the specified game.
    /// </summary>
    Task OpenCartAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Plays the video for the specified game.
    /// </summary>
    Task PlayVideoAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the game manual document.
    /// </summary>
    Task OpenManualAsync(string systemName, string fileNameWithoutExtension, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the game walkthrough document.
    /// </summary>
    Task OpenWalkthroughAsync(string systemName, string fileNameWithoutExtension, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the cabinet image for the specified arcade game.
    /// </summary>
    Task OpenCabinetAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the flyer image for the specified arcade game.
    /// </summary>
    Task OpenFlyerAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Opens the PCB (printed circuit board) image for the specified arcade game.
    /// </summary>
    Task OpenPcbAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Takes a screenshot of the selected emulator window.
    /// </summary>
    Task TakeScreenshotOfSelectedWindowAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManagerService selectedSystemManager, SettingsManagerService settings, Button? button, MainWindow mainWindow, GamePadController gamePadController, GameLauncherService gameLauncher, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Deletes the specified game file from disk.
    /// </summary>
    Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Deletes the cover image associated with the specified game.
    /// </summary>
    Task DeleteCoverImageAsync(string fileNameWithoutExtension, string selectedSystemName, SystemManagerService selectedSystemManager, SettingsManagerService contextSettings, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IFindCoverImageService findCoverImage, IMessageBoxLibraryService messageBox);
}
