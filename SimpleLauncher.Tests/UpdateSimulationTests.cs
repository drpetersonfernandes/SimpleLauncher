using System.IO.Compression;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.CheckForUpdates;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Simulates the update extraction logic used by SimpleLauncher.
/// A test ZIP is created in memory, extracted via the real UpdateChecker
/// extraction path, verified, and then every file created during the test
/// is deleted.
/// </summary>
public class UpdateSimulationTests : IDisposable
{
    private readonly string _testDirectory;

    public UpdateSimulationTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_UpdateTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // Aggressive cleanup: delete everything generated during the test
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

    [Fact]
    public void ExtractAllFromZipValidZipExtractsAllFilesSuccessfully()
    {
        // Arrange: build a ZIP in memory that mimics an updater package
        var zipStream = CreateTestZip(new Dictionary<string, string>
        {
            ["Updater.exe"] = "fake updater binary content",
            ["Updater.dll"] = "fake updater dll content",
            ["subfolder/config.json"] = "{\"version\":\"1.0.0\"}",
            ["README.txt"] = "This is a test update package."
        });

        // Act: use the real extraction logic from UpdateChecker
        var result = UpdateChecker.ExtractAllFromZip(zipStream, _testDirectory, null, new NoOpLogErrors());

        // Assert: extraction reported success
        Assert.True(result, "ExtractAllFromZip should return true for a valid ZIP.");

        // Assert: files exist on disk with correct content
        AssertFileContent(Path.Combine(_testDirectory, "Updater.exe"), "fake updater binary content");
        AssertFileContent(Path.Combine(_testDirectory, "Updater.dll"), "fake updater dll content");
        AssertFileContent(Path.Combine(_testDirectory, "subfolder", "config.json"), "{\"version\":\"1.0.0\"}");
        AssertFileContent(Path.Combine(_testDirectory, "README.txt"), "This is a test update package.");
    }

    [Fact]
    public void ExtractAllFromZipEmptyZipReturnsFalse()
    {
        // Arrange: create an empty ZIP
        var zipStream = new MemoryStream();
        using (new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            // No entries — archive is created and disposed for side-effect only
        }

        zipStream.Position = 0;

        // Act
        var result = UpdateChecker.ExtractAllFromZip(zipStream, _testDirectory, null, new NoOpLogErrors());

        // Assert
        Assert.False(result, "ExtractAllFromZip should return false for an empty ZIP.");
    }

    [Fact]
    public void IsNewVersionAvailableLatestGreaterThanCurrentReturnsTrue()
    {
        // Arrange & Act
        var result = InvokeIsNewVersionAvailable("5.3.1", "5.3.2");

        // Assert
        Assert.True(result, "A higher latest version should signal an update is available.");
    }

    [Fact]
    public void IsNewVersionAvailableSameVersionReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable("5.3.2", "5.3.2");
        Assert.False(result, "Same version should not signal an update.");
    }

    [Fact]
    public void IsNewVersionAvailableCurrentGreaterThanLatestReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable("5.3.3", "5.3.2");
        Assert.False(result, "Current newer than latest should not signal an update.");
    }

    [Fact]
    public void NormalizeVersionVariousInputsNormalizesCorrectly()
    {
        // Act & Assert
        Assert.Equal("5.3.2.0", InvokeNormalizeVersion("release5.3.2"));
        Assert.Equal("1.0.0.0", InvokeNormalizeVersion("v1.0"));
        Assert.Equal("10.20.30.40", InvokeNormalizeVersion("10.20.30.40"));
        Assert.Equal("0.0.0.0", InvokeNormalizeVersion(""));
        Assert.Equal("0.0.0.0", InvokeNormalizeVersion(string.Empty));
    }

    [Fact]
    public void ParseVersionAndAssetUrlsFromResponseValidGitHubJsonParsesCorrectly()
    {
        // Arrange
        const string json = """
        {
          "tag_name": "release5.3.2",
          "assets": [
            { "name": "updater_win-x64.zip", "browser_download_url": "https://example.com/updater.zip" },
            { "name": "release_5.3.2_win-x64.zip", "browser_download_url": "https://example.com/release.zip" }
          ]
        }
        """;

        // Act
        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        // Assert
        Assert.Equal("5.3.2.0", version);
        Assert.Equal("https://example.com/release.zip", releaseUrl);
        Assert.Equal("https://example.com/updater.zip", updaterUrl);
    }

    [Fact]
    public void ParseVersionAndAssetUrlsFromResponseMalformedJsonReturnsNulls()
    {
        const string json = "this is not valid json {{{";

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        Assert.Null(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void ParseVersionAndAssetUrlsFromResponseMissingTagNameReturnsNulls()
    {
        const string json = """
        {
          "assets": [
            { "name": "updater_win-x64.zip", "browser_download_url": "https://example.com/updater.zip" }
          ]
        }
        """;

        var (version, _, _) = InvokeParseVersionAndAssetUrls(json);

        Assert.Null(version);
    }

    [Fact]
    public void ParseVersionAndAssetUrlsFromResponseMissingAssetsArrayReturnsNulls()
    {
        const string json = """
        {
          "tag_name": "release5.3.2"
        }
        """;

        var (version, releaseUrl, updaterUrl) = InvokeParseVersionAndAssetUrls(json);

        // Without assets array, the method returns nulls for all fields
        Assert.Null(version);
        Assert.Null(releaseUrl);
        Assert.Null(updaterUrl);
    }

    [Fact]
    public void IsNewVersionAvailableNullCurrentReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable(null, "5.3.2");
        Assert.False(result);
    }

    [Fact]
    public void IsNewVersionAvailableNullLatestReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable("5.3.1", null);
        Assert.False(result);
    }

    [Fact]
    public void IsNewVersionAvailableEmptyStringsReturnsFalse()
    {
        var result = InvokeIsNewVersionAvailable("", "");
        Assert.False(result);
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
        var method = typeof(UpdateChecker).GetMethod("IsNewVersionAvailable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(checker, [current, latest]);
        return (bool)(result ?? throw new InvalidOperationException("Reflection invoke returned null."));
    }

    private static string InvokeNormalizeVersion(string version)
    {
        var method = typeof(UpdateChecker).GetMethod("NormalizeVersion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = method?.Invoke(null, [version]);
        return (string)(result ?? throw new InvalidOperationException("Reflection invoke returned null."));
    }

    private static (string version, string releaseUrl, string updaterUrl) InvokeParseVersionAndAssetUrls(string json)
    {
        var checker = CreateCheckerInstance();
        var method = typeof(UpdateChecker).GetMethod("ParseVersionAndAssetUrlsFromResponse",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(checker, [json]);
        return (ValueTuple<string, string, string>)(result ?? throw new InvalidOperationException("Reflection invoke returned null."));
    }

    private static UpdateChecker CreateCheckerInstance()
    {
        var constructor = typeof(UpdateChecker).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).First();
        var factory = new MockHttpClientFactory();
        return (UpdateChecker)constructor.Invoke([factory, new NoOpLogErrors()]);
    }

    private sealed class MockHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
