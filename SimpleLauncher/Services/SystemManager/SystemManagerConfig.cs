using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.SystemManager;

/// <summary>
/// Immutable configuration model for a system (console/platform), implementing ISystemManager for read-only access.
/// </summary>
public class SystemManagerConfig : ISystemManager
{
    /// <summary>Gets the name of the system.</summary>
    public string SystemName { get; init; }

    /// <summary>Gets the list of ROM folder paths for this system.</summary>
    public List<string> SystemFolders { get; init; }

    /// <summary>Gets the first (primary) system folder path.</summary>
    public string PrimarySystemFolder => SystemFolders?.FirstOrDefault();

    /// <summary>Gets the path to the folder containing system images.</summary>
    public string SystemImageFolder { get; init; }

    /// <summary>Gets the list of file extensions to search for in ROM folders.</summary>
    public List<string> FileFormatsToSearch { get; init; }

    /// <summary>Gets whether compressed files should be extracted before launching.</summary>
    public bool ExtractFileBeforeLaunch { get; init; }

    /// <summary>Gets the list of file extensions that can be launched directly.</summary>
    public List<string> FileFormatsToLaunch { get; init; }

    /// <summary>Gets the list of configured emulators for this system.</summary>
    public List<Emulator> Emulators { get; init; }

    IReadOnlyList<IEmulator> ISystemManager.Emulators => Emulators?.Cast<IEmulator>().ToList();

    /// <summary>Gets whether games should be grouped by their parent folder.</summary>
    public bool GroupByFolder { get; init; }

    /// <summary>Gets whether recursive folder searching is disabled.</summary>
    public bool DisableRecursiveSearch { get; init; }
}
