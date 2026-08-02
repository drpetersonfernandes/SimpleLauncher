using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Carries all the data needed to launch a game, including file paths, emulator, system, and settings.
/// </summary>
public class LaunchContext
{
    /// <summary>
    /// Gets or sets the path to the game file to launch.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the resolved full path to the game file.
    /// </summary>
    public string ResolvedFilePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the emulator used to launch the game.
    /// </summary>
    public string EmulatorName { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the system the game belongs to.
    /// </summary>
    public string SystemName { get; set; } = "";

    /// <summary>
    /// Gets or sets the system manager service for the selected system.
    /// </summary>
    public ISystemManager? SystemManagerService { get; set; }

    /// <summary>
    /// Gets or sets the emulator manager used to launch the game.
    /// </summary>
    public Emulator? EmulatorManager { get; set; }

    /// <summary>
    /// Gets or sets the command line parameters passed to the emulator.
    /// </summary>
    public string Parameters { get; set; } = "";

    /// <summary>
    /// Gets or sets the settings manager service for the application.
    /// </summary>
    public SettingsManagerService? Settings { get; set; }

    /// <summary>
    /// Gets or sets the window context used for UI interactions during launch.
    /// </summary>
    public IWindowContext? WindowContext { get; set; }

    /// <summary>
    /// Gets or sets the loading state provider used to show loading UI during launch.
    /// </summary>
    public ILoadingState? LoadingState { get; set; }
}
