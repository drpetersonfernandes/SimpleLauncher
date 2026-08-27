using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Handles mounting ISO disc images using PowerShell and launching games from the mounted drive.
/// </summary>
public interface IMountIsoFiles
{
    /// <summary>
    /// Mounts an ISO file, locates EBOOT.BIN, and launches it with the specified emulator.
    /// </summary>
    /// <param name="resolvedIsoFilePath">The full path to the ISO file to mount.</param>
    /// <param name="selectedSystemName">The name of the selected system.</param>
    /// <param name="selectedEmulatorName">The name of the selected emulator.</param>
    /// <param name="selectedSystemManager">The system manager for the selected system.</param>
    /// <param name="selectedEmulatorManager">The emulator configuration for the selected emulator.</param>
    /// <param name="rawEmulatorParameters">The raw emulator parameters to use when launching.</param>
    /// <param name="windowContext">The window context used to launch the game.</param>
    /// <param name="logPath">The full path to the log file.</param>
    /// <param name="gameLauncher">The game launcher service.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MountIsoFileAsync(string resolvedIsoFilePath, string selectedSystemName, string selectedEmulatorName,
        ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters,
        IWindowContext windowContext, string logPath, ILauncherService gameLauncher, ILogger logErrors,
        IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Waits for a directory to exist by polling at regular intervals until a timeout is reached.
    /// </summary>
    /// <param name="directoryPath">The directory path to wait for.</param>
    /// <param name="maxWaitTimeMs">Maximum wait time in milliseconds.</param>
    /// <param name="pollIntervalMs">Polling interval in milliseconds.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <returns>True if the directory appeared within the timeout; otherwise, false.</returns>
    Task<bool> WaitForDirectoryToExistAsync(string directoryPath, int maxWaitTimeMs, int pollIntervalMs,
        ILogger logErrors);

    /// <summary>
    /// Mounts an ISO file using a PowerShell command and returns the assigned drive letter.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file to mount.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>The drive letter assigned to the mounted ISO, or null if mounting failed.</returns>
    Task<string?> ExecutePowerShellMountCommandAsync(string isoPath, ILogger logErrors,
        IMessageBoxLibraryService messageBox);

    /// <summary>
    /// Dismounts an ISO file using a PowerShell command.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file to dismount.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecutePowerShellDismountCommandAsync(string isoPath, ILogger logErrors, IMessageBoxLibraryService messageBox);
}