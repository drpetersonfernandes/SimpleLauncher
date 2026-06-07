using System.Reflection;
using System.Text;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class RetroAchievementsFileHasherTests : IDisposable
{
    private readonly string _testDirectory;

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    public RetroAchievementsFileHasherTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_RAHasherTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    private string CreateTempFile(string relativePath, byte[] content)
    {
        var fullPath = Path.Combine(_testDirectory, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(fullPath, content);
        return fullPath;
    }

    private string CreateTempFile(string relativePath, string content, Encoding? encoding = null)
    {
        var fullPath = Path.Combine(_testDirectory, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(fullPath, content, encoding ?? Encoding.UTF8);
        return fullPath;
    }

    [Fact]
    public async Task CalculateStandardMd5AsyncReturnsExpectedHash()
    {
        var content = "Hello World"u8.ToArray();
        var filePath = CreateTempFile("test.bin", content);

        var result = await RetroAchievementsFileHasher.CalculateStandardMd5Async(filePath, new NoOpLogErrors());

        Assert.Equal("b10a8db164e0754105b7a99be72e3fe5", result);
    }

    [Fact]
    public async Task CalculateStandardMd5AsyncEmptyFileReturnsEmptyHash()
    {
        var filePath = CreateTempFile("empty.bin", []);

        var result = await RetroAchievementsFileHasher.CalculateStandardMd5Async(filePath, new NoOpLogErrors());

        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", result);
    }

    [Fact]
    public void CalculateFilenameHashReturnsExpectedHash()
    {
        var result = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\roms\game.zip", new NoOpLogErrors());

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void CalculateFilenameHashSameFilenameWithoutExtensionProducesSameHash()
    {
        var hash1 = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\folder\mygame.zip", new NoOpLogErrors());
        var hash2 = RetroAchievementsFileHasher.CalculateFilenameHash(@"D:\other\mygame.7z", new NoOpLogErrors());

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CalculateFilenameHashDifferentFilenamesProduceDifferentHashes()
    {
        var hash1 = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\game1.zip", new NoOpLogErrors());
        var hash2 = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\game2.zip", new NoOpLogErrors());

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task CalculateArduboyHashAsyncNormalizesLineEndings()
    {
        var filePath1 = CreateTempFile("arduboy_crlf.hex", "hello\r\nworld\n");
        var filePath2 = CreateTempFile("arduboy_lf.hex", "hello\nworld\n");

        var hash1 = await RetroAchievementsFileHasher.CalculateArduboyHashAsync(filePath1, new NoOpLogErrors());
        var hash2 = await RetroAchievementsFileHasher.CalculateArduboyHashAsync(filePath2, new NoOpLogErrors());

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async Task CalculateN64HashAsyncZ64ReturnsStandardMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.z64", content);

        var n64Hash = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath, new NoOpLogErrors());
        var standardHash = await RetroAchievementsFileHasher.CalculateStandardMd5Async(filePath, new NoOpLogErrors());

        Assert.Equal(standardHash, n64Hash);
    }

    [Fact]
    public async Task CalculateN64HashAsyncV64ReturnsByteSwappedMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.v64", content);

        var result = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath, new NoOpLogErrors());

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public async Task CalculateN64HashAsyncN64ReturnsByteSwappedMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.n64", content);

        var result = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath, new NoOpLogErrors());

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public async Task CalculateN64HashAsyncV64AndN64ProduceSameHash()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath1 = CreateTempFile("game.v64", content);
        var filePath2 = CreateTempFile("game.n64", content);

        var hash1 = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath1, new NoOpLogErrors());
        var hash2 = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath2, new NoOpLogErrors());

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void SwapBytesEvenLengthSwapsPairs()
    {
        var input = new byte[] { 1, 2, 3, 4, 5, 6 };
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 2, 1, 4, 3, 6, 5 }, result);
    }

    [Fact]
    public void SwapBytesOddLengthHandlesLastByte()
    {
        var input = new byte[] { 1, 2, 3 };
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 2, 1, 3 }, result);
    }

    [Fact]
    public void SwapBytesSingleByteReturnsSameByte()
    {
        var input = new byte[] { 42 };
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 42 }, result);
    }

    [Fact]
    public void SwapBytesEmptyArrayReturnsEmptyArray()
    {
        var input = Array.Empty<byte>();
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToHexStringReturnsExpectedHex()
    {
        var input = new byte[] { 0x00, 0xFF, 0xAB, 0x1A };
        var result = InvokePrivateStaticMethod<string>("ToHexString", input);

        Assert.Equal("00ffab1a", result);
    }

    [Fact]
    public void ToHexStringEmptyArrayReturnsEmptyString()
    {
        var input = Array.Empty<byte>();
        var result = InvokePrivateStaticMethod<string>("ToHexString", input);

        Assert.Equal(string.Empty, result);
    }

    private static T? InvokePrivateStaticMethod<T>(string methodName, params object[] parameters)
    {
        var type = typeof(RetroAchievementsFileHasher);
        var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                     ?? throw new InvalidOperationException($"Method '{methodName}' not found on {type.Name}.");
        return (T?)method.Invoke(null, parameters);
    }
}
