using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="RetroAchievementsHashStore" /> covering JSON persistence of
///     per-system RetroAchievements hash scans in a temporary folder.
/// </summary>
public class RetroAchievementsHashStoreTests : IDisposable
{
    private readonly RetroAchievementsHashStore _store;
    private readonly string _tempFolder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetroAchievementsHashStoreTests" /> class
    ///     with an isolated temporary hash folder.
    /// </summary>
    public RetroAchievementsHashStoreTests()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncherHashStoreTests", Guid.NewGuid().ToString("N"));
        _store = new RetroAchievementsHashStore(new NoOpLogger(), _tempFolder);
    }

    /// <summary>
    ///     Deletes the temporary hash folder used by the tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempFolder)) Directory.Delete(_tempFolder, true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Verifies that a saved hash scan can be loaded back with the same values.
    /// </summary>
    [Fact]
    public void SaveThenLoad_RoundTripsHashes()
    {
        var data = new RaSystemHashes
        {
            SystemName = "Nintendo 64",
            ScannedAtUtc = new DateTime(2026, 8, 16, 10, 0, 0, DateTimeKind.Utc),
            FileCount = 2,
            Hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [@"C:\roms\N64\Game.zip"] = "abc123",
                [@"C:\roms\N64\Other.zip"] = "def456"
            }
        };

        _store.SaveSystemHashes(data);

        var loaded = _store.LoadSystemHashes("Nintendo 64");

        Assert.NotNull(loaded);
        Assert.Equal("Nintendo 64", loaded.SystemName);
        Assert.Equal(data.ScannedAtUtc, loaded.ScannedAtUtc);
        Assert.Equal(2, loaded.FileCount);
        Assert.Equal(2, loaded.Hashes.Count);
        Assert.Equal("abc123", loaded.Hashes[@"C:\roms\N64\Game.zip"]);
        Assert.Equal("def456", loaded.Hashes[@"C:\roms\N64\Other.zip"]);
    }

    /// <summary>
    ///     Verifies that HasSystemHashes is true after saving and false before any save.
    /// </summary>
    [Fact]
    public void HasSystemHashes_ReflectsSavedFiles()
    {
        Assert.False(_store.HasSystemHashes("NES"));

        _store.SaveSystemHashes(new RaSystemHashes { SystemName = "NES" });

        Assert.True(_store.HasSystemHashes("NES"));
    }

    /// <summary>
    ///     Verifies that loading a system that was never scanned returns null.
    /// </summary>
    [Fact]
    public void LoadSystemHashes_WhenFileMissing_ReturnsNull()
    {
        Assert.Null(_store.LoadSystemHashes("NeverScannedSystem"));
    }

    /// <summary>
    ///     Verifies that system names with characters that are invalid in file names
    ///     are sanitized when building the JSON file name.
    /// </summary>
    [Fact]
    public void GetSystemHashFilePath_SanitizesInvalidCharacters()
    {
        var path = _store.GetSystemHashFilePath("Arcade: Test/System?");

        Assert.EndsWith(".json", path, StringComparison.OrdinalIgnoreCase);

        var fileName = Path.GetFileName(path);
        Assert.DoesNotContain(':', fileName);
        Assert.DoesNotContain('/', fileName);
        Assert.DoesNotContain('?', fileName);
    }

    /// <summary>
    ///     Verifies that a corrupted JSON file loads as null instead of throwing.
    /// </summary>
    [Fact]
    public void LoadSystemHashes_WhenFileCorrupted_ReturnsNull()
    {
        var filePath = _store.GetSystemHashFilePath("NES");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "this is not valid json {{");

        Assert.Null(_store.LoadSystemHashes("NES"));
    }
}