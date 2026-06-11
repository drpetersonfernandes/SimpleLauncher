using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.FindCoverImage;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public class FindCoverImageService : IFindCoverImageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly bool _enableFuzzyMatching;
    private readonly double _fuzzyMatchingThreshold;
    private static readonly string GlobalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");

    private const double PrefixScale = 0.1;
    private const int MaxPrefixLength = 4;

    public FindCoverImageService(IConfiguration configuration, ILogErrors logErrors, bool enableFuzzyMatching = false, double fuzzyMatchingThreshold = 0.8)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _enableFuzzyMatching = enableFuzzyMatching;
        _fuzzyMatchingThreshold = fuzzyMatchingThreshold;
    }

    public string FindCoverImagePath(string fileNameWithoutExtension, string systemName, string systemImageFolder)
    {
        var imageExtensions = _configuration.GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        string resolvedImageFolder;
        if (string.IsNullOrEmpty(systemImageFolder))
        {
            resolvedImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemName ?? "");
        }
        else
        {
            resolvedImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder) ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemName ?? "");
        }

        if (!string.IsNullOrEmpty(resolvedImageFolder) && Directory.Exists(resolvedImageFolder))
        {
            // Exact match
            foreach (var ext in imageExtensions)
            {
                var imagePath = Path.Combine(resolvedImageFolder, $"{fileNameWithoutExtension}{ext}");
                if (File.Exists(imagePath))
                    return imagePath;
            }

            // Fuzzy match
            if (_enableFuzzyMatching)
            {
                var filesInImageFolder = Directory.EnumerateFiles(resolvedImageFolder)
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

                    if (similarity > highestSimilarity)
                    {
                        highestSimilarity = similarity;
                        bestMatchPath = filePathInFolder;
                    }
                }

                if (bestMatchPath != null && highestSimilarity >= _fuzzyMatchingThreshold)
                {
                    return bestMatchPath;
                }
            }
        }

        // Fall back to default images
        if (string.IsNullOrEmpty(resolvedImageFolder)) return GlobalDefaultImagePath;

        var defaultSystemImagePath = Path.Combine(resolvedImageFolder, "default.png");
        if (File.Exists(defaultSystemImagePath))
            return defaultSystemImagePath;

        return GlobalDefaultImagePath;
    }

    public static double CalculateJaroWinklerSimilarity(string s1, string s2)
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
            else break;
        }

        return jaroDistance + prefix * PrefixScale * (1 - jaroDistance);
    }
}
