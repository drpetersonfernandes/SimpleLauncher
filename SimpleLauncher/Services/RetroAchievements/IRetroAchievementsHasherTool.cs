using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.RetroAchievements.Models;

namespace SimpleLauncher.Services.RetroAchievements;

public interface IRetroAchievementsHasherTool
{
    bool IsSystemSupportedForHashing(string systemName);
    Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, List<string> fileFormatsToLaunch, ILoadingState loadingState, ILogErrors logErrors);
}
