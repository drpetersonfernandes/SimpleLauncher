namespace SimpleLauncher.Models;

/// <summary>
/// Holds the results of a system configuration validation check.
/// </summary>
public class SystemValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the overall configuration is valid.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets the list of user-friendly error messages.
    /// </summary>
    public IList<string> ErrorMessages { get; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the system folder path(s) are valid.
    /// </summary>
    public bool AreSystemFoldersValid { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the system image folder path is valid.
    /// </summary>
    public bool IsSystemImageFolderValid { get; set; } = true;

    /// <summary>
    /// Gets the list of invalid emulator location paths.
    /// </summary>
    public IList<string> InvalidEmulatorLocations { get; } = new List<string>();
}
