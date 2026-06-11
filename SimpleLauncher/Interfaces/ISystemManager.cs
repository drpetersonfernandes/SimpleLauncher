namespace SimpleLauncher.Interfaces;

public interface ISystemManager
{
    string SystemName { get; }
    List<string> SystemFolders { get; }
    string PrimarySystemFolder { get; }
    string SystemImageFolder { get; }
    List<string> FileFormatsToSearch { get; }
    bool ExtractFileBeforeLaunch { get; }
    List<string> FileFormatsToLaunch { get; }
    IReadOnlyList<IEmulator> Emulators { get; }
    bool GroupByFolder { get; }
    bool DisableRecursiveSearch { get; }
}
