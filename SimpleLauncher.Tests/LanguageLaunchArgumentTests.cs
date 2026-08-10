using Xunit;

namespace SimpleLauncher.Tests;

public class LanguageLaunchArgumentTests
{
    public static TheoryData<string[], string> ParsingCases => new()
    {
        { new[] { "--language", "es" }, "es" },
        { new[] { "-language", "TR" }, "TR" },                      // raw value returned; canonicalized later
        { new[] { "--language=fr" }, "fr" },
        { new[] { "-debug", "--language", "pt-br" }, "pt-br" }       // mixed with other args
    };

    [Theory]
    [MemberData(nameof(ParsingCases))]
    public void TryGetLanguageArg_ParsesKnownForms(string[] args, string expected)
    {
        Assert.Equal(expected, App.TryGetLanguageArg(args));
    }

    public static TheoryData<string[]> AbsentCases => new()
    {
        { Array.Empty<string>() },
        { new[] { "-debug" } },
        { new[] { "--language" } },   // missing value
        { new[] { "--restarting" } }
    };

    [Theory]
    [MemberData(nameof(AbsentCases))]
    public void TryGetLanguageArg_ReturnsNull_WhenAbsent(string[] args)
    {
        Assert.Null(App.TryGetLanguageArg(args));
    }

    public static TheoryData<string[], string, string> ResolutionCases => new()
    {
        { new[] { "--language", "es" }, "en", "es" },      // arg wins
        { new[] { "-language", "TR" }, "en", "tr" },      // canonicalized
        { Array.Empty<string>(), "fr", "fr" },            // configured used
        { new[] { "--language", "zz" }, "fr", "zz" },     // unsupported -> passed through (ApplyLanguage falls back to en)
        { new[] { "-debug" }, "de", "de" }                // unrelated args ignored
    };

    [Theory]
    [MemberData(nameof(ResolutionCases))]
    public void ResolveStartupLanguage_PrecedenceAndFallback(string[] args, string configured, string expected)
    {
        Assert.Equal(expected, App.ResolveStartupLanguage(args, configured));
    }

    [Fact]
    public void ResolveStartupLanguage_AllSupportedCodes_AreAccepted()
    {
        foreach (var code in LanguageLaunchTestsBase.SupportedLanguageCodes)
        {
            Assert.Equal(code, App.ResolveStartupLanguage(new[] { "--language", code.ToUpperInvariant() }, "en"));
        }
    }
}
