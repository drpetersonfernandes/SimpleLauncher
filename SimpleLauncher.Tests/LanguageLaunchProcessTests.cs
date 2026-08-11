using System.Diagnostics;
using Xunit;

namespace SimpleLauncher.Tests;

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
                // Poll until the app settles: either a fallback marker appears (failure)
                // or the timeout passes with none (success). Handles async log-flush timing.
                var outcome = await WaitForOutcomeAsync(process, failedBefore, fallbackBefore, TimeSpan.FromSeconds(6));

                Assert.True(outcome == Outcome.Success,
                    $"--language {code} did not load its resources: {outcome} (see {LanguageLaunchTestsBase.LogDirectory})");
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
            var outcome = await WaitForOutcomeAsync(process, failedBefore, fallbackBefore, TimeSpan.FromSeconds(6));

            Assert.True(outcome == Outcome.FellBackToEnglish,
                $"Expected English fallback for unsupported code 'zz', got: {outcome}");
        }
        finally
        {
            KillProcess(process);
        }
    }

    private enum Outcome
    {
        Success,
        FellBackToEnglish,
        ProcessExited
    }

    /// <summary>
    /// Waits until the app either logs a language failure (fallback markers), exits,
    /// or has been running stably for <paramref name="settleTime"/> with no markers
    /// (success) — whichever comes first.
    /// </summary>
    private static async Task<Outcome> WaitForOutcomeAsync(Process process, int failedBefore, int fallbackBefore, TimeSpan settleTime)
    {
        var started = DateTime.UtcNow;
        var deadline = started.AddSeconds(40);
        while (DateTime.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                return Outcome.ProcessExited;
            }

            if (LanguageLaunchTestsBase.CountLogMarker(FailedToApplyMarker) > failedBefore ||
                LanguageLaunchTestsBase.CountLogMarker(FallbackMarker) > fallbackBefore)
            {
                return Outcome.FellBackToEnglish;
            }

            if (DateTime.UtcNow - started > settleTime)
            {
                return Outcome.Success; // app stable for the settle period with no language failure
            }

            await Task.Delay(300);
        }

        return process.HasExited ? Outcome.ProcessExited : Outcome.Success;
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
