using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Mounts ZIP archives containing disc-based games using SimpleZipDrive and the Dokan filesystem driver.
/// </summary>
public interface IMountZipFiles
{
    /// <summary>
    /// Gets the configured mount drive root path (e.g., "Z:\").
    /// </summary>
    string ConfiguredMountDriveRoot { get; }

    /// <summary>
    /// Mounts a ZIP archive and launches the EBOOT.BIN file found within it using the specified emulator.
    /// </summary>
    /// <param name="resolvedZipFilePath">The full path to the ZIP archive to mount.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="selectedEmulatorManager">The emulator configuration for the selected emulator.</param>
    /// <param name="rawEmulatorParameters">The raw emulator parameters to use when launching.</param>
    /// <param name="windowContext">The window context used to launch the game.</param>
    /// <param name="logPath">The full path to the log file, if available.</param>
    /// <param name="gameLauncher">The game launcher service.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MountZipFileAndLoadEbootBinAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string? logPath, ILauncherService gameLauncher, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Mounts a ZIP archive and searches for a nested file to launch using the specified emulator.
    /// </summary>
    /// <param name="resolvedZipFilePath">The full path to the ZIP archive to mount.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="selectedEmulatorManager">The emulator configuration for the selected emulator.</param>
    /// <param name="rawEmulatorParameters">The raw emulator parameters to use when launching.</param>
    /// <param name="windowContext">The window context used to launch the game.</param>
    /// <param name="logPath">The full path to the log file, if available.</param>
    /// <param name="gameLauncher">The game launcher service.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MountZipFileAndSearchForFileToLoadAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string? logPath, ILauncherService gameLauncher, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Mounts a ZIP archive and launches ScummVM with the mounted game path.
    /// </summary>
    /// <param name="resolvedZipFilePath">The full path to the ZIP archive to mount.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="selectedEmulatorManager">The emulator configuration for the selected emulator.</param>
    /// <param name="selectedEmulatorParameters">The emulator parameters to use when launching.</param>
    /// <param name="logPath">The full path to the log file, if available.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MountZipFileAndLoadWithScummVmAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string selectedEmulatorParameters, string? logPath, ILogger logErrors, IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Terminates all running SimpleZipDrive processes to ensure clean unmounting.
    /// </summary>
    /// <param name="logErrors">The error logger.</param>
    void KillAllSimpleZipDriveProcesses(ILogger logErrors);
}
