using System.IO.Compression;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;
using CheckForUpdatesService = SimpleLauncher.Services.CheckForUpdatesService;

namespace SimpleLauncher.Tests;

public class CheckForUpdatesTests : IDisposable
{
    private readonly string _testDirectory;

    public CheckForUpdatesTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_CheckUpdateTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    // ------------------------------------------------------------------
    // IsNewVersionAvailable
    // ------------------------------------------------------------------

    [Fact]
    public void IsNewVersionAvailableFourPartVersionsReturnsTrue()
    {
        var result = InvokeIsNewVersionAvailable("5.6.0.0", "5.6.1.0");
        Assert.True(result);
    }

    [Fact]
    public void IsNewVersionAvailableFourPartVersionsEqualReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable("5.6.0.0", "5.6.0.0");
        Assert.False(result);
    }

    [Fact]
    public void IsNewVersionAvailableMajorDifferenceReturnsTrue()
    {
        var result = InvokeIsNewVersionAvailable("4.9.9.9", "5.0.0.0");
        Assert.True(result);
    }

    [Fact]
    public void IsNewVersionAvailableMinorDifferenceReturnsTrue()
    {
        var result = InvokeIsNewVersionAvailable("5.5.9.0", "5.6.0.0");
        Assert.True(result);
    }

    [Fact]
    public void IsNewVersionAvailablePatchDifferenceReturnsTrue()
    {
        var result = InvokeIsNewVersionAvailable("5.6.0.0", "5.6.0.1");
        Assert.True(result);
    }

    [Fact]
    public void IsNewVersionAvailableCurrentNewerMajorReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable("6.0.0.0", "5.9.9.9");
        Assert.False(result);
    }

    [Theory]
    [InlineData("release5.3.2", "5.3.3")]
    [InlineData("v5.3.2", "5.3.3")]
    [InlineData("5.3.2", "release5.3.3")]
    public void IsNewVersionAvailablePrefixedVersionsReturnsTrue(string current, string latest)
    {
        var result = InvokeIsNewVersionAvailable(current, latest);
        Assert.True(result);
    }

    [Theory]
    [InlineData("release5.3.2", "release5.3.2")]
    [InlineData("v5.3.2", "v5.3.2")]
    public void IsNewVersionAvailableSamePrefixedVersionsReturnsFalse(string current, string latest)
    {
        var result = InvokeIsNewVersionAvailable(current, latest);
        Assert.False(result);
    }

    // ------------------------------------------------------------------
    // NormalizeVersion
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("release5.6.0", "5.6.0.0")]
    [InlineData("v5.6.0", "5.6.0.0")]
    [InlineData("5.6", "5.6.0.0")]
    [InlineData("5", "5.0.0.0")]
    [InlineData("10.20.30.40", "10.20.30.40")]
    [InlineData("release1.2.3.4", "1.2.3.4")]
    [InlineData("v0.1", "0.1.0.0")]
    public void NormalizeVersionVariousFormatsReturnsExpected(string input, string expected)
    {
        Assert.Equal(expected, InvokeNormalizeVersion(input));
    }

    [Fact]
    public void NormalizeVersionNullReturnsZeroes()
    {
        Assert.Equal("0.0.0.0", InvokeNormalizeVersion(null!));
    }

    [Fact]
    public void NormalizeVersionWhitespaceOnlyReturnsZeroes()
    {
        Assert.Equal("0.0.0.0", InvokeNormalizeVersion("   "));
    }

    [Fact]
    public void NormalizeVersionOnlyPrefixReturnsZeroes()
    {
        Assert.Equal("0.0.0.0", InvokeNormalizeVersion("release"));
    }

    // ------------------------------------------------------------------
    // ParseVersionAndAssetUrlsFromResponse
    // ------------------------------------------------------------------

    [Fact]
    public void ParseResponseWithBothAssetsReturnsAllUrls()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": [
            { "name": "updater_win-x64.zip", "browser_download_url": "https://example.com/updater-x64.zip" },
            { "name": "release_5.6.0_win-x64.zip", "browser_download_url": "https://example.com/release-x64.zip" },
            { "name": "updater_win-arm64.zip", "browser_download_url": "https://example.com/updater-arm64.zip" },
            { "name": "release_5.6.0_win-arm64.zip", "browser_download_url": "https://example.com/release-arm64.zip" }
          ]
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.NotNull(version);
        Assert.NotNull(releaseUrl);
        Assert.NotNull(updaterUrl);
        Assert.StartsWith("5.6.0", version, StringComparison.Ordinal);
        Assert.Contains("updater", updaterUrl, StringComparison.Ordinal);
        Assert.Contains("release", releaseUrl, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseResponseWithEmptyAssetsArrayReturnsVersionOnly()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": []
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.NotNull(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithUnrelatedAssetsReturnsNullsForUrls()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": [
            { "name": "source.zip", "browser_download_url": "https://example.com/source.zip" },
            { "name": "readme.txt", "browser_download_url": "https://example.com/readme.txt" }
          ]
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.NotNull(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithMissingBrowserDownloadUrlReturnsNull()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": [
            { "name": "updater_win-x64.zip" }
          ]
        }
        """;

        var (version, _, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.NotNull(version);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithEmptyTagNameReturnsNulls()
    {
        const string json = """
        {
          "tag_name": "",
          "assets": [
            { "name": "updater_win-x64.zip", "browser_download_url": "https://example.com/updater.zip" }
          ]
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.Null(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithAssetsNotArrayReturnsNulls()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": "not an array"
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.Null(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithUpdaterOnlyReturnsNullForRelease()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": [
            { "name": "updater_win-x64.zip", "browser_download_url": "https://example.com/updater.zip" }
          ]
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.NotNull(version);
        Assert.Null(releaseUrl);
        Assert.NotNull(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithReleaseOnlyReturnsNullForUpdater()
    {
        const string json = """
        {
          "tag_name": "release5.6.0",
          "assets": [
            { "name": "release_5.6.0_win-x64.zip", "browser_download_url": "https://example.com/release.zip" }
          ]
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.NotNull(version);
        Assert.NotNull(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseResponseWithVersionOnlyPrefixReturnsNulls()
    {
        const string json = """
        {
          "tag_name": "release",
          "assets": [
            { "name": "updater_win-x64.zip", "browser_download_url": "https://example.com/updater.zip" }
          ]
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.Null(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    // ------------------------------------------------------------------
    // ExtractAllFromZip
    // ------------------------------------------------------------------

    [Fact]
    public void ExtractAllFromZipWithPathTraversalReturnsFalse()
    {
        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("..\\..\\malicious.txt", CompressionLevel.Fastest);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            writer.Write("malicious content");
        }

        zipStream.Position = 0;

        var result = CheckForUpdatesService.ExtractAllFromZip(zipStream, _testDirectory, null!, new NoOpLogger());

        Assert.False(result, "ExtractAllFromZip should return false for path traversal entries.");
    }

    [Fact]
    public void ExtractAllFromZipWithNestedDirectoriesExtractsCorrectly()
    {
        var zipStream = CreateTestZip(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["level1/level2/level3/deep.txt"] = "deep content",
            ["level1/shallow.txt"] = "shallow content"
        });

        var result = CheckForUpdatesService.ExtractAllFromZip(zipStream, _testDirectory, null!, new NoOpLogger());

        Assert.True(result);
        AssertFileContent(Path.Combine(_testDirectory, "level1", "level2", "level3", "deep.txt"), "deep content");
        AssertFileContent(Path.Combine(_testDirectory, "level1", "shallow.txt"), "shallow content");
    }

    [Fact]
    public void ExtractAllFromZipWithSingleFileExtractsSuccessfully()
    {
        var zipStream = CreateTestZip(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["single.txt"] = "only file"
        });

        var result = CheckForUpdatesService.ExtractAllFromZip(zipStream, _testDirectory, null!, new NoOpLogger());

        Assert.True(result);
        AssertFileContent(Path.Combine(_testDirectory, "single.txt"), "only file");
    }

    [Fact]
    public void ExtractAllFromZipWithLargeContentExtractsSuccessfully()
    {
        var largeContent = new string('X', 100_000);
        var zipStream = CreateTestZip(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["large.bin"] = largeContent
        });

        var result = CheckForUpdatesService.ExtractAllFromZip(zipStream, _testDirectory, null!, new NoOpLogger());

        Assert.True(result);
        AssertFileContent(Path.Combine(_testDirectory, "large.bin"), largeContent);
    }

    [Fact]
    public void ExtractAllFromZipOverwritesExistingFiles()
    {
        var existingFile = Path.Combine(_testDirectory, "overwrite.txt");
        File.WriteAllText(existingFile, "old content");

        var zipStream = CreateTestZip(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["overwrite.txt"] = "new content"
        });

        var result = CheckForUpdatesService.ExtractAllFromZip(zipStream, _testDirectory, null!, new NoOpLogger());

        Assert.True(result);
        AssertFileContent(existingFile, "new content");
    }

    [Fact]
    public void ExtractAllFromZipCorruptedStreamReturnsFalse()
    {
        var corruptedStream = new MemoryStream(("PK\x03\x04c"u8 + "orrupted data"u8).ToArray());

        var result = CheckForUpdatesService.ExtractAllFromZip(corruptedStream, _testDirectory, null!, new NoOpLogger());

        Assert.False(result, "ExtractAllFromZip should return false for a corrupted stream.");
    }

    // ------------------------------------------------------------------
    // GetLatestUpdaterInfoAsync — real GitHub API
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetLatestUpdaterInfoAsyncFromGitHubReturnsVersionAndUrl()
    {
        var service = CreateCheckerInstance();

        var (updaterUrl, version) = await service.GetLatestUpdaterInfoAsync();

        Assert.NotNull(version);
        Assert.NotNull(updaterUrl);
        Assert.NotEmpty(version);
        Assert.NotEmpty(updaterUrl);
        Assert.StartsWith("5.", version, StringComparison.Ordinal);
        Assert.Contains("updater", updaterUrl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("github", updaterUrl, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------------
    // DownloadUpdateFileToMemoryAsync — real download from GitHub
    // ------------------------------------------------------------------

    [Fact]
    public async Task DownloadUpdateFileToMemoryAsyncFromGitHubDownloadsContent()
    {
        var service = CreateCheckerInstance();

        var (updaterUrl, _) = await service.GetLatestUpdaterInfoAsync();
        Assert.NotNull(updaterUrl);

        using var memoryStream = new MemoryStream();
        await service.DownloadUpdateFileToMemoryAsync(updaterUrl, memoryStream);

        Assert.True(memoryStream.Length > 0, "Downloaded content should not be empty.");
        memoryStream.Position = 0;

        // Verify it's a valid ZIP (PK header)
        var header = new byte[2];
        _ = await memoryStream.ReadAsync(header);
        Assert.Equal(0x50, header[0]); // 'P'
        Assert.Equal(0x4B, header[1]); // 'K'
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static MemoryStream CreateTestZip(Dictionary<string, string> entries)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var kvp in entries)
            {
                var entry = archive.CreateEntry(kvp.Key, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write(kvp.Value);
            }
        }

        stream.Position = 0;
        return stream;
    }

    private static void AssertFileContent(string path, string expectedContent)
    {
        Assert.True(File.Exists(path), $"Expected file to exist: {path}");
        var actual = File.ReadAllText(path);
        Assert.Equal(expectedContent, actual);
    }

    private static bool InvokeIsNewVersionAvailable(string? current, string? latest)
    {
        var checker = CreateCheckerInstance();
        var method = typeof(CheckForUpdatesService).GetMethod("IsNewVersionAvailable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(checker, [current, latest]);
        return (bool)(result ?? throw new InvalidOperationException("Reflection invoke returned null."));
    }

    private static string InvokeNormalizeVersion(string version)
    {
        var method = typeof(CheckForUpdatesService).GetMethod("NormalizeVersion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = method?.Invoke(null, [version]);
        return (string)(result ?? throw new InvalidOperationException("Reflection invoke returned null."));
    }

    private static (string? version, string? releaseUrl, string? updaterUrl) InvokeParseVersionAndAssetUrls(string json)
    {
        var checker = CreateCheckerInstance();
        var method = typeof(CheckForUpdatesService).GetMethod("ParseVersionAndAssetUrlsFromResponse",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(checker, [json]);
        return ((string?, string?, string?))(result ?? throw new InvalidOperationException("Reflection invoke returned null."));
    }

    private static CheckForUpdatesService CreateCheckerInstance()
    {
        var constructor = typeof(CheckForUpdatesService).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).First();
        var factory = new RealHttpClientFactory();
        var logErrors = new NoOpLogger();
        return (CheckForUpdatesService)constructor.Invoke([factory, null, null, logErrors, null, null]);
    }

    private sealed class RealHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}
