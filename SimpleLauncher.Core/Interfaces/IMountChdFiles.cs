using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Mounts CHD (Compressed Hunks of Data) disc images using CHDMounter and the Dokan filesystem driver.
/// </summary>
public interface IMountChdFiles
{
    /// <summary>
    /// Mounts a CHD file and returns a disposable drive handle with the mounted path and drive letter.
    /// </summary>
    /// <param name="resolvedChdFilePath">The full path to the CHD file to mount.</param>
    /// <param name="consoleAlias">The console alias (e.g. "ps2", "xbox") to use for mounting, or null to auto-select.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation, resulting in a <see cref="MountChdDrive"/> with the mounted path and drive letter.</returns>
    Task<MountChdDrive> MountAsync(string resolvedChdFilePath, string? consoleAlias, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Mounts a CHD file, locates a game file within the mounted drive, launches the emulator, and unmounts on exit.
    /// </summary>
    /// <param name="resolvedChdFilePath">The full path to the CHD file to mount.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="selectedEmulatorManager">The emulator configuration for the selected emulator.</param>
    /// <param name="rawEmulatorParameters">The raw emulator parameters to use when launching.</param>
    /// <param name="windowContext">The window context used to launch the game.</param>
    /// <param name="gameLauncher">The game launcher service.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MountChdFileAndLoadAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, ILauncherService gameLauncher, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Mounts a CHD file with an explicit console alias, locates a game file, launches the emulator, and unmounts on exit.
    /// </summary>
    /// <param name="resolvedChdFilePath">The full path to the CHD file to mount.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="selectedEmulatorManager">The emulator configuration for the selected emulator.</param>
    /// <param name="rawEmulatorParameters">The raw emulator parameters to use when launching.</param>
    /// <param name="windowContext">The window context used to launch the game.</param>
    /// <param name="gameLauncher">The game launcher service.</param>
    /// <param name="consoleAlias">The console alias (e.g. "ps2", "xbox") to use for mounting.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MountChdFileAndLoadWithConsoleAliasAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, ILauncherService gameLauncher, string? consoleAlias, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Determines the CHDMounter console alias for a given system name and emulator name.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="emulatorName">The name of the emulator.</param>
    /// <param name="emulatorLocation">The emulator executable path (used for emulator-specific overrides), or null.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <returns>The console alias for the system, or null if it could not be determined.</returns>
    string? GetConsoleAliasFromSystemName(string systemName, string emulatorName, string? emulatorLocation, ILogger logErrors);

    /// <summary>
    /// Terminates all running CHDMounter processes to ensure clean unmounting.
    /// </summary>
    /// <param name="logErrors">The error logger.</param>
    void KillAllChdMounterProcesses(ILogger logErrors);
}
