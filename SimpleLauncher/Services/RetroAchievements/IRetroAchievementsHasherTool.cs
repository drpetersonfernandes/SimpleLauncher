using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.RetroAchievements.Models;

namespace SimpleLauncher.Services.RetroAchievements;

public interface IRetroAchievementsHasherTool
{
    bool IsSystemSupportedForHashing(string systemName);
    Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, List<string> fileFormatsToLaunch, ILoadingState loadingState, ILogErrors logErrors);
}
