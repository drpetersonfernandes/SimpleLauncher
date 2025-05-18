using System;
using System.IO;
using System.Linq;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class FindCoverImage
{
    private static readonly string GlobalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");

    // Define the Jaro-Winkler prefix scale (usually 0.1)
    private const double PrefixScale = 0.1;

    // Define the maximum prefix length for Winkler adjustment
    private const int MaxPrefixLength = 4;

    public static string FindCoverImagePath(string fileNameWithoutExtension, string systemName, SystemManager systemManager)
    {
        var applicationPath = AppDomain.CurrentDomain.BaseDirectory;
        var imageExtensions = GetImageExtensions.GetExtensions();

        string systemImagePath;
        if (string.IsNullOrEmpty(systemManager?.SystemImageFolder))
        {
            systemImagePath = Path.Combine(applicationPath, "images", systemName);
        }
        else
        {
            systemImagePath = Path.IsPathRooted(systemManager.SystemImageFolder)
                ? systemManager.SystemImageFolder // If already absolute
                : Path.Combine(applicationPath, systemManager.SystemImageFolder); // Make it absolute
        }

        // 1. Check for exact match first
        foreach (var ext in imageExtensions)
        {
            var imagePath = Path.Combine(systemImagePath, $"{fileNameWithoutExtension}{ext}");
            if (File.Exists(imagePath))
                return imagePath;
        }

        // Get settings for fuzzy matching
        var settings = App.Settings; // Access settings via the static App property
        var enableFuzzyMatching = settings.EnableFuzzyMatching;
        var similarityThreshold = settings.FuzzyMatchingThreshold;

        // 2. If no exact match and fuzzy matching is enabled, check for similar filenames if the directory exists
        if (enableFuzzyMatching && Directory.Exists(systemImagePath))
        {
            var filesInImageFolder = Directory.GetFiles(systemImagePath)
                .Where(f => imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            string bestMatchPath = null;
            double highestSimilarity = 0;
            var lowerRomName = fileNameWithoutExtension.ToLowerInvariant();

            foreach (var filePath in filesInImageFolder)
            {
                var fileWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(fileWithoutExt)) continue; // Skip files without names

                var lowerFileName = fileWithoutExt.ToLowerInvariant();

                // Calculate similarity using Jaro-Winkler
                var similarity = CalculateJaroWinklerSimilarity(lowerRomName, lowerFileName);

                if (!(similarity > highestSimilarity)) continue;

                highestSimilarity = similarity;
                bestMatchPath = filePath;
            }

            // If the highest similarity meets the threshold, return that path
            if (bestMatchPath != null && highestSimilarity >= similarityThreshold)
            {
                return bestMatchPath;
            }
        }

        // 3. If no exact or similar match, fall back to default images
        var defaultSystemImagePath = Path.Combine(systemImagePath, "default.png");
        if (File.Exists(defaultSystemImagePath))
        {
            return defaultSystemImagePath;
        }
        else
        {
            return GlobalDefaultImagePath;
        }
    }

    /// <summary>
    /// Calculates the Jaro-Winkler similarity between two strings.
    /// Returns a value between 0.0 (no similarity) and 1.0 (identical).
    /// </summary>
    public static double CalculateJaroWinklerSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return string.IsNullOrEmpty(s1) == string.IsNullOrEmpty(s2) ? 1.0 : 0.0;
        }

        // Convert to lowercase for case-insensitive comparison
        s1 = s1.ToLowerInvariant();
        s2 = s2.ToLowerInvariant();

        var len1 = s1.Length;
        var len2 = s2.Length;

        // Calculate Jaro Distance
        var matchDistance = Math.Max(len1, len2) / 2 - 1;
        var matches = 0;
        var s1Matches = new bool[len1];
        var s2Matches = new bool[len2];

        // Find matches
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

        // Count transpositions
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

        // Calculate Winkler adjustment
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

        // Calculate Jaro-Winkler distance
        var jaroWinklerDistance = jaroDistance + prefix * PrefixScale * (1 - jaroDistance);

        return jaroWinklerDistance;
    }
}

