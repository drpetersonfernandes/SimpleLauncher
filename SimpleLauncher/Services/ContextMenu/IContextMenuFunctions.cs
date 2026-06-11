using System.Windows.Controls;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.ContextMenu;

public interface IContextMenuFunctions
{
    Task AddToFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task RemoveFromFavorites(string systemName, string fileNameWithExtension, WrapPanel gameFileGrid, FavoritesManager favoritesManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenVideoLink(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, SettingsManager.SettingsManager settings, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenInfoLink(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, SettingsManager.SettingsManager settings, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, IEnumerable<MameManager.MameManager> machines, MainWindow mainWindow, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenRetroAchievementsWindowAsync(string filePath, string fileNameWithoutExtension, SystemManager.SystemManager systemManager, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenCover(string systemName, string fileNameWithoutExtension, SystemManager.SystemManager systemManager, MainWindow mainWindow, IMessageBoxLibraryService messageBox);
    Task OpenTitleSnapshot(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenGameplaySnapshot(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenCart(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task PlayVideo(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenManual(string systemName, string fileNameWithoutExtension, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenWalkthrough(string systemName, string fileNameWithoutExtension, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task OpenCabinet(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenFlyer(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task OpenPcb(string systemName, string fileNameWithoutExtension, IMessageBoxLibraryService messageBox);
    Task TakeScreenshotOfSelectedWindowAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager settings, Button button, MainWindow mainWindow, GamePadController gamePadController, GameLauncher.GameLauncher gameLauncher, PlaySoundEffects playSoundEffects, ILoadingState loadingStateProvider, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task DeleteGameAsync(string filePath, string fileNameWithExtension, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task DeleteCoverImageAsync(string fileNameWithoutExtension, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager contextSettings, MainWindow mainWindow, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IFindCoverImageService findCoverImage, IMessageBoxLibraryService messageBox);
}
