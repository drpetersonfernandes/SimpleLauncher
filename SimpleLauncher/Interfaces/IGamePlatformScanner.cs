using SimpleLauncher.Services.GameScan;

namespace SimpleLauncher.Interfaces;

public interface IGamePlatformScanner
{
    Task ScanAsync(GameScannerService gameScannerService, ILogger logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames);
}
