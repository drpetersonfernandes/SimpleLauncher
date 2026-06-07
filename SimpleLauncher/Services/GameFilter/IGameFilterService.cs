namespace SimpleLauncher.Services.GameFilter;

public interface IGameFilterService
{
    Task<List<string>> FilterByShowGamesSettingAsync(
        List<string> files, string selectedSystem, SystemManager.SystemManager config);

    Task<List<string>> FilterByLetterAsync(List<string> files, string startLetter);

    List<string> SortByMameDescription(
        List<string> files, string mameSortOrder, Dictionary<string, string> mameLookup);

    Task<List<string>> FilterBySearchQueryAsync(
        List<string> files, string searchQuery, Dictionary<string, string> mameLookup);
}