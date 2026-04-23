using System.Text.RegularExpressions;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Scans every localization resource file (strings.*.xaml) for duplicate x:Key entries.
/// The test fails if any key appears more than once inside the same file.
/// </summary>
public partial class DetectDuplicateResourceKeysTests
{
    [Fact]
    public void AllResourceFilesShouldHaveNoDuplicateKeys()
    {
        var resourcesPath = Path.Combine(GetSimpleLauncherPath(), "resources");
        var resourceFiles = Directory.EnumerateFiles(resourcesPath, "strings.*.xaml")
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (resourceFiles.Count == 0)
            Assert.Fail($"No resource files found in: {resourcesPath}");

        var duplicates = new List<(string FileName, string Key, int Count)>();
        var keyRegex = MyRegex();

        foreach (var file in resourceFiles)
        {
            var content = File.ReadAllText(file);
            var keyCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in keyRegex.Matches(content))
            {
                var key = match.Groups[1].Value;
                keyCounts[key] = keyCounts.GetValueOrDefault(key) + 1;
            }

            foreach (var kvp in keyCounts.Where(static x => x.Value > 1))
            {
                duplicates.Add((Path.GetFileName(file), kvp.Key, kvp.Value));
            }
        }

        if (duplicates.Count == 0)
            return;

        var message = "Duplicate resource keys detected:\n\n" +
                      string.Join(
                          "\n",
                          duplicates.Select(static d => $"File: {d.FileName}, Key: '{d.Key}', Occurrences: {d.Count}")
                      );

        Assert.Fail(message);
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

    [GeneratedRegex("""
                    x:Key="([^"]+)"
                    """, RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
