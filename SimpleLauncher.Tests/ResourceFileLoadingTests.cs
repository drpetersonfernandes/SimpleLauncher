using System.Windows;
using System.Windows.Markup;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Loads every localization resource dictionary (strings.*.xaml) at runtime
/// using the WPF XAML parser.  The test fails if any file cannot be parsed,
/// if the root element is not a ResourceDictionary, or if duplicate keys
/// cause a runtime exception during load.
/// </summary>
public class ResourceFileLoadingTests
{
    /// <summary>
    /// Verifies that all localization resource XAML files can be loaded and parsed without errors.
    /// </summary>
    [Fact]
    public void AllResourceFilesShouldLoadWithoutErrors()
    {
        var resourcesPath = Path.Combine(ProjectPathHelper.GetSimpleLauncherPath(), "resources");
        var resourceFiles = Directory.EnumerateFiles(resourcesPath, "strings.*.xaml")
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (resourceFiles.Count == 0)
            Assert.Fail($"No resource files found in: {resourcesPath}");

        var failures = new List<(string FileName, string Error)>();

        foreach (var file in resourceFiles)
        {
            try
            {
                using var stream = File.OpenRead(file);
                var loadedObject = XamlReader.Load(stream);

                if (loadedObject is not ResourceDictionary)
                {
                    failures.Add((
                        Path.GetFileName(file),
                        $"Root element is {loadedObject?.GetType().Name ?? "null"}, expected ResourceDictionary."
                    ));
                }
            }
            catch (XamlParseException ex)
            {
                failures.Add((Path.GetFileName(file), $"XAML parse error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                failures.Add((Path.GetFileName(file), $"{ex.GetType().Name}: {ex.Message}"));
            }
        }

        if (failures.Count == 0)
            return;

        var message = "Failed to load the following resource files:\n\n" +
                      string.Join(
                          "\n",
                          failures.Select(static f => $"File: {f.FileName}\nError: {f.Error}")
                      );

        Assert.Fail(message);
    }
}
