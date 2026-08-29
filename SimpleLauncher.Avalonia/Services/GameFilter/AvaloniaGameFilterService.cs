using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Services.GameFilter;

/// <summary>
///     Encapsulates game-list filtering and sorting logic (letter filter, MAME sort,
///     search query, show-games visibility). Extracted from MainViewModel for testability
///     and reuse, mirroring the WPF GameFilterService.
/// </summary>
public class AvaloniaGameFilterService
{
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IMameDataService _mameData;
    private readonly SettingsManagerService _settings;

    public AvaloniaGameFilterService(
        IFindCoverImageService findCoverImage,
        SettingsManagerService settings,
        IMameDataService mameData)
    {
        _findCoverImage = findCoverImage;
        _settings = settings;
        _mameData = mameData;
    }

    /// <summary>
    ///     Filters the game list by the Show Games setting (ShowAll / ShowWithCover / ShowWithoutCover).
    /// </summary>
    public List<GameCardViewModel> FilterByShowGamesSetting(List<GameCardViewModel> games)
    {
        var showGamesMode = _settings.ShowGames;
        if (showGamesMode is "ShowAll" || games.Count == 0)
            return games;

        return games.Where(g => string.Equals(showGamesMode, "ShowWithCover", StringComparison.Ordinal)
            ? g.HasCover
            : !g.HasCover).ToList();
    }

    /// <summary>
    ///     Filters the game list by a starting letter. "#" matches files starting with a digit.
    ///     Empty or null letter returns the list unfiltered.
    /// </summary>
    public List<GameCardViewModel> FilterByLetter(List<GameCardViewModel> games, string letter)
    {
        if (string.IsNullOrEmpty(letter))
            return games;

        if (string.Equals(letter, "#", StringComparison.Ordinal))
            return games.Where(game =>
            {
                var fileName = Path.GetFileName(game.FilePath);
                return !string.IsNullOrEmpty(fileName) && char.IsDigit(fileName[0]);
            }).ToList();

        return games.Where(game =>
        {
            var fileName = Path.GetFileName(game.FilePath);
            return !string.IsNullOrEmpty(fileName) &&
                   fileName.StartsWith(letter, StringComparison.OrdinalIgnoreCase);
        }).ToList();
    }

    /// <summary>
    ///     Sorts the game list by MAME machine description or file name.
    /// </summary>
    public List<GameCardViewModel> SortByMameOrder(List<GameCardViewModel> games, string mameSortOrder)
    {
        if (string.Equals(mameSortOrder, "MachineDescription", StringComparison.Ordinal))
            return games.OrderBy(game =>
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(game.FilePath);
                return _mameData.Lookup.TryGetValue(fileNameWithoutExtension, out var description) &&
                       !string.IsNullOrWhiteSpace(description)
                    ? description
                    : fileNameWithoutExtension;
            }, StringComparer.OrdinalIgnoreCase).ToList();

        return games.OrderBy(game => Path.GetFileName(game.FilePath), StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    ///     Filters the game list by a search query applied to DisplayTitle.
    /// </summary>
    public List<GameCardViewModel> FilterBySearchQuery(List<GameCardViewModel> games, string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return games;

        return games.Where(g =>
            g.DisplayTitle.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}