using System.Windows.Controls;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.SystemManager;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SystemManager;
using ILoadingState = SimpleLauncher.Core.Services.LoadingInterface.ILoadingState;

namespace SimpleLauncher.Models;

/// <summary>
/// Contextual information passed to right-click menu handlers for game buttons.
/// </summary>
public class RightClickContext(
    string filePath,
    string fileNameWithExtension,
    string fileNameWithoutExtension,
    string selectedSystemName,
    SystemManager selectedSystemManager,
    List<Services.MameManager.MameManager> machines,
    FavoritesManager favoritesManager,
    Services.SettingsManager.SettingsManager settings,
    ComboBox emulatorComboBox,
    Favorite selectedFavorite,
    Emulator emulatorManager,
    WrapPanel gameFileGrid,
    Button button,
    MainWindow mainWindow,
    GamePadController gamePadController,
    Action onFavoriteRemoved = null,
    Services.GameLauncher.GameLauncher gameLauncher = null,
    PlaySoundEffects playSoundEffects = null,
    ILoadingState loadingStateProvider = null)
{
    /// <summary>Gets the full file path of the game ROM.</summary>
    public string FilePath { get; } = filePath;

    /// <summary>Gets the file name with extension.</summary>
    public string FileNameWithExtension { get; } = fileNameWithExtension;

    /// <summary>Gets the file name without extension.</summary>
    public string FileNameWithoutExtension { get; } = fileNameWithoutExtension;

    /// <summary>Gets the name of the selected system.</summary>
    public string SelectedSystemName { get; } = selectedSystemName;

    /// <summary>Gets the selected system manager instance.</summary>
    public SystemManager SelectedSystemManager { get; } = selectedSystemManager;

    /// <summary>Gets the list of MAME machine entries.</summary>
    public List<Services.MameManager.MameManager> Machines { get; } = machines;

    /// <summary>Gets the favorites manager instance.</summary>
    public FavoritesManager FavoritesManager { get; } = favoritesManager;

    /// <summary>Gets the application settings manager.</summary>
    public Services.SettingsManager.SettingsManager Settings { get; } = settings;

    /// <summary>Gets the emulator combo box control.</summary>
    public ComboBox EmulatorComboBox { get; } = emulatorComboBox;

    /// <summary>Gets the selected favorite entry, if any.</summary>
    public Favorite Favorite { get; } = selectedFavorite;

    /// <summary>Gets the emulator configuration instance.</summary>
    public Emulator Emulator { get; } = emulatorManager;

    /// <summary>Gets the game file grid panel.</summary>
    public WrapPanel GameFileGrid { get; } = gameFileGrid;

    /// <summary>Gets or sets the button that was right-clicked.</summary>
    public Button Button { get; set; } = button;

    /// <summary>Gets the main application window.</summary>
    public MainWindow MainWindow { get; } = mainWindow;

    /// <summary>Gets the game pad controller instance.</summary>
    public GamePadController GamePadController { get; } = gamePadController;

    /// <summary>Gets the callback invoked when a favorite is removed.</summary>
    public Action OnFavoriteRemoved { get; } = onFavoriteRemoved;

    /// <summary>Gets the game launcher service, if available.</summary>
    public Services.GameLauncher.GameLauncher GameLauncher { get; } = gameLauncher;

    /// <summary>Gets the sound effects service, if available.</summary>
    public PlaySoundEffects PlaySoundEffects { get; } = playSoundEffects;

    /// <summary>Gets the loading state provider for overlay display.</summary>
    public ILoadingState LoadingStateProvider { get; } = loadingStateProvider ?? mainWindow;
}