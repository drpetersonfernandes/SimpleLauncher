using System.Collections.Generic;
using System.Windows.Controls;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using System;

namespace SimpleLauncher.Models;

public class RightClickContext(
    string filePath,
    string fileNameWithExtension,
    string fileNameWithoutExtension,
    string selectedSystemName,
    SystemManager selectedSystemManager,
    List<MameManager> machines,
    FavoritesManager favoritesManager,
    SettingsManager settings,
    ComboBox emulatorComboBox,
    Favorite selectedFavorite,
    SystemManager.Emulator emulatorManager,
    WrapPanel gameFileGrid,
    Button button,
    MainWindow mainWindow,
    GamePadController gamePadController,
    Action onFavoriteRemoved = null)
{
    public string FilePath { get; } = filePath;
    public string FileNameWithExtension { get; } = fileNameWithExtension;
    public string FileNameWithoutExtension { get; } = fileNameWithoutExtension;
    public string SelectedSystemName { get; } = selectedSystemName;
    public SystemManager SelectedSystemManager { get; } = selectedSystemManager;
    public List<MameManager> Machines { get; } = machines;
    public FavoritesManager FavoritesManager { get; } = favoritesManager;
    public SettingsManager Settings { get; } = settings;
    public ComboBox EmulatorComboBox { get; } = emulatorComboBox;
    public Favorite Favorite { get; } = selectedFavorite;
    public SystemManager.Emulator Emulator { get; } = emulatorManager;
    public WrapPanel GameFileGrid { get; } = gameFileGrid;
    public Button Button { get; set; } = button;
    public MainWindow MainWindow { get; } = mainWindow;
    public GamePadController GamePadController { get; } = gamePadController;
    public Action OnFavoriteRemoved { get; } = onFavoriteRemoved;
}