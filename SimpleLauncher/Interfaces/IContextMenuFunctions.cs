using System.Windows.Controls;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IContextMenuFunctions
{
    Task AddToFavoritesAsync(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task RemoveFromFavoritesAsync(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenVideoLinkAsync(string systemName, string fileNameWithoutExtension, IEnumerable<MameManagerService> machines, SettingsManagerService settings, MainWindow mainWindow, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenInfoLinkAsync(string systemName, string fileNameWithoutExtension, IEnumerable<MameManagerService> machines, SettingsManagerService settings, MainWindow mainWindow, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenRomHistoryWindowAsync(string systemName, string fileNameWithoutExtension, IEnumerable<MameManagerService> machines, MainWindow mainWindow, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManagerService systemManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenCoverAsync(string systemName, string fileNameWithoutExtension, SystemManagerService systemManager, MainWindow mainWindow, IMessageBoxLibraryService messageBox);
    Task OpenTitleSnapshotAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenGameplaySnapshotAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenCartAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task PlayVideoAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenManualAsync(string systemName, string fileNameWithoutExtension, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenWalkthroughAsync(string systemName, string fileNameWithoutExtension, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task OpenCabinetAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenFlyerAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenPcbAsync(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task TakeScreenshotOfSelectedWindowAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManagerService selectedSystemManager, SettingsManagerService settings, Button? button, MainWindow mainWindow, GamePadController gamePadController, GameLauncherService gameLauncher, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task DeleteCoverImageAsync(string fileNameWithoutExtension, string selectedSystemName, SystemManagerService selectedSystemManager, SettingsManagerService contextSettings, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogger logErrors, IFindCoverImageService findCoverImage, IMessageBoxLibraryService messageBox);
}
