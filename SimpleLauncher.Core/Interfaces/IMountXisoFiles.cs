using SimpleLauncher.Core.Services.GameLauncher.MountFiles;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Mounts original Xbox ISO (XISO) images using SimpleXisoDrive.exe and the Dokan filesystem driver.
/// </summary>
public interface IMountXisoFiles
{
    /// <summary>
    /// Mounts an XISO file and returns a disposable drive handle with the mounted default.xbe path.
    /// </summary>
    /// <param name="resolvedIsoFilePath">The full path to the XISO file to mount.</param>
    /// <param name="logPath">The full path to the log file, if available.</param>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    /// <returns>A task representing the asynchronous operation, resulting in a <see cref="MountXisoDrive"/> with the mounted default.xbe path.</returns>
    Task<MountXisoDrive> MountAsync(string resolvedIsoFilePath, string? logPath, ILogger logErrors,
        IMessageBoxLibraryService messageBox);
}