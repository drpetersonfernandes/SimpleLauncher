using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Treats strings.en.xaml as the master resource file. Any key present in a
/// non-English resource file that is missing from the English file is
/// automatically deleted. After cleanup, every file should have exactly the
/// same number of keys as the English base file.
/// </summary>
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class DetectResourceKeyCountMismatchTests
{
    private static readonly XNamespace XNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>
    /// Removes extra keys from non-English resource files so they match the
    /// English base file, then asserts that all files have the same key count.
    /// </summary>
    [Fact]
    public void AllLanguageFilesShouldHaveSameKeyCountAsEnglish()
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

        // Delete extra keys from every non-English file.
        foreach (var file in otherFiles)
        {
            var removed = DeleteExtraKeys(file, englishKeys);
            if (removed.Count > 0)
            {
                System.Diagnostics.Trace.WriteLine(
                    $"Removed extra keys from {Path.GetFileName(file)}: {string.Join(", ", removed)}");
            }
        }

        // Verify all files now have the same key count as English.
        var englishKeyCount = englishKeys.Count;
        var mismatches = new List<(string FileName, int KeyCount)>();

        foreach (var file in otherFiles)
        {
            var keyCount = CountKeys(file);
            if (keyCount != englishKeyCount)
                mismatches.Add((Path.GetFileName(file), keyCount));
        }

        if (mismatches.Count > 0)
        {
            var message = "After cleanup, some language resource files still do not match the English key count." +
                          $"{Environment.NewLine}English key count: {englishKeyCount}" +
                          $"{Environment.NewLine}{Environment.NewLine}" +
                          "Mismatched files:" + Environment.NewLine +
                          string.Join(
                              Environment.NewLine,
                              mismatches.Select(static m => $"  {m.FileName} – {m.KeyCount} keys"));

            Assert.Fail(message);
        }
    }

    /// <summary>
    /// Removes every element whose x:Key value is not present in the supplied
    /// <paramref name="englishKeys"/> set. Returns the list of keys that were
    /// removed.
    /// </summary>
    private static List<string> DeleteExtraKeys(string filePath, HashSet<string> englishKeys)
    {
        var removed = new List<string>();
        var doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
        var root = doc.Root;
        if (root == null)
            return removed;

        var toRemove = root.Elements()
            .Where(e =>
            {
                var attr = e.Attribute(XNamespace + "Key");
                return attr != null && !englishKeys.Contains(attr.Value);
            })
            .ToList();

        foreach (var element in toRemove)
        {
            removed.Add(element.Attribute(XNamespace + "Key")!.Value);
            element.Remove();
        }

        if (toRemove.Count > 0)
            doc.Save(filePath);

        return removed;
    }

    private static HashSet<string> ExtractKeys(string filePath)
    {
        var doc = XDocument.Load(filePath, LoadOptions.None);
        var root = doc.Root;
        if (root == null)
            return new HashSet<string>();

        return root.Elements()
            .Select(static e => e.Attribute(XNamespace + "Key"))
            .Where(static attr => attr != null)
            .Select(static attr => attr!.Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static int CountKeys(string filePath)
    {
        var doc = XDocument.Load(filePath, LoadOptions.None);
        var root = doc.Root;
        if (root == null)
            return 0;

        return root.Elements()
            .Count(static e => e.Attribute(XNamespace + "Key") != null);
    }
}
