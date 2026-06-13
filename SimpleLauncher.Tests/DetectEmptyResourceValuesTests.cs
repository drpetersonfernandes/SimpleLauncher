using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Scans every localization resource file (strings.*.xaml) for entries
/// with empty string values and fails if any are found.
/// </summary>
public partial class DetectEmptyResourceValuesTests
{
    /// <summary>
    /// Verifies that no localization resource file contains entries with empty string values.
    /// </summary>
    [Fact]
    public void AllResourceFilesShouldHaveNoEmptyValues()
    {
        var resourcesPath = Path.Combine(ProjectPathHelper.GetSimpleLauncherPath(), "resources");
        var resourceFiles = Directory.EnumerateFiles(resourcesPath, "strings.*.xaml")
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (resourceFiles.Count == 0)
            Assert.Fail($"No resource files found in: {resourcesPath}");

        var emptyEntriesByFile = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var regex = MyRegex();

        foreach (var file in resourceFiles)
        {
            var lines = File.ReadAllLines(file);
            var emptyKeys = new List<string>();

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    if (string.IsNullOrEmpty(value))
                    {
                        emptyKeys.Add(key);
                    }
                }
            }

            if (emptyKeys.Count > 0)
            {
                emptyEntriesByFile[Path.GetFileName(file)] = emptyKeys;
            }
        }

        if (emptyEntriesByFile.Count == 0)
            return;

        var message = new StringBuilder();
        message.AppendLine("Empty resource values detected. Every key must have a non-empty value:");
        message.AppendLine();

        foreach (var kvp in emptyEntriesByFile.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            message.AppendLine(CultureInfo.InvariantCulture, $"  File: {kvp.Key}");
            foreach (var key in kvp.Value.OrderBy(static x => x, StringComparer.OrdinalIgnoreCase))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"    - {key}");
            }
        }

        Assert.Fail(message.ToString());
    }

    [GeneratedRegex("""^\s*<system:String x:Key="([^"]+)">(.*)</system:String>\s*$""", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
