using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.Favorites.Models;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.ContextMenu.Models;

public class RightClickContext(
    string filePath,
    string fileNameWithExtension,
    string fileNameWithoutExtension,
    string selectedSystemName,
    SystemManager.SystemManager selectedSystemManager,
    List<MameManager.MameManager> machines,
    FavoritesManager favoritesManager,
    SettingsManager.SettingsManager settings,
    ComboBox emulatorComboBox,
    Favorite selectedFavorite,
    SystemManager.SystemManager.Emulator emulatorManager,
    WrapPanel gameFileGrid,
    Button button,
    MainWindow mainWindow,
    GamePadController gamePadController,
    Action onFavoriteRemoved = null,
    GameLauncher.GameLauncher gameLauncher = null,
    PlaySoundEffects playSoundEffects = null)
{
    public string FilePath { get; } = filePath;
    public string FileNameWithExtension { get; } = fileNameWithExtension;
    public string FileNameWithoutExtension { get; } = fileNameWithoutExtension;
    public string SelectedSystemName { get; } = selectedSystemName;
    public SystemManager.SystemManager SelectedSystemManager { get; } = selectedSystemManager;
    public List<MameManager.MameManager> Machines { get; } = machines;
    public FavoritesManager FavoritesManager { get; } = favoritesManager;
    public SettingsManager.SettingsManager Settings { get; } = settings;
    public ComboBox EmulatorComboBox { get; } = emulatorComboBox;
    public Favorite Favorite { get; } = selectedFavorite;
    public SystemManager.SystemManager.Emulator Emulator { get; } = emulatorManager;
    public WrapPanel GameFileGrid { get; } = gameFileGrid;
    public Button Button { get; set; } = button;
    public MainWindow MainWindow { get; } = mainWindow;
    public GamePadController GamePadController { get; } = gamePadController;
    public Action OnFavoriteRemoved { get; } = onFavoriteRemoved;
    public GameLauncher.GameLauncher GameLauncher { get; } = gameLauncher;
    public PlaySoundEffects PlaySoundEffects { get; } = playSoundEffects;
}