using System.Globalization;
using System.Text;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Treats strings.en.xaml as the master resource file and verifies that
/// every other strings.*.xaml file contains exactly the same set of keys.
/// This test is read-only — it never modifies any resource file.
/// </summary>
public partial class DetectMissingKeysInOtherLanguagesTests
{
    /// <summary>
    /// Verifies that every non-English resource file contains all keys from the English base file.
    /// </summary>
    [Fact]
    public void AllLanguageFilesShouldContainEveryKeyFromEnglish()
    {
        var resourcesPath = Path.Combine(ProjectPathHelper.GetSimpleLauncherPath(), "resources");
        var englishFile = Path.Combine(resourcesPath, "strings.en.xaml");

        if (!File.Exists(englishFile))
            Assert.Fail($"English resource file not found: {englishFile}");

        var englishKeys = ExtractKeys(englishFile);

        var otherFiles = Directory.EnumerateFiles(resourcesPath, "strings.*.xaml")
            .Where(static f => !f.EndsWith("strings.en.xaml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (otherFiles.Count == 0)
            Assert.Fail("No other language resource files found to compare against English.");

        var missingKeysByFile = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var extraKeysByFile = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in otherFiles)
        {
            var fileKeys = ExtractKeys(file);
            var fileName = Path.GetFileName(file);

            var missing = englishKeys.Except(fileKeys, StringComparer.Ordinal).ToList();
            var extra = fileKeys.Except(englishKeys, StringComparer.Ordinal).ToList();

            if (missing.Count > 0)
            {
                missingKeysByFile[fileName] = missing;
            }

            if (extra.Count > 0)
            {
                extraKeysByFile[fileName] = extra;
            }
        }

        if (missingKeysByFile.Count == 0 && extraKeysByFile.Count == 0)
            return; // 100 % match – pass

        var message = new StringBuilder();

        if (extraKeysByFile.Count > 0)
        {
            message.AppendLine("EXTRA KEYS (present in other files but not in strings.en.xaml):");
            message.AppendLine();
            foreach (var kvp in extraKeysByFile.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"  File: {kvp.Key}");
                foreach (var key in kvp.Value.OrderBy(static x => x, StringComparer.OrdinalIgnoreCase))
                {
                    message.AppendLine(CultureInfo.InvariantCulture, $"    - {key}");
                }
            }

            message.AppendLine();
        }

        if (missingKeysByFile.Count > 0)
        {
            message.AppendLine("MISSING KEYS (present in English but missing in other files):");
            foreach (var kvp in missingKeysByFile.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"  File: {kvp.Key}");
                foreach (var key in kvp.Value.OrderBy(static x => x, StringComparer.OrdinalIgnoreCase))
                {
                    message.AppendLine(CultureInfo.InvariantCulture, $"    - {key}");
                }
            }
        }

        Assert.Fail(message.ToString());
    }

    private static HashSet<string> ExtractKeys(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var regex = MyRegex();
        return regex.Matches(content)
            .Select(static m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    [System.Text.RegularExpressions.GeneratedRegex("x:Key=\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}
