using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.SystemManager;

public class SystemManagerConfig : ISystemManager
{
    public string SystemName { get; init; }
    public List<string> SystemFolders { get; init; }
    public string PrimarySystemFolder => SystemFolders?.FirstOrDefault();
    public string SystemImageFolder { get; init; }
    public List<string> FileFormatsToSearch { get; init; }
    public bool ExtractFileBeforeLaunch { get; init; }
    public List<string> FileFormatsToLaunch { get; init; }
    public List<Emulator> Emulators { get; init; }
    public bool GroupByFolder { get; init; }
    public bool DisableRecursiveSearch { get; init; }
}
