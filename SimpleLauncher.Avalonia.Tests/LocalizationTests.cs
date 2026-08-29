using System.Text.Json;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Localization parity tests: every strings.{lang}.json must ship the exact same
///     key set as strings.en.json, and the LocalizationService must resolve all of
///     them (including the case-insensitive lookup for WPF-style codes like 'pt-br').
/// </summary>
public class LocalizationTests
{
    private static string ResourcesDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

    private static IEnumerable<string> LanguageFiles()
    {
        var dir = ResourcesDir;
        if (!Directory.Exists(dir)) return [];

        return Directory.EnumerateFiles(dir, "strings.*.json")
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
    }

    private static Dictionary<string, string> LoadKeys(string file)
    {
        var json = File.ReadAllText(file);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    [Fact]
    public void EveryLanguageFileExistsForTheCanonicalLanguageSet()
    {
        var expected = LocalizationService.AvailableLanguages.Keys
            .Select(lang => $"strings.{lang}.json")
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var actual = LanguageFiles().Select(Path.GetFileName).Where(n => n is not null).Select(n => n!)
            .OrderBy(n => n, StringComparer.Ordinal).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void EveryLanguageFileSharesTheEnglishKeySet()
    {
        var enFile = Path.Combine(ResourcesDir, "strings.en.json");
        Assert.True(File.Exists(enFile), $"Missing {enFile}");
        var enKeys = LoadKeys(enFile).Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

        foreach (var file in LanguageFiles())
        {
            var keys = LoadKeys(file).Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

            var missing = enKeys.Except(keys).ToList();
            var extra = keys.Except(enKeys).ToList();
            Assert.True(missing.Count == 0, $"{Path.GetFileName(file)} is missing keys: {string.Join(", ", missing)}");
            Assert.True(extra.Count == 0, $"{Path.GetFileName(file)} has unexpected keys: {string.Join(", ", extra)}");
        }
    }

    [Fact]
    public void LocalizationServiceLoadsEveryLanguage()
    {
        var localization = new LocalizationService();

        foreach (var lang in LocalizationService.AvailableLanguages.Keys)
        {
            localization.LoadLanguage(lang);

            Assert.Equal(lang, localization.CurrentLanguage);
            Assert.NotEqual("Sidebar.AllGames", localization.GetString("Sidebar.AllGames"));
            Assert.NotEqual("Toolbar.Search", localization.GetString("Toolbar.Search"));
        }
    }

    [Fact]
    public void LocalizationServiceResolvesWpfStyleCodesCaseInsensitively()
    {
        // The WPF app stores 'pt-br' / 'zh-hans' in settings.xml; the Avalonia
        // files are named 'pt-BR' / 'zh-Hans'. On Linux the lookup must still match.
        var localization = new LocalizationService();

        localization.LoadLanguage("pt-br");
        Assert.Equal("pt-BR", localization.CurrentLanguage);
        Assert.NotEqual("Toolbar.Games", localization.GetString("Toolbar.Games"));

        localization.LoadLanguage("zh-hans");
        Assert.Equal("zh-Hans", localization.CurrentLanguage);
        Assert.NotEqual("Sidebar.AllGames", localization.GetString("Sidebar.AllGames"));
    }
}