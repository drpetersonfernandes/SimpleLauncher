namespace SimpleLauncher.Core.Models;

/// <summary>
/// Describes a system whose game folders should be scanned for RetroAchievements hashes.
/// </summary>
public class RaHashScanTarget
{
    /// <summary>
    /// Gets or sets the name of the system to scan.
    /// </summary>
    public string SystemName { get; set; } = "";

    /// <summary>
    /// Gets or sets the list of configured system folders (relative or absolute).
    /// </summary>
    public IList<string> SystemFolders { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of file extensions to search for.
    /// </summary>
    public IList<string> FileFormatsToSearch { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of file extensions to look for inside compressed files
    /// (used to extract the ROM before hashing).
    /// </summary>
    public IList<string> FileFormatsToLaunch { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether subfolders should be skipped during the scan.
    /// </summary>
    public bool DisableRecursiveSearch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the system groups game entries by folder.
    /// </summary>
    public bool GroupByFolder { get; set; }
}