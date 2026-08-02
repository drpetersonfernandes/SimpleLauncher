namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to validate directory accessibility.
/// </summary>
public interface IDirectoryValidationService
{
    /// <summary>
    /// Determines whether the specified path is a writable directory.
    /// </summary>
    /// <param name="path">The directory path to check.</param>
    /// <returns>True if the directory exists and is writable; otherwise, false.</returns>
    bool IsWritableDirectory(string path);
}
