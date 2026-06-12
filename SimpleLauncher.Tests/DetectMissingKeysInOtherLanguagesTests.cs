using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Treats strings.en.xaml as the master resource file and verifies that
/// every other strings.*.xaml file contains exactly the same set of keys.
/// Keys present in non-English files but missing from the English base
/// are automatically removed. The test fails only if keys are missing
/// from the non-English files (present in English but absent elsewhere).
/// </summary>
public partial class DetectMissingKeysInOtherLanguagesTests
{
    /// <summary>
    /// Verifies that every non-English resource file contains all keys from the English base file.
    /// </summary>
    [Fact]
    public void AllLanguageFilesShouldContainEveryKeyFromEnglish()
    {
        var resourcesPath = Path.Combine(GetSimpleLauncherPath(), "resources");
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
        var removedKeysByFile = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        foreach (var file in otherFiles)
        {
            var doc = XDocument.Load(file, LoadOptions.PreserveWhitespace);
            var root = doc.Root;
            if (root == null)
                continue;

            var elementsWithKey = root.Elements()
                .Where(e => e.Attribute(xNamespace + "Key") != null)
                .ToList();

            var fileKeys = elementsWithKey
                // ReSharper disable once NullableWarningSuppressionIsUsed
                .Select(e => e.Attribute(xNamespace + "Key")!.Value)
                .ToHashSet(StringComparer.Ordinal);

            var fileName = Path.GetFileName(file);

            var missing = englishKeys.Except(fileKeys, StringComparer.Ordinal).ToList();
            var extra = fileKeys.Except(englishKeys, StringComparer.Ordinal).ToList();

            if (missing.Count > 0)
            {
                missingKeysByFile[fileName] = missing;
            }

            if (extra.Count > 0)
            {
                removedKeysByFile[fileName] = extra;

                // Remove elements whose key is not present in English.
                foreach (var element in elementsWithKey)
                {
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    var key = element.Attribute(xNamespace + "Key")!.Value;
                    if (!englishKeys.Contains(key))
                    {
                        element.Remove();
                    }
                }

                // Ensure alphabetical order (case-insensitive) after removal.
                var remainingElements = root.Elements()
                    .Where(e => e.Attribute(xNamespace + "Key") != null)
                    .ToList();

                var sortedElements = remainingElements
                    .OrderBy(
                        // ReSharper disable once NullableWarningSuppressionIsUsed
                        e => e.Attribute(xNamespace + "Key")!.Value,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!remainingElements.SequenceEqual(sortedElements))
                {
                    foreach (var el in remainingElements)
                        el.Remove();

                    foreach (var el in sortedElements)
                        root.Add(el);
                }

                var encoding = new UTF8Encoding(true);
                using var writer = new StreamWriter(file, false, encoding);
                doc.Save(writer);
            }
        }

        if (missingKeysByFile.Count == 0 && removedKeysByFile.Count == 0)
            return; // 100 % match – pass

        var message = new StringBuilder();

        if (removedKeysByFile.Count > 0)
        {
            message.AppendLine("The following extra keys (not present in strings.en.xaml) were automatically removed:");
            message.AppendLine();
            foreach (var kvp in removedKeysByFile.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
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

    private static string GetSimpleLauncherPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "SimpleLauncher");
            if (Directory.Exists(candidate) &&
                File.Exists(Path.Combine(candidate, "SimpleLauncher.csproj")))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the SimpleLauncher project directory from the test output folder.");
    }

    [System.Text.RegularExpressions.GeneratedRegex("x:Key=\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}
