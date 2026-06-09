using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Core.Models;

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
