namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Provides methods to clean up trash and temporary files within the SimpleLauncher folder.
/// </summary>
public interface ICleanSimpleLauncherFolderService
{
    /// <summary>
    ///     Removes trash files and folders from the SimpleLauncher directory.
    /// </summary>
    void CleanupTrash();

    /// <summary>
    ///     Removes temporary files from the SimpleLauncher directory.
    /// </summary>
    void CleanupTempFiles();
}