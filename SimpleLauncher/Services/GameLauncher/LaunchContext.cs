using SimpleLauncher.Services.LoadingInterface;

namespace SimpleLauncher.Services.GameLauncher;

public class LaunchContext
{
    public string FilePath { get; set; }
    public string ResolvedFilePath { get; set; }
    public string EmulatorName { get; set; }
    public string SystemName { get; set; }
    public SystemManager.SystemManager SystemManager { get; set; }
    public SystemManager.SystemManager.Emulator EmulatorManager { get; set; }
    public string Parameters { get; set; }
    public SettingsManager.SettingsManager Settings { get; set; }
    public MainWindow MainWindow { get; set; }
    public ILoadingState LoadingState { get; set; }
}