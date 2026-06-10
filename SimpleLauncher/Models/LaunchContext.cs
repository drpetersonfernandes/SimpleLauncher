using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Models;

public class LaunchContext
{
    public string FilePath { get; set; }
    public string ResolvedFilePath { get; set; }
    public string EmulatorName { get; set; }
    public string SystemName { get; set; }
    public ISystemManager SystemManager { get; set; }
    public Emulator EmulatorManager { get; set; }
    public string Parameters { get; set; }
    public SettingsManager Settings { get; set; }
    public IWindowContext WindowContext { get; set; }
    public ILoadingState LoadingState { get; set; }
}
