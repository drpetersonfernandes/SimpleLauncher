using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Compares every resource key referenced in the SimpleLauncher source code
/// against the English resource dictionary (strings.en.xaml).
/// Missing keys are automatically appended to the resource file and the test
/// fails so the developer is informed of what was added.
/// </summary>
public partial class DetectMissingResourceStringsTests
{
    [Fact]
    public void EnglishResourceFileShouldContainAllReferencedKeys()
    {
        var simpleLauncherPath = GetSimpleLauncherPath();
        var stringsEnPath = Path.Combine(simpleLauncherPath, "resources", "strings.en.xaml");
        var appXamlPath = Path.Combine(simpleLauncherPath, "App.xaml");

        if (!File.Exists(stringsEnPath))
            Assert.Fail($"English resource file not found: {stringsEnPath}");

        // Keys already defined in the English resource file.
        var existingKeys = ExtractKeysFromXaml(stringsEnPath);

        // Keys defined in App.xaml (brushes, styles, converters, etc.) are not localization strings.
        var appKeys = File.Exists(appXamlPath) ? ExtractKeysFromXaml(appXamlPath) : new HashSet<string>(StringComparer.Ordinal);

        // Collect keys referenced in C# together with their fallback value when available.
        var csKeys = CollectCsKeys(simpleLauncherPath);

        // Collect keys referenced in XAML via DynamicResource (excluding library resources that contain dots).
        var xamlKeys = CollectXamlDynamicResourceKeys(simpleLauncherPath);

        // Determine missing keys.
        var missingFromCs = csKeys.Keys.Except(existingKeys, StringComparer.Ordinal);
        var missingFromXaml = xamlKeys.Except(existingKeys, StringComparer.Ordinal)
            .Except(appKeys, StringComparer.Ordinal);

        var missingKeys = missingFromCs.Concat(missingFromXaml)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (missingKeys.Count == 0)
            return; // nothing missing – pass

        // Separate keys with known fallback values from keys without known values.
        var keysWithValues = new Dictionary<string, string>(StringComparer.Ordinal);
        var keysWithoutValues = new List<string>();

        foreach (var key in missingKeys)
        {
            if (csKeys.TryGetValue(key, out var fallback) && !string.IsNullOrEmpty(fallback))
            {
                keysWithValues[key] = fallback;
            }
            else
            {
                keysWithoutValues.Add(key);
            }
        }

        // Only auto-add keys that have a known non-empty fallback value.
        if (keysWithValues.Count > 0)
        {
            AppendMissingEntries(stringsEnPath, keysWithValues);
        }

        // Build failure message.
        var message = new StringBuilder();
        var hasMissing = keysWithoutValues.Count > 0;
        var hasAdded = keysWithValues.Count > 0;

        if (hasAdded)
        {
            message.AppendLine(CultureInfo.InvariantCulture, $"{keysWithValues.Count} missing resource key(s) were automatically added to strings.en.xaml:");
            message.AppendLine();
            foreach (var key in keysWithValues.Keys.OrderBy(static k => k, StringComparer.OrdinalIgnoreCase))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"  - {key}");
            }

