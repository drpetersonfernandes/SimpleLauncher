namespace SimpleLauncher.Interfaces;

public interface IRetroAchievementsSystemMatcher
{
    string GetBestMatchSystemName(string inputSystemName);
    bool IsOfficialSystemName(string systemName);
    List<string> GetSupportedSystemNames();
    int GetSystemId(string inputSystemName);
    string GetExactAliasMatch(string inputSystemName);
    bool IsSystemInMappings(string systemName);
}
