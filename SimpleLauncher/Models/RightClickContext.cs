using System.Collections.Generic;
using System.Windows.Controls;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Models;

public class RightClickContext(
    string filePath,
    string fileNameWithExtension,
    string fileNameWithoutExtension,
    string selectedSystemName,
    SystemManager selectedSystemManager,
    List<MameManager> machines,
    Dictionary<string, string> mameLookup,
    FavoritesManager favoritesManager,
    SettingsManager settings,
    ComboBox emulatorComboBox,
    Favorite selectedFavorite,
    SystemManager.Emulator emulatorManager,
    MainWindow mainWindow)
{
    public string FilePath { get; } = filePath;
    public string FileNameWithExtension { get; } = fileNameWithExtension;
    public string FileNameWithoutExtension { get; } = fileNameWithoutExtension;
    public string SelectedSystemName { get; } = selectedSystemName;
    public SystemManager SelectedSystemManager { get; } = selectedSystemManager;
    public List<MameManager> Machines { get; } = machines;
    public Dictionary<string, string> MameLookup { get; } = mameLookup;
    public FavoritesManager FavoritesManager { get; } = favoritesManager;
    public SettingsManager Settings { get; } = settings;
    public ComboBox EmulatorComboBox { get; } = emulatorComboBox;
    public Favorite Favorite { get; } = selectedFavorite;
    public SystemManager.Emulator Emulator { get; } = emulatorManager;
    public MainWindow MainWindow { get; } = mainWindow;
}