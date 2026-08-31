using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using SimpleLauncher.Avalonia.Tests.TestHelpers;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Detects mismatched fallback strings for the same resource key in the SimpleLauncher.Avalonia
///     source code. If the same key is used with different literal fallback values (e.g.
///     GetString("Key", "Value1") vs GetString("Key", "Value2")) the test fails and presents
///     every inconsistency to the user.
/// </summary>
public partial class DetectMismatchedResourceStringsTests
{
    /// <summary>
    ///     Verifies that the same resource key is not used with different fallback string literals in source code.
    /// </summary>
    [Fact]
    public void SourceCodeShouldHaveNoMismatchedResourceFallbacks()
    {
        var avaloniaPath = AvaloniaProjectPathHelper.GetAvaloniaProjectPath();
        var mismatches = FindMismatches(avaloniaPath);

        if (mismatches.Count == 0)
            return; // pass

        var message = "Mismatched resource fallback strings detected:\n" +
                      string.Join(
                          "\n",
                          mismatches.Select(static m =>
                              $"Key: {m.Key}\n" +
                              "Values Found:\n" +
                              string.Join("\n", m.Values.Select(static v => $"  - {v}")) +
                              "\n"
                          )
                      );

        Assert.Fail(message);
    }

    private static List<(string Key, List<string> Values)> FindMismatches(string sourcePath)
    {
        // Matches: GetString("KEY", "VALUE") — literal fallback only.
        var regex = MyRegex();

        var resourceDictionary = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        var files = Directory.EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories)
            .Where(static f => !IsBuildOrObjFolder(f));

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (Match match in regex.Matches(content))
            {
                var key = Unescape(match.Groups[1].Value);
                var value = Unescape(match.Groups[2].Value);

                if (!resourceDictionary.TryGetValue(key, out var values))
                {
                    values = new HashSet<string>(StringComparer.Ordinal);
                    resourceDictionary[key] = values;
                }

                values.Add(value);
            }
        }

        return resourceDictionary
            .Where(static kvp => kvp.Value.Count > 1)
            .Select(static kvp => (kvp.Key, kvp.Value.ToList()))
            .ToList();
    }

    private static bool IsBuildOrObjFolder(string path)
    {
        return path.Contains(@"\bin\", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
               || path.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    private static string Unescape(string text)
    {
        return string.IsNullOrEmpty(text) ? text : text.Replace("\\\"", "\"").Replace(@"\\", "\\");
    }

    [SuppressMessage("Meziantou.Analyzer", "MA0023:UseRegexOptionsExplicitCapture",
        Justification = "Capturing groups are needed to extract key and fallback value")]
    [GeneratedRegex("""GetString\(\s*"((?:[^"\\]|\\.)*)"\s*,\s*"((?:[^"\\]|\\.)*)"\s*\)""", RegexOptions.Compiled, 1000)]
    private static partial Regex MyRegex();
}
