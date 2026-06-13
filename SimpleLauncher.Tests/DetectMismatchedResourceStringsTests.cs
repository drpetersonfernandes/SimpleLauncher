using System.Text.RegularExpressions;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Detects mismatched fallback strings for the same resource key in the source code.
/// If the same key is used with different literal fallback values (e.g.
/// TryFindResource("Key") ?? "Value1" vs TryFindResource("Key") ?? "Value2")
/// the test fails and presents every inconsistency to the user.
/// </summary>
public partial class DetectMismatchedResourceStringsTests
{
    /// <summary>
    /// Verifies that the same resource key is not used with different fallback string literals in source code.
    /// </summary>
    [Fact]
    public void SourceCodeShouldHaveNoMismatchedResourceFallbacks()
    {
        var simpleLauncherPath = ProjectPathHelper.GetSimpleLauncherPath();
        var mismatches = FindMismatches(simpleLauncherPath);

        if (mismatches.Count == 0)
            return; // pass

        var message = "Mismatched resource fallback strings detected:\n" +
                      string.Join(
                          "\n",
                          mismatches.Select(static m =>
                              $"Key: {m.Key}\n" +
                              $"Values Found:\n" +
                              string.Join("\n", m.Values.Select(static v => $"  - {v}")) +
                              "\n"
                          )
                      );

        Assert.Fail(message);
    }

    private static List<(string Key, List<string> Values)> FindMismatches(string sourcePath)
    {
        // Matches: TryFindResource("KEY") ?? "VALUE" (literal fallback only)
        var regex = MyRegex();

        var resourceDictionary = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        var files = Directory.EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(sourcePath, "*.xaml", SearchOption.AllDirectories))
            .Where(static f => !IsBuildOrResourceFolder(f));

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (Match match in regex.Matches(content))
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;

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

    private static bool IsBuildOrResourceFolder(string path)
    {
        return path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
               || path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)
               || path.Contains("\\resources\\", StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex("""
                    TryFindResource\(\s*"([^"]+)"\s*\)\s*\?\?\s*"([^"]+)"
                    """, RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
