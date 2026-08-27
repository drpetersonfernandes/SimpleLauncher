using System.Collections;
using System.Resources;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Guards the localization PACKAGING step: every language the app can select must
/// be embedded as a pack resource in the built assembly, otherwise
/// App.ApplyLanguage throws IOException ("Failed to Apply Language") at runtime
/// and falls back to English.
/// </summary>
public class LocalizationResourcePackagingTests
{
    // Mirrors LanguageMenuService.NameToCode (the 18 selectable languages)
    private static readonly string[] SupportedLanguageCodes =
    [
        "ar", "bn", "de", "en", "es", "fr", "hi", "id", "it",
        "ja", "ko", "nl", "pt-br", "ru", "tr", "ur", "vi", "zh-hans"
    ];

    [Fact]
    public void AllSupportedLanguages_AreEmbeddedAsPackResources()
    {
        var assembly = typeof(App).Assembly;

        using var stream = assembly.GetManifestResourceStream("SimpleLauncher.g.resources");
        Assert.NotNull(stream);

        using var reader = new ResourceReader(stream);
        var embeddedNames = new HashSet<string>(
            reader.Cast<DictionaryEntry>().Select(d => (string)d.Key),
            StringComparer.OrdinalIgnoreCase);

        foreach (var code in SupportedLanguageCodes)
        {
            var expectedResource = $"resources/strings.{code}.xaml";
            Assert.True(
                embeddedNames.Contains(expectedResource),
                $"'{expectedResource}' is NOT embedded in the assembly. " +
                "Check SimpleLauncher.csproj: the language needs a <Resource Include=\"resources\\strings.<code>.xaml\" /> entry " +
                "(a bare <Page Remove> excludes it from the build entirely).");
        }
    }

    [Fact]
    public void EveryEmbeddedStringsResource_HasAMatchingSourceLanguage()
    {
        var assembly = typeof(App).Assembly;

        using var stream = assembly.GetManifestResourceStream("SimpleLauncher.g.resources");
        Assert.NotNull(stream);

        using var reader = new ResourceReader(stream);
        var embedded = reader.Cast<DictionaryEntry>()
            .Select(d => (string)d.Key)
            .Where(k => k.StartsWith("resources/strings.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.NotEmpty(embedded);

        foreach (var resource in embedded)
        {
            var code = resource.Replace("resources/strings.", "", StringComparison.OrdinalIgnoreCase)
                .Replace(".xaml", "", StringComparison.OrdinalIgnoreCase);
            Assert.True(SupportedLanguageCodes.Contains(code, StringComparer.OrdinalIgnoreCase),
                $"Embedded resource '{resource}' does not map to a known language.");
        }
    }
}