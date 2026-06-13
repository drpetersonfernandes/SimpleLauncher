using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckPaths;

namespace SimpleLauncher.Services.FindCoverImage;

/// <summary>
/// Locates cover image files for games using exact name matching and optional Jaro-Winkler fuzzy matching,
/// falling back to a default image when no match is found.
/// </summary>
public partial class FindCoverImageService : IFindCoverImageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly bool _enableFuzzyMatching;
    private readonly double _fuzzyMatchingThreshold;
    private readonly bool _enableAnnotationStripping;
    private static readonly string GlobalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");

    private const double PrefixScale = 0.1;
    private const int MaxPrefixLength = 4;

    /// <summary>
    /// Initializes a new instance of the FindCoverImageService class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logErrors">The log errors.</param>
    /// <param name="enableFuzzyMatching">The enable fuzzy matching.</param>
    /// <param name="fuzzyMatchingThreshold">The fuzzy matching threshold.</param>
    /// <param name="enableAnnotationStripping">Whether to strip parenthetical annotations before matching.</param>
    public FindCoverImageService(IConfiguration configuration, ILogErrors logErrors, bool enableFuzzyMatching = false, double fuzzyMatchingThreshold = 0.8, bool enableAnnotationStripping = true)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _enableFuzzyMatching = enableFuzzyMatching;
        _fuzzyMatchingThreshold = fuzzyMatchingThreshold;
        _enableAnnotationStripping = enableAnnotationStripping;
    }

    /// <summary>
    /// Finds the cover image path for a game by exact filename match, then optional fuzzy matching,
    /// and finally falls back to a system-specific or global default image.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The game filename without its extension.</param>
    /// <param name="systemName">The system name used to resolve the image folder.</param>
    /// <param name="systemImageFolder">Optional explicit image folder path for the system.</param>
    /// <returns>The full path to the matching cover image, or a default image path.</returns>
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

            // Normalized exact match (strip annotations like region, language, version)
            if (_enableAnnotationStripping)
            {
                var strippedRomName = StripAnnotations(fileNameWithoutExtension);
                if (strippedRomName != fileNameWithoutExtension)
                {
                    foreach (var ext in imageExtensions)
                    {
                        var imagePath = Path.Combine(resolvedImageFolder, $"{strippedRomName}{ext}");
                        if (File.Exists(imagePath))
                            return imagePath;
                    }
                }
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
                var normalizedRomName = _enableAnnotationStripping
                    ? StripAnnotations(lowerRomName)
                    : lowerRomName;

                foreach (var filePathInFolder in filesInImageFolder)
                {
                    var fileWithoutExt = Path.GetFileNameWithoutExtension(filePathInFolder);
                    if (string.IsNullOrEmpty(fileWithoutExt)) continue;

                    var lowerFileName = fileWithoutExt.ToLowerInvariant();
                    var normalizedFileName = _enableAnnotationStripping
                        ? StripAnnotations(lowerFileName)
                        : lowerFileName;

                    var similarity = CalculateJaroWinklerSimilarity(normalizedRomName, normalizedFileName);

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

    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex StripParenthesesRegex();

    [GeneratedRegex(@"\s*\[[^\]]*\]")]
    private static partial Regex StripSquareBracketsRegex();

    [GeneratedRegex(@"\s*\{[^}]*\}")]
    private static partial Regex StripCurlyBracesRegex();

    /// <summary>
    /// Strips parenthetical annotations (parentheses, square brackets, curly braces) from a filename,
    /// removing region, language, version, and other metadata tags commonly found in ROM filenames.
    /// </summary>
    /// <param name="fileName">The filename to clean.</param>
    /// <returns>The filename with annotations removed and trailing whitespace/dots/underscores trimmed.</returns>
    public static string StripAnnotations(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return fileName;

        var result = StripParenthesesRegex().Replace(fileName, "");
        result = StripSquareBracketsRegex().Replace(result, "");
        result = StripCurlyBracesRegex().Replace(result, "");
        result = result.Trim().TrimEnd('.', '_', ' ');

        return string.IsNullOrWhiteSpace(result) ? fileName : result;
    }

    /// <summary>
    /// Calculates the Jaro-Winkler similarity between two strings, returning a value from 0.0 (no match) to 1.0 (exact match).
    /// </summary>
    /// <param name="s1">The first string to compare.</param>
    /// <param name="s2">The second string to compare.</param>
    /// <returns>A similarity score between 0.0 and 1.0.</returns>
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
