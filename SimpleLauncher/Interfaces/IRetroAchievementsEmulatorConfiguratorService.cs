namespace SimpleLauncher.Interfaces;

public interface IRetroAchievementsEmulatorConfiguratorService
{
    bool ConfigureRetroArch(string exePath, string username, string password);
    bool ConfigurePcsx2(string exePath, string username, string token);
    bool ConfigureDuckStation(string exePath, string username, string token);
    bool ConfigurePpspp(string exePath, string username, string token);
    bool ConfigureDolphin(string exePath, string username, string token);
    bool ConfigureFlycast(string exePath, string username, string token);
    bool ConfigureBizHawk(string exePath, string username, string token);
}
