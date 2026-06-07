using System.IO;
using SimpleLauncher.Services.FindCoverImage;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameFilter;

public class GameFilterService : IGameFilterService
{
    private readonly IFindCoverImage _findCoverImage;
    private readonly SettingsManager.SettingsManager _settings;

    public GameFilterService(IFindCoverImage findCoverImage, SettingsManager.SettingsManager settings)
    {
        _findCoverImage = findCoverImage;
        _settings = settings;
    }

    public Task<List<string>> FilterByShowGamesSettingAsync(
        List<string> files, string selectedSystem, SystemManager.SystemManager config)
    {
        if (files.Count == 0 || _settings.ShowGames == "ShowAll")
            return Task.FromResult(files);

        var filteredFiles = new List<string>();

        foreach (var filePath in files)
        {
            var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);
            var imagePath = _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, config, _settings);

            bool isDefaultImage;
            if (string.IsNullOrEmpty(imagePath) || imagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
            {
                isDefaultImage = true;
            }
            else
            {
                var resolvedImagePath = PathHelper.ResolveRelativeToAppDirectory(imagePath);
                isDefaultImage = string.IsNullOrEmpty(resolvedImagePath) ||
                                 !File.Exists(resolvedImagePath) ||
                                 resolvedImagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase);
            }

            switch (_settings.ShowGames)
            {
                case "ShowWithCover" when !isDefaultImage:
                case "ShowWithoutCover" when isDefaultImage:
                    filteredFiles.Add(filePath);
                    break;
            }
        }

        return Task.FromResult(filteredFiles);
    }

    public Task<List<string>> FilterByLetterAsync(List<string> files, string startLetter)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(startLetter))
                return files;

            if (startLetter == "#")
            {
                return files.Where(static file => !string.IsNullOrEmpty(file) &&
                                                  file.Length > 0 &&
                                                  char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }

            return files.Where(file => !string.IsNullOrEmpty(file) &&
                                       Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
        });
    }

    public List<string> SortByMameDescription(
        List<string> files, string mameSortOrder, Dictionary<string, string> mameLookup)
    {
        if (mameSortOrder == "MachineDescription")
        {
            return files.OrderBy(f =>
            {
                var fileName = Path.GetFileNameWithoutExtension(f);
                return mameLookup.TryGetValue(fileName, out var description) && !string.IsNullOrWhiteSpace(description)
                    ? description
                    : fileName;
            }, StringComparer.OrdinalIgnoreCase).ToList();
        }

        return files.OrderBy(static f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).ToList();
    }

    public Task<List<string>> FilterBySearchQueryAsync(
        List<string> files, string searchQuery, Dictionary<string, string> mameLookup)
    {
        var lowerQuery = searchQuery.ToLowerInvariant();
        return Task.Run(() =>
            files.FindAll(file =>
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                if (filenameMatch) return true;

                if (mameLookup != null && mameLookup.TryGetValue(fileName, out var description))
                {
                    return description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }));
    }
}