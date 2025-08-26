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
    FavoritesManager favoritesManager,
    SettingsManager settings,
    ComboBox emulatorComboBox,
    MainWindow mainWindow)
{
    // File-specific information
    public string FilePath { get; } = filePath;
    public string FileNameWithExtension { get; } = fileNameWithExtension;
    public string FileNameWithoutExtension { get; } = fileNameWithoutExtension;

    // System and Game information
    public string SelectedSystemName { get; } = selectedSystemName;
    public SystemManager SelectedSystemManager { get; } = selectedSystemManager;
    public List<MameManager> Machines { get; } = machines;

    // Core application managers and settings
    public FavoritesManager FavoritesManager { get; } = favoritesManager;
    public SettingsManager Settings { get; } = settings;

    // UI Elements (if absolutely necessary)
    public ComboBox EmulatorComboBox { get; } = emulatorComboBox;
    public MainWindow MainWindow { get; } = mainWindow;
}