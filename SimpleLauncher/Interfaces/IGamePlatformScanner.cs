using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameScan;

namespace SimpleLauncher.Interfaces;

public interface IGamePlatformScanner
{
    Task ScanAsync(GameScannerService gameScannerService, ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames);
}
