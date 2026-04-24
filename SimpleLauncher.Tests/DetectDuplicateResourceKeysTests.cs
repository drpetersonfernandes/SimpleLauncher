using System.Text;
using System.Xml.Linq;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Scans every localization resource file (strings.*.xaml) for duplicate x:Key entries.
/// Duplicate keys with identical XML representations are automatically removed so that
/// only one remains. If duplicate keys have different values, the test fails.
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

        var conflicts = new List<(string FileName, string Key, List<string> Values)>();
        XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        foreach (var file in resourceFiles)
        {
            var doc = XDocument.Load(file, LoadOptions.PreserveWhitespace);
            var root = doc.Root;
            if (root == null)
                continue;

            var elementsWithKey = root.Elements()
                .Where(e => e.Attribute(xNamespace + "Key") != null)
                .ToList();

            var grouped = elementsWithKey
                .GroupBy(e => e.Attribute(xNamespace + "Key")!.Value, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            var hasChanges = false;

            foreach (var group in grouped)
            {
                var key = group.Key;
                var elements = group.ToList();

                var distinctRepresentations = elements
                    .Select(e => e.ToString(SaveOptions.DisableFormatting))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (distinctRepresentations.Count == 1)
                {
                    // All duplicates are identical: keep the first, remove the rest.
                    for (var i = 1; i < elements.Count; i++)
                    {
                        elements[i].Remove();
                        hasChanges = true;
                    }
                }
                else
                {
                    var values = elements
                        .Select(e => e.ToString(SaveOptions.DisableFormatting))
                        .ToList();
                    conflicts.Add((Path.GetFileName(file), key, values));
                }
            }

            if (hasChanges)
            {
                var encoding = new UTF8Encoding(true);
                using var writer = new StreamWriter(file, false, encoding);
                doc.Save(writer);
            }
        }

        if (conflicts.Count == 0)
            return;

        var message = new StringBuilder();
        message.AppendLine("Duplicate resource keys with conflicting values detected:");
        message.AppendLine();
        foreach (var conflict in conflicts)
        {
            message.AppendLine($"File: {conflict.FileName}, Key: '{conflict.Key}'");
            foreach (var value in conflict.Values)
            {
                message.AppendLine($"  - {value}");
            }
        }

        Assert.Fail(message.ToString());
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

}
