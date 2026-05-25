using System.Reflection;
using System.Text;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class RetroAchievementsFileHasherTests : IDisposable
{
    private readonly string _testDirectory;

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
    public async Task CalculateStandardMd5Async_ReturnsExpectedHash()
    {
        var content = "Hello World"u8.ToArray();
        var filePath = CreateTempFile("test.bin", content);

        var result = await RetroAchievementsFileHasher.CalculateStandardMd5Async(filePath);

        Assert.Equal("b10a8db164e0754105b7a99be72e3fe5", result);
    }

    [Fact]
    public async Task CalculateStandardMd5Async_EmptyFile_ReturnsEmptyHash()
    {
        var filePath = CreateTempFile("empty.bin", []);

        var result = await RetroAchievementsFileHasher.CalculateStandardMd5Async(filePath);

        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", result);
    }

    [Fact]
    public void CalculateFilenameHash_ReturnsExpectedHash()
    {
        var result = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\roms\game.zip");

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void CalculateFilenameHash_SameFilenameWithoutExtension_ProducesSameHash()
    {
        var hash1 = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\folder\mygame.zip");
        var hash2 = RetroAchievementsFileHasher.CalculateFilenameHash(@"D:\other\mygame.7z");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CalculateFilenameHash_DifferentFilenames_ProduceDifferentHashes()
    {
        var hash1 = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\game1.zip");
        var hash2 = RetroAchievementsFileHasher.CalculateFilenameHash(@"C:\game2.zip");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task CalculateArduboyHashAsync_NormalizesLineEndings()
    {
        var filePath1 = CreateTempFile("arduboy_crlf.hex", "hello\r\nworld\n");
        var filePath2 = CreateTempFile("arduboy_lf.hex", "hello\nworld\n");

        var hash1 = await RetroAchievementsFileHasher.CalculateArduboyHashAsync(filePath1);
        var hash2 = await RetroAchievementsFileHasher.CalculateArduboyHashAsync(filePath2);

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async Task CalculateN64HashAsync_Z64_ReturnsStandardMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.z64", content);

        var n64Hash = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath);
        var standardHash = await RetroAchievementsFileHasher.CalculateStandardMd5Async(filePath);

        Assert.Equal(standardHash, n64Hash);
    }

    [Fact]
    public async Task CalculateN64HashAsync_V64_ReturnsByteSwappedMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.v64", content);

        var result = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath);

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public async Task CalculateN64HashAsync_N64_ReturnsByteSwappedMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.n64", content);

        var result = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath);

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public async Task CalculateN64HashAsync_V64AndN64_ProduceSameHash()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath1 = CreateTempFile("game.v64", content);
        var filePath2 = CreateTempFile("game.n64", content);

        var hash1 = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath1);
        var hash2 = await RetroAchievementsFileHasher.CalculateN64HashAsync(filePath2);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void SwapBytes_EvenLength_SwapsPairs()
    {
        var input = new byte[] { 1, 2, 3, 4, 5, 6 };
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 2, 1, 4, 3, 6, 5 }, result);
    }

    [Fact]
    public void SwapBytes_OddLength_HandlesLastByte()
    {
        var input = new byte[] { 1, 2, 3 };
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 2, 1, 3 }, result);
    }

    [Fact]
    public void SwapBytes_SingleByte_ReturnsSameByte()
    {
        var input = new byte[] { 42 };
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 42 }, result);
    }

    [Fact]
    public void SwapBytes_EmptyArray_ReturnsEmptyArray()
    {
        var input = Array.Empty<byte>();
        var result = InvokePrivateStaticMethod<byte[]>("SwapBytes", input);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToHexString_ReturnsExpectedHex()
    {
        var input = new byte[] { 0x00, 0xFF, 0xAB, 0x1A };
        var result = InvokePrivateStaticMethod<string>("ToHexString", input);

        Assert.Equal("00ffab1a", result);
    }

    [Fact]
    public void ToHexString_EmptyArray_ReturnsEmptyString()
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
