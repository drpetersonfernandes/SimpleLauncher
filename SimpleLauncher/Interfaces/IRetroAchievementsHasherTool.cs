using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IRetroAchievementsHasherTool
{
    bool IsSystemSupportedForHashing(string systemName);
    Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, List<string> fileFormatsToLaunch, ILoadingState loadingState, ILogger logErrors);
}
