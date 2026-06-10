using SimpleLauncher.Services.SystemManager;

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
    List<Emulator> Emulators { get; }
    bool GroupByFolder { get; }
    bool DisableRecursiveSearch { get; }
}
