using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.FindCoverImage;

public class FindCoverImage(IConfiguration configuration, ILogErrors logErrors) : IFindCoverImage
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogErrors _logErrors = logErrors;
    private static readonly string GlobalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");

    private const double PrefixScale = 0.1;
    private const int MaxPrefixLength = 4;

    public string FindCoverImagePath(string fileNameWithoutExtension, string systemName, SystemManager.SystemManager systemManager, SettingsManager.SettingsManager settings)
    {
        var applicationPath = AppDomain.CurrentDomain.BaseDirectory;
        var imageExtensions = _configuration.GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        string systemImageFolder;
        if (string.IsNullOrEmpty(systemManager.SystemImageFolder))
        {
            systemImageFolder = Path.Combine(applicationPath, "images", systemName ?? string.Empty);
        }
        else
        {
            systemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemManager.SystemImageFolder);
        }

        if (!string.IsNullOrEmpty(systemImageFolder) && Directory.Exists(PathHelper.GetLongPath(systemImageFolder)))
        {
            {
                foreach (var ext in imageExtensions)
                {
                    var imagePath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}{ext}");
                    if (File.Exists(PathHelper.GetLongPath(imagePath)))
                        return imagePath;
                }

                var enableFuzzyMatching = false;
                var similarityThreshold = 0.8;

                if (settings != null)
                {
                    enableFuzzyMatching = settings.EnableFuzzyMatching;
                    similarityThreshold = settings.FuzzyMatchingThreshold;
                }
                else
                {
                    _logErrors.LogAndForget(null, "SettingsManager was null in FindCoverImage. Using default fuzzy matching settings.");
                }

                if (enableFuzzyMatching)
                {
                    var filesInImageFolder = Directory.EnumerateFiles(systemImageFolder)
                        .Where(f => imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    string bestMatchPath = null;
                    double highestSimilarity = 0;
                    var lowerRomName = fileNameWithoutExtension.ToLowerInvariant();

                    foreach (var filePathInFolder in filesInImageFolder)
                    {
                        var fileWithoutExt = Path.GetFileNameWithoutExtension(filePathInFolder);
                        if (string.IsNullOrEmpty(fileWithoutExt)) continue;

                        var lowerFileName = fileWithoutExt.ToLowerInvariant();
                        var similarity = CalculateJaroWinklerSimilarity(lowerRomName, lowerFileName);

                        if (!(similarity > highestSimilarity)) continue;

                        highestSimilarity = similarity;
                        bestMatchPath = filePathInFolder;
                    }

                    if (bestMatchPath != null && highestSimilarity >= similarityThreshold)
                    {
                        return bestMatchPath;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(systemImageFolder)) return GlobalDefaultImagePath;

        var defaultSystemImagePath = Path.Combine(systemImageFolder, "default.png");
        if (File.Exists(PathHelper.GetLongPath(defaultSystemImagePath)))
        {
            return defaultSystemImagePath;
        }

        return GlobalDefaultImagePath;
    }

    public double CalculateJaroWinklerSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return string.IsNullOrEmpty(s1) == string.IsNullOrEmpty(s2) ? 1.0 : 0.0;
        }

        s1 = s1.ToLowerInvariant();
        s2 = s2.ToLowerInvariant();

        var len1 = s1.Length;
        var len2 = s2.Length;

        var matchDistance = Math.Max(len1, len2) / 2 - 1;
        var matches = 0;
        var s1Matches = new bool[len1];
        var s2Matches = new bool[len2];

        for (var i = 0; i < len1; i++)
        {
            var start = Math.Max(0, i - matchDistance);
            var end = Math.Min(len2 - 1, i + matchDistance);

            for (var j = start; j <= end; j++)
            {
                if (s2Matches[j] || s1[i] != s2[j]) continue;

                s1Matches[i] = true;
                s2Matches[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0) return 0.0;

        var k = 0;
        var transpositions = 0;
        for (var i = 0; i < len1; i++)
        {
            if (!s1Matches[i]) continue;

            while (!s2Matches[k])
            {
                k++;
            }

            if (s1[i] != s2[k])
            {
                transpositions++;
            }

            k++;
        }

        var jaroDistance = ((double)matches / len1 + (double)matches / len2 + (matches - transpositions / 2.0) / matches) / 3.0;

        var prefix = 0;
        for (var i = 0; i < Math.Min(MaxPrefixLength, Math.Min(len1, len2)); i++)
        {
            if (s1[i] == s2[i])
            {
                prefix++;
            }
            else
            {
                break;
            }
        }

        var jaroWinklerDistance = jaroDistance + prefix * PrefixScale * (1 - jaroDistance);

        return jaroWinklerDistance;
    }
}
