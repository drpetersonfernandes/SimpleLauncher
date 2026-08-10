using Moq;
using SimpleLauncher.Avalonia.Services.RetroAchievements;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the platform-aware RAHasher executable selection and — on Linux —
/// a real end-to-end run of the bundled native binary (complex-system hashing).
/// </summary>
public class RaHasherToolIntegrationTests
{
    private static string HasherBinaryPath => Path.Combine(
        AppContext.BaseDirectory, "tools", "RAHasher",
        RetroAchievementsHasherTool.GetHasherExecutableName());

    [Fact]
    public void GetHasherExecutableName_MatchesPlatform()
    {
        var name = RetroAchievementsHasherTool.GetHasherExecutableName();

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal("RAHasher.exe", name);
        }
        else
        {
            Assert.Equal("RAHasher", name);
        }
    }

    [Fact]
    public async Task GetHashAsync_RunsBundledBinary_ReturnsHashOrNull()
    {
        // Integration test: runs the real bundled RAHasher binary against a dummy
        // disc image (system id 1 = PlayStation, complex hashing). On Linux CI the
        // net10.0 publish of the test output contains tools/RAHasher/RAHasher;
        // on Windows the ELF binary cannot execute, so null is acceptable.
        if (!File.Exists(HasherBinaryPath))
        {
            return; // binary not shipped in this context — nothing to verify
        }

        var tempFile = Path.Combine(Path.GetTempPath(), "SLTest_rahasher_" + Guid.NewGuid().ToString("N") + ".iso");
        try
        {
            File.WriteAllBytes(tempFile, new byte[65536]); // 64 KB dummy disc image

            var logger = new Mock<ILogger>().Object;
            var tool = new RetroAchievementsHasherTool(
                logger,
                new Mock<IExtractionService>().Object,
                () => throw new InvalidOperationException("System selection not expected in this test"),
                () => throw new InvalidOperationException("Main window not expected in this test"),
                new Mock<IRetroAchievementsSystemMatcher>().Object,
                new Mock<IRetroAchievementsFileHasher>().Object,
                new Mock<IDiscConverter>().Object);

            var hash = await tool.GetHashAsync(tempFile, systemId: 1, logErrors: logger);

            if (OperatingSystem.IsWindows())
            {
                // The ELF binary cannot execute on Windows — null is acceptable there.
                if (hash is not null)
                {
                    AssertHashFormat(hash);
                }
            }
            else
            {
                // On Linux the bundled binary is deterministic for a fixed input:
                // it must actually produce a hash (complex-system hashing works).
                Assert.False(string.IsNullOrEmpty(hash), "RAHasher should produce a hash on Linux");
                AssertHashFormat(hash);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static void AssertHashFormat(string hash)
    {
        Assert.Equal(32, hash.Length);
        Assert.All(hash, c => Assert.True(Uri.IsHexDigit(c), $"non-hex char in hash: {hash}"));
    }
}
