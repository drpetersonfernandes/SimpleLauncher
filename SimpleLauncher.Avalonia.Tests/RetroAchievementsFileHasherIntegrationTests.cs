using Moq;
using SimpleLauncher.Core.Services.RetroAchievements;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Integration tests for the RetroAchievementsSharp library-backed file hasher
/// (the replacement for the bundled RAHasher binary). The library is managed
/// .NET, so these tests run the real hashing path on every platform.
/// </summary>
public class RetroAchievementsFileHasherIntegrationTests
{
    private static RetroAchievementsFileHasher CreateHasher()
    {
        return new RetroAchievementsFileHasher(
            new Mock<ILogger>().Object,
            new RetroAchievementsSystemMatcher(new Mock<ILogger>().Object, new Mock<ILogger>().Object));
    }

    [Fact]
    public async Task CalculateHashAsync_GenesisRom_ReturnsValidHash()
    {
        // Integration test: runs the real RetroAchievementsSharp hashing engine
        // against a dummy cartridge file (system 'genesis/mega drive', whole-file MD5).
        var tempFile = Path.Combine(Path.GetTempPath(), "SLTest_rahash_" + Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            File.WriteAllBytes(tempFile, new byte[65536]);

            var hasher = CreateHasher();
            var hash = await hasher.CalculateHashAsync(tempFile, "genesis/mega drive");

            Assert.False(string.IsNullOrEmpty(hash), "The hashing engine should produce a hash for a cartridge ROM");
            AssertHashFormat(hash);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CalculateHashAsync_UnsupportedInput_ReturnsNull()
    {
        // A dummy zero-filled file is not a valid disc image, so the engine must
        // fail gracefully (return null) instead of throwing.
        var tempFile = Path.Combine(Path.GetTempPath(), "SLTest_rahash_" + Guid.NewGuid().ToString("N") + ".iso");
        try
        {
            File.WriteAllBytes(tempFile, new byte[65536]);

            var hasher = CreateHasher();
            var hash = await hasher.CalculateHashAsync(tempFile, "playstation");

            Assert.Null(hash);
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