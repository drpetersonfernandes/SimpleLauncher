namespace SimpleLauncher.Interfaces;

public interface ISystemManager
{
    string SystemName { get; }
    IList<string> SystemFolders { get; }
    string? PrimarySystemFolder { get; }
    string SystemImageFolder { get; }
    IList<string> FileFormatsToSearch { get; }
    bool ExtractFileBeforeLaunch { get; }
    IList<string> FileFormatsToLaunch { get; }
    IReadOnlyList<IEmulator> Emulators { get; }
    bool GroupByFolder { get; }
    bool DisableRecursiveSearch { get; }
}
