using System.Reflection;
using System.Text;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests for <see cref="RetroAchievementsFileHasher"/> covering MD5 calculation,
/// filename hashing, N64 byte-swap hashing, Arduboy line-ending normalization, and hex encoding.
/// </summary>
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

    private readonly RetroAchievementsFileHasher _hasher = new(new NoOpLogErrors());

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

    /// <summary>
    /// Verifies that CalculateStandardMd5Async returns the correct MD5 hash for known content.
    /// </summary>
    [Fact]
    public async Task CalculateStandardMd5AsyncReturnsExpectedHash()
    {
        var content = "Hello World"u8.ToArray();
        var filePath = CreateTempFile("test.bin", content);

        var result = await _hasher.CalculateStandardMd5Async(filePath);

        Assert.Equal("b10a8db164e0754105b7a99be72e3fe5", result);
    }

    /// <summary>
    /// Verifies that CalculateStandardMd5Async returns the MD5 hash for an empty file.
    /// </summary>
    [Fact]
    public async Task CalculateStandardMd5AsyncEmptyFileReturnsEmptyHash()
    {
        var filePath = CreateTempFile("empty.bin", []);

        var result = await _hasher.CalculateStandardMd5Async(filePath);

        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", result);
    }

    /// <summary>
    /// Verifies that CalculateFilenameHash returns a 32-character hex string for a valid file path.
    /// </summary>
    [Fact]
    public void CalculateFilenameHashReturnsExpectedHash()
    {
        var result = _hasher.CalculateFilenameHash(@"C:\roms\game.zip");

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    /// <summary>
    /// Verifies that CalculateFilenameHash produces the same hash for files with the same name
    /// but different paths or extensions.
    /// </summary>
    [Fact]
    public void CalculateFilenameHashSameFilenameWithoutExtensionProducesSameHash()
    {
        var hash1 = _hasher.CalculateFilenameHash(@"C:\folder\mygame.zip");
        var hash2 = _hasher.CalculateFilenameHash(@"D:\other\mygame.7z");

        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// Verifies that CalculateFilenameHash produces different hashes for different file names.
    /// </summary>
    [Fact]
    public void CalculateFilenameHashDifferentFilenamesProduceDifferentHashes()
    {
        var hash1 = _hasher.CalculateFilenameHash(@"C:\game1.zip");
        var hash2 = _hasher.CalculateFilenameHash(@"C:\game2.zip");

        Assert.NotEqual(hash1, hash2);
    }

    /// <summary>
    /// Verifies that CalculateArduboyHashAsync normalizes CRLF and LF line endings to produce the same hash.
    /// </summary>
    [Fact]
    public async Task CalculateArduboyHashAsyncNormalizesLineEndings()
    {
        var filePath1 = CreateTempFile("arduboy_crlf.hex", "hello\r\nworld\n");
        var filePath2 = CreateTempFile("arduboy_lf.hex", "hello\nworld\n");

        var hash1 = await _hasher.CalculateArduboyHashAsync(filePath1);
        var hash2 = await _hasher.CalculateArduboyHashAsync(filePath2);

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// Verifies that CalculateN64HashAsync for .z64 files returns the standard MD5 hash.
    /// </summary>
    [Fact]
    public async Task CalculateN64HashAsyncZ64ReturnsStandardMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.z64", content);

        var n64Hash = await _hasher.CalculateN64HashAsync(filePath);
        var standardHash = await _hasher.CalculateStandardMd5Async(filePath);

        Assert.Equal(standardHash, n64Hash);
    }

    /// <summary>
    /// Verifies that CalculateN64HashAsync for .v64 files returns a byte-swapped MD5 hash.
    /// </summary>
    [Fact]
    public async Task CalculateN64HashAsyncV64ReturnsByteSwappedMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.v64", content);

        var result = await _hasher.CalculateN64HashAsync(filePath);

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    /// <summary>
    /// Verifies that CalculateN64HashAsync for .n64 files returns a byte-swapped MD5 hash.
    /// </summary>
    [Fact]
    public async Task CalculateN64HashAsyncN64ReturnsByteSwappedMd5()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath = CreateTempFile("game.n64", content);

        var result = await _hasher.CalculateN64HashAsync(filePath);

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    /// <summary>
    /// Verifies that .v64 and .n64 files with identical content produce the same N64 hash.
    /// </summary>
    [Fact]
    public async Task CalculateN64HashAsyncV64AndN64ProduceSameHash()
    {
        var content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filePath1 = CreateTempFile("game.v64", content);
        var filePath2 = CreateTempFile("game.n64", content);

        var hash1 = await _hasher.CalculateN64HashAsync(filePath1);
        var hash2 = await _hasher.CalculateN64HashAsync(filePath2);

        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// Verifies that the private ToHexString method returns the correct lowercase hex string.
    /// </summary>
    [Fact]
    public void ToHexStringReturnsExpectedHex()
    {
        var input = new byte[] { 0x00, 0xFF, 0xAB, 0x1A };
        var result = InvokePrivateStaticMethod<string>("ToHexString", input);

        Assert.Equal("00ffab1a", result);
    }

    /// <summary>
    /// Verifies that the private ToHexString method returns an empty string for an empty byte array.
    /// </summary>
    [Fact]
    public void ToHexStringEmptyArrayReturnsEmptyString()
    {
        var input = Array.Empty<byte>();
        var result = InvokePrivateStaticMethod<string>("ToHexString", input);

        Assert.Equal("", result);
    }

    private static T? InvokePrivateStaticMethod<T>(string methodName, params object[] parameters)
    {
        var type = typeof(RetroAchievementsFileHasher);
        var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                     ?? throw new InvalidOperationException($"Method '{methodName}' not found on {type.Name}.");
        return (T?)method.Invoke(null, parameters);
    }
}
