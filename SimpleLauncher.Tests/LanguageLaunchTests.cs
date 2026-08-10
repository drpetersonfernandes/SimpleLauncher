using System.Diagnostics;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Unit tests for the --language launch argument parsing and startup resolution.
/// </summary>
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

/// <summary>
/// Shared helpers for the language launch tests.
/// </summary>
public static class LanguageLaunchTestsBase
{
    // Mirrors LanguageMenuService.NameToCode (18 selectable languages)
    public static readonly string[] SupportedLanguageCodes =
    [
        "ar", "bn", "de", "en", "es", "fr", "hi", "id", "it",
        "ja", "ko", "nl", "pt-br", "ru", "tr", "ur", "vi", "zh-hans"
    ];

    public static string LogDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SimpleLauncher");

    /// <summary>
    /// Locates the built SimpleLauncher.exe (output of the referenced WPF app project).
    /// </summary>
    public static string FindAppExecutable()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        {
            foreach (var config in new[] { "Debug", "Release" })
            {
                var candidate = Path.Combine(dir.FullName, "SimpleLauncher", "bin", config, "net10.0-windows", "SimpleLauncher.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        throw new FileNotFoundException("SimpleLauncher.exe not found. Build the WPF project first.");
    }

    /// <summary>
    /// Counts occurrences of a marker across all current SimpleLauncher log files.
    /// </summary>
    public static int CountLogMarker(string marker)
    {
        if (!Directory.Exists(LogDirectory)) return 0;

        var count = 0;
        foreach (var file in Directory.GetFiles(LogDirectory, "error_user*.log"))
        {
            try
            {
                var content = File.ReadAllText(file);
                var idx = 0;
                while ((idx = content.IndexOf(marker, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    count++;
                    idx += marker.Length;
                }
            }
            catch
            {
                // log file may be locked by the running app — skip
            }
        }

        return count;
    }
}

/// <summary>
/// Real process-level launch tests: starts SimpleLauncher.exe with --language &lt;code&gt;
/// and verifies the language loads (no fallback) or, for unsupported codes, that the
/// app falls back to English without crashing.
/// Category: Integration — excluded from the default test run (like the Network tests).
/// </summary>
[Trait("Category", "Integration")]
public class LanguageLaunchProcessTests
{
    private const string FailedToApplyMarker = "Failed to Apply Language";
    private const string FallbackMarker = "Fallback to English language resources";

    [Fact]
    public async Task Launch_AllSupportedLanguages_Succeeds()
    {
        if (Process.GetProcessesByName("SimpleLauncher").Length > 0)
        {
            return; // an instance is already running — the single-instance mutex would interfere
        }

        var exe = LanguageLaunchTestsBase.FindAppExecutable();

        foreach (var code in LanguageLaunchTestsBase.SupportedLanguageCodes)
        {
            var failedBefore = LanguageLaunchTestsBase.CountLogMarker(FailedToApplyMarker);
            var fallbackBefore = LanguageLaunchTestsBase.CountLogMarker(FallbackMarker);

            using var process = StartProcess(exe, $"--language {code}");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(7));

                Assert.False(process.HasExited, $"App exited while launching with --language {code}");

                var newFailed = LanguageLaunchTestsBase.CountLogMarker(FailedToApplyMarker) - failedBefore;
                var newFallback = LanguageLaunchTestsBase.CountLogMarker(FallbackMarker) - fallbackBefore;

                Assert.True(newFailed == 0,
                    $"--language {code} FAILED to load its resources (see log: '{FailedToApplyMarker}' x{newFailed})");
                Assert.True(newFallback == 0,
                    $"--language {code} fell back to English ('{FallbackMarker}' x{newFallback})");
            }
            finally
            {
                KillProcess(process);
            }
        }
    }

    [Fact]
    public async Task Launch_UnsupportedLanguage_FallsBackToEnglish()
    {
        if (Process.GetProcessesByName("SimpleLauncher").Length > 0)
        {
            return;
        }

        var exe = LanguageLaunchTestsBase.FindAppExecutable();
        var failedBefore = LanguageLaunchTestsBase.CountLogMarker(FailedToApplyMarker);
        var fallbackBefore = LanguageLaunchTestsBase.CountLogMarker(FallbackMarker);

        using var process = StartProcess(exe, "--language zz");
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(7));

            Assert.False(process.HasExited, "App exited while launching with an unsupported language");

            var newFailed = LanguageLaunchTestsBase.CountLogMarker(FailedToApplyMarker) - failedBefore;
            var newFallback = LanguageLaunchTestsBase.CountLogMarker(FallbackMarker) - fallbackBefore;

            Assert.True(newFailed > 0, "Expected 'Failed to Apply Language' for unsupported code 'zz'");
            Assert.True(newFallback > 0, "Expected English fallback warning for unsupported code 'zz'");
        }
        finally
        {
            KillProcess(process);
        }
    }

    private static Process StartProcess(string exe, string arguments)
    {
        var psi = new ProcessStartInfo(exe, arguments)
        {
            WorkingDirectory = Path.GetDirectoryName(exe) ?? "",
            UseShellExecute = false
        };
        return Process.Start(psi) ?? throw new InvalidOperationException("Failed to start the app");
    }

    private static void KillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
        }
        catch
        {
            // already gone
        }
    }
}
