using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides methods to filter and sort game file lists based on various criteria.
/// </summary>
public interface IGameFilterService
{
    /// <summary>
    ///     Asynchronously filters the game file list based on the "show games" setting.
    /// </summary>
    /// <param name="files">The list of game file paths to filter.</param>
    /// <param name="selectedSystem">The name of the selected system.</param>
    /// <param name="config">The system manager configuration.</param>
    /// <returns>The filtered list of game file paths.</returns>
    Task<IList<string>> FilterByShowGamesSettingAsync(
        IList<string> files, string selectedSystem, SystemManagerService config);

    /// <summary>
    ///     Asynchronously filters the game file list to include only files starting with the specified letter.
    /// </summary>
    /// <param name="files">The list of game file paths to filter.</param>
    /// <param name="startLetter">The starting letter to filter by.</param>
    /// <returns>The filtered list of game file paths.</returns>
    Task<IList<string>> FilterByLetterAsync(IList<string> files, string startLetter);

    /// <summary>
    ///     Sorts the game file list by MAME description.
    /// </summary>
    /// <param name="files">The list of game file paths to sort.</param>
    /// <param name="mameSortOrder">The sort order to apply.</param>
    /// <param name="mameLookup">The MAME lookup dictionary mapping ROM names to descriptions.</param>
    /// <returns>The sorted list of game file paths.</returns>
    IList<string> SortByMameDescription(
        IList<string> files, string mameSortOrder, IDictionary<string, string> mameLookup);

    /// <summary>
    ///     Asynchronously filters the game file list based on a search query.
    /// </summary>
    /// <param name="files">The list of game file paths to filter.</param>
    /// <param name="searchQuery">The search query to match against.</param>
    /// <param name="mameLookup">The MAME lookup dictionary mapping ROM names to descriptions.</param>
    /// <returns>The filtered list of game file paths.</returns>
    Task<IList<string>> FilterBySearchQueryAsync(
        IList<string> files, string searchQuery, IDictionary<string, string> mameLookup);
}