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

        string systemImageFolder;
        if (string.IsNullOrEmpty(systemManager.SystemImageFolder))
        {
            systemImageFolder = Path.Combine(applicationPath, "images", systemName ?? string.Empty);
        }
        else
        {
            systemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemManager.SystemImageFolder);
        }

        // Check if the resolved system image folder path is valid before proceeding
        if (!string.IsNullOrEmpty(systemImageFolder) && Directory.Exists(systemImageFolder))
        {
            // 1. Check for the exact match first within the resolved folder
            foreach (var ext in imageExtensions)
            {
                var imagePath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}{ext}");
                if (File.Exists(imagePath))
                    return imagePath; // Return the found path (which is already resolved)
            }

            var settings = App.Settings;
            var enableFuzzyMatching = false;
            var similarityThreshold = 0.8;

            if (settings != null)
            {
                enableFuzzyMatching = settings.EnableFuzzyMatching;
                similarityThreshold = settings.FuzzyMatchingThreshold;
            }
            else
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(null, "App.Settings was null in FindCoverImage. Using default fuzzy matching settings.");
            }

            // 2. If no exact match and fuzzy matching is enabled, check for similar filenames
            if (enableFuzzyMatching)
            {
                var filesInImageFolder = Directory.GetFiles(systemImageFolder)
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
                    bestMatchPath = filePathInFolder; // This is already a resolved path
                }

                if (bestMatchPath != null && highestSimilarity >= similarityThreshold)
                {
                    return bestMatchPath; // Return the found resolved path
                }
            }
        }
        else if (!string.IsNullOrEmpty(systemManager.SystemImageFolder)) // Only log if a path was actually configured
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(null, $"FindCoverImagePath: System image folder path invalid or not found for system '{systemName}': '{systemManager.SystemImageFolder}' -> '{systemImageFolder}'. Cannot search for images.");
        }

        // 3. Fallback to default images
        // Check the default system image path within the resolved system image folder first
        if (string.IsNullOrEmpty(systemImageFolder)) return GlobalDefaultImagePath;

        // Only check if the resolved folder path was valid
        var defaultSystemImagePath = Path.Combine(systemImageFolder, "default.png");
        if (File.Exists(defaultSystemImagePath))
        {
            return defaultSystemImagePath; // Return the resolved default path
        }

        // Fallback to the global default image
        return GlobalDefaultImagePath;
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