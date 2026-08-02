namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to check file lock status.
/// </summary>
public interface IFileLockService
{
    /// <summary>
    /// Determines whether the specified file is currently locked by another process.
    /// </summary>
    /// <param name="filePath">The path of the file to check.</param>
    /// <returns>True if the file is locked; otherwise, false.</returns>
    bool IsFileLocked(string filePath);
}
