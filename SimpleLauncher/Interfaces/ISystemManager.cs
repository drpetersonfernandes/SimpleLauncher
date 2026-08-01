namespace SimpleLauncher.Interfaces;

/// <summary>
/// Represents a system (console/platform) configuration including ROM folders, file formats, and emulators.
/// </summary>
public interface ISystemManager
{
    /// <summary>
    /// Gets the name of the system (e.g., "NES", "SNES").
    /// </summary>
    string SystemName { get; }

    /// <summary>
    /// Gets the list of ROM folder paths for this system.
    /// </summary>
    IList<string> SystemFolders { get; }

    /// <summary>
    /// Gets the first (primary) system folder path.
    /// </summary>
    string? PrimarySystemFolder { get; }

    /// <summary>
    /// Gets the path to the folder containing system images.
    /// </summary>
    string SystemImageFolder { get; }

    /// <summary>
    /// Gets the list of file extensions to search for in ROM folders.
    /// </summary>
    IList<string> FileFormatsToSearch { get; }

    /// <summary>
    /// Gets whether compressed files should be extracted before launching.
    /// </summary>
    bool ExtractFileBeforeLaunch { get; }

    /// <summary>
    /// Gets the list of file extensions that can be launched directly.
    /// </summary>
    IList<string> FileFormatsToLaunch { get; }

    /// <summary>
    /// Gets the list of configured emulators for this system.
    /// </summary>
    IReadOnlyList<IEmulator> Emulators { get; }

    /// <summary>
    /// Gets whether games should be grouped by their parent folder.
    /// </summary>
    bool GroupByFolder { get; }

    /// <summary>
    /// Gets whether recursive folder searching is disabled.
    /// </summary>
    bool DisableRecursiveSearch { get; }
}
