using System.Xml.Linq;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Counts the number of keys in every localization resource file (strings.*.xaml)
/// and verifies that each file contains exactly the same number of keys as the
/// English base file (strings.en.xaml). The test fails if any file has a
/// different key count.
/// </summary>
public class DetectResourceKeyCountMismatchTests
{
    [Fact]
    public void AllLanguageFilesShouldHaveSameKeyCountAsEnglish()
    {
        var resourcesPath = Path.Combine(GetSimpleLauncherPath(), "resources");
        var englishFile = Path.Combine(resourcesPath, "strings.en.xaml");

        if (!File.Exists(englishFile))
            Assert.Fail($"English resource file not found: {englishFile}");

        var englishKeyCount = CountKeys(englishFile);

        var otherFiles = Directory.EnumerateFiles(resourcesPath, "strings.*.xaml")
            .Where(static f => !f.EndsWith("strings.en.xaml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (otherFiles.Count == 0)
            Assert.Fail("No other language resource files found to compare against English.");

        var mismatches = new List<(string FileName, int KeyCount)>();

        foreach (var file in otherFiles)
        {
            var keyCount = CountKeys(file);
            if (keyCount != englishKeyCount)
            {
                mismatches.Add((Path.GetFileName(file), keyCount));
            }
        }

        if (mismatches.Count == 0)
            return;

        var message = $"Language resource files do not have the same number of keys as the English base file (strings.en.xaml).{Environment.NewLine}" +
                      $"English key count: {englishKeyCount}{Environment.NewLine}{Environment.NewLine}" +
                      "Mismatched files:" + Environment.NewLine +
                      string.Join(
                          Environment.NewLine,
                          mismatches.Select(static m => $"  {m.FileName} – {m.KeyCount} keys")
                      );

        Assert.Fail(message);
    }

    private static int CountKeys(string filePath)
    {
        XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        var doc = XDocument.Load(filePath, LoadOptions.None);
        var root = doc.Root;
        if (root == null)
            return 0;

        return root.Elements()
            .Count(e => e.Attribute(xNamespace + "Key") != null);
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
