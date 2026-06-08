using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.Models;

public class LaunchContext
{
    public string FilePath { get; set; }
    public string ResolvedFilePath { get; set; }
    public string EmulatorName { get; set; }
    public string SystemName { get; set; }
    public SystemManager.SystemManager SystemManager { get; set; }
    public Emulator EmulatorManager { get; set; }
    public string Parameters { get; set; }
    public Core.Services.SettingsManager.SettingsManager Settings { get; set; }
    public IWindowContext WindowContext { get; set; }
    public ILoadingState LoadingState { get; set; }
}