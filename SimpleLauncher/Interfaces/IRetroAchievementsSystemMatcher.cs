namespace SimpleLauncher.Interfaces;

public interface IRetroAchievementsSystemMatcher
{
    string GetBestMatchSystemName(string inputSystemName);
    bool IsOfficialSystemName(string systemName);
    IList<string> GetSupportedSystemNames();
    int GetSystemId(string inputSystemName);
    string? GetExactAliasMatch(string inputSystemName);
    bool IsSystemInMappings(string systemName);
}
