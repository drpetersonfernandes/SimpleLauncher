namespace SimpleLauncher.Interfaces;

public interface IGameFilterService
{
    Task<IList<string>> FilterByShowGamesSettingAsync(
        IList<string> files, string selectedSystem, Services.SystemManager.SystemManagerService config);

    Task<IList<string>> FilterByLetterAsync(IList<string> files, string startLetter);

    IList<string> SortByMameDescription(
        IList<string> files, string mameSortOrder, IDictionary<string, string> mameLookup);

    Task<IList<string>> FilterBySearchQueryAsync(
        IList<string> files, string searchQuery, IDictionary<string, string> mameLookup);
}