            message.AppendLine();
        }

        if (hasMissing)
        {
            message.AppendLine("The following referenced keys could not be automatically added because no fallback value is known. Please add them manually to strings.en.xaml:");
            message.AppendLine();
            foreach (var key in keysWithoutValues.OrderBy(static k => k, StringComparer.OrdinalIgnoreCase))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"  - {key}");
            }
        }

        if (hasMissing || hasAdded)
        {
            Assert.Fail(message.ToString());
        }
    }

    /// <summary>
    /// Scans .cs files for TryFindResource("...") and captures the key.
    /// When a literal fallback string is present (?? "...") it is stored as the value.
    /// </summary>
    private static Dictionary<string, string> CollectCsKeys(string sourcePath)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        // Captures key + optional literal fallback value.
        var regex = MyRegex();

        var files = Directory.EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories)
            .Where(static f => !IsBuildOrResourceFolder(f));

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (Match match in regex.Matches(content))
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Scans .xaml files for {DynamicResource KeyName} usages.
    /// Keys that contain dots (e.g. MahApps.Brushes.Accent) are ignored because
    /// they belong to external libraries, not the application string resources.
    /// </summary>
    private static HashSet<string> CollectXamlDynamicResourceKeys(string sourcePath)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var regex = MyRegex1();

        var files = Directory.EnumerateFiles(sourcePath, "*.xaml", SearchOption.AllDirectories)
            .Where(static f => !IsBuildOrResourceFolder(f));

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (Match match in regex.Matches(content))
            {
                var key = match.Groups[1].Value;
                if (!key.Contains('.'))
                    result.Add(key);
            }
        }

        return result;
    }

    private static HashSet<string> ExtractKeysFromXaml(string xamlPath)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var content = File.ReadAllText(xamlPath);
        var regex = MyRegex2();

        foreach (Match match in regex.Matches(content))
            keys.Add(match.Groups[1].Value);

        return keys;
    }

    /// <summary>
    /// Parses strings.en.xaml, appends the missing entries, sorts everything
    /// alphabetically by key, and rewrites the file preserving the XML header.
    /// </summary>
    private static void AppendMissingEntries(string filePath, Dictionary<string, string> missingEntries)
    {
        var lines = File.ReadAllLines(filePath).ToList();
        var entryRegex = MyRegex3();

        var existingEntries = new Dictionary<string, string>(StringComparer.Ordinal);
        var firstEntryIndex = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            var match = entryRegex.Match(lines[i]);
            if (match.Success)
            {
                existingEntries[match.Groups[1].Value] = UnescapeXml(match.Groups[2].Value);
                if (firstEntryIndex == -1)
                {
                    firstEntryIndex = i;
                }
            }
            else if (lines[i].Trim() == "</ResourceDictionary>")
            {
                if (firstEntryIndex == -1)
                {
                    firstEntryIndex = i;
                }
            }
        }

        // Merge missing entries.
        foreach (var kvp in missingEntries)
        {
            if (!existingEntries.ContainsKey(kvp.Key))
            {
                existingEntries[kvp.Key] = kvp.Value;
            }
        }

        // Rebuild file: header + sorted entries + footer.
        var header = firstEntryIndex >= 0 ? lines.Take(firstEntryIndex).ToList() : lines.ToList();
        var sortedEntries = existingEntries
            .OrderBy(static e => e.Key, StringComparer.OrdinalIgnoreCase)
            .Select(static e => $"    <system:String x:Key=\"{e.Key}\">{EscapeXml(e.Value)}</system:String>")
            .ToList();
        var footer = new List<string> { "</ResourceDictionary>" };

        var encoding = new UTF8Encoding(true);
        File.WriteAllLines(filePath, header.Concat(sortedEntries).Concat(footer), encoding);
    }

    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static string UnescapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&amp;", "&");
    }

    private static bool IsBuildOrResourceFolder(string path)
    {
        return path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
               || path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)
               || path.Contains("\\resources\\", StringComparison.OrdinalIgnoreCase);
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

    [GeneratedRegex("""TryFindResource\(\s*"([^"]+)"\s*\)(?:\s*\?\?\s*"([^"]+)")?""", RegexOptions.Compiled)]
    private static partial Regex MyRegex();

    [GeneratedRegex(@"\{DynamicResource\s+([^}\s""]+)\}", RegexOptions.Compiled)]
    private static partial Regex MyRegex1();

    [GeneratedRegex("""
                    x:Key="([^"]+)"
                    """, RegexOptions.Compiled)]
    private static partial Regex MyRegex2();

    [GeneratedRegex("""^\s*<system:String x:Key="([^"]+)">(.*)</system:String>\s*$""")]
    private static partial Regex MyRegex3();
}
