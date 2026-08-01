using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IRetroAchievementsHasherTool
{
    bool IsSystemSupportedForHashing(string systemName);
    Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, IList<string> fileFormatsToLaunch, ILoadingState loadingState, ILogger logErrors);
}
