using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="RetroAchievementsHashScanner"/> covering background scanning,
/// JSON persistence through the hash store, and prevention of parallel scans.
/// </summary>
public class RetroAchievementsHashScannerTests : IDisposable
{
    private readonly string _tempFolder;
    private readonly string _romsFolder;
    private readonly RetroAchievementsHashStore _store;
    private readonly RetroAchievementsSystemMatcher _systemMatcher;
    private readonly FakeFileHasher _fileHasher;
    private readonly FakeGetListOfFilesService _getListOfFiles;
    private readonly RetroAchievementsHashScanner _scanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsHashScannerTests"/> class
    /// with an isolated temporary ROM and hash folder.
    /// </summary>
    public RetroAchievementsHashScannerTests()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncherHashScannerTests", Guid.NewGuid().ToString("N"));
        _romsFolder = Path.Combine(_tempFolder, "Roms");
        Directory.CreateDirectory(_romsFolder);

        _store = new RetroAchievementsHashStore(new NoOpLogger(), Path.Combine(_tempFolder, "Hashes"));
        _systemMatcher = new RetroAchievementsSystemMatcher(new NoOpLogger(), new NoOpLogger());
        _fileHasher = new FakeFileHasher();
        _getListOfFiles = new FakeGetListOfFilesService();
        _scanner = new RetroAchievementsHashScanner(
            new NoOpLogger(), _systemMatcher, _fileHasher, _getListOfFiles, _store);
    }

    /// <summary>
    /// Deletes the temporary folders used by the tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, true);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that scanning a system writes a JSON file containing a hash for every game file.
    /// </summary>
    [Fact]
    public async Task ScanSystemAsync_CalculatesAndPersistsHashes()
    {
        File.WriteAllText(Path.Combine(_romsFolder, "Game 1.zip"), "rom1");
        File.WriteAllText(Path.Combine(_romsFolder, "Game 2.zip"), "rom2");

        var completedSystems = new List<string>();
        var started = await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false,
            onCompleted: completedSystems.Add);

        Assert.True(started);
        Assert.Contains("Nintendo 64", completedSystems, StringComparer.OrdinalIgnoreCase);

        var loaded = _store.LoadSystemHashes("Nintendo 64");
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Hashes.Count);
        Assert.Equal(2, loaded.FileCount);
        Assert.Equal("hash-Game 1", loaded.Hashes[Path.Combine(_romsFolder, "Game 1.zip")]);
        Assert.Equal("hash-Game 2", loaded.Hashes[Path.Combine(_romsFolder, "Game 2.zip")]);
    }

    /// <summary>
    /// Verifies that scanning a system again with the same game count skips re-hashing.
    /// </summary>
    [Fact]
    public async Task ScanSystemAsync_WhenGameCountUnchanged_SkipsReHashing()
    {
        File.WriteAllText(Path.Combine(_romsFolder, "Game 1.zip"), "rom1");

        await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false);

        Assert.Equal(1, _fileHasher.HashCallCount);

        var completedSystems = new List<string>();
        await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false,
            onCompleted: completedSystems.Add);

        Assert.Equal(1, _fileHasher.HashCallCount);
        Assert.Empty(completedSystems);
        Assert.Single(_store.LoadSystemHashes("Nintendo 64")!.Hashes);
    }

    /// <summary>
    /// Verifies that scanning a system again after a game was added recalculates the hashes.
    /// </summary>
    [Fact]
    public async Task ScanSystemAsync_WhenGameCountChanged_ReHashes()
    {
        File.WriteAllText(Path.Combine(_romsFolder, "Game 1.zip"), "rom1");

        await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false);

        Assert.Equal(1, _fileHasher.HashCallCount);

        File.WriteAllText(Path.Combine(_romsFolder, "Game 2.zip"), "rom2");

        var completedSystems = new List<string>();
        await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false,
            onCompleted: completedSystems.Add);

        Assert.Equal(3, _fileHasher.HashCallCount);
        Assert.Contains("Nintendo 64", completedSystems, StringComparer.OrdinalIgnoreCase);

        var loaded = _store.LoadSystemHashes("Nintendo 64");
        Assert.Equal(2, loaded!.Hashes.Count);
        Assert.Equal(2, loaded.FileCount);
    }

    /// <summary>
    /// Verifies that files that cannot be hashed are skipped and an empty result is still persisted.
    /// </summary>
    [Fact]
    public async Task ScanSystemAsync_WhenHashFails_SkipsFileButPersistsResult()
    {
        File.WriteAllText(Path.Combine(_romsFolder, "Broken.zip"), "broken");

        _fileHasher.FailAll = true;

        var started = await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false);

        Assert.True(started);
        Assert.True(_store.HasSystemHashes("Nintendo 64"));
        Assert.Empty(_store.LoadSystemHashes("Nintendo 64")!.Hashes);
    }

    /// <summary>
    /// Verifies that a second scan request is rejected while a scan is already running.
    /// </summary>
    [Fact]
    public async Task ScanSystemAsync_SecondCallWhileScanning_IsRejected()
    {
        File.WriteAllText(Path.Combine(_romsFolder, "Game.zip"), "rom");

        _fileHasher.BlockCalls = true;

        var firstTask = _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false);

        // The scan flag is set synchronously, so the second call is rejected immediately.
        var secondStarted = await _scanner.ScanSystemAsync(
            "Nintendo 64",
            [_romsFolder],
            [".zip"],
            disableRecursiveSearch: true,
            groupByFolder: false);

        Assert.False(secondStarted);
        Assert.True(_scanner.IsScanning);

        _fileHasher.ReleaseBlock();
        await firstTask;
        Assert.False(_scanner.IsScanning);
    }

    /// <summary>
    /// Verifies that systems without a usable RetroAchievements console ID are reported as not scannable.
    /// </summary>
    [Fact]
    public void IsSystemScannable_SupportsOnlyKnownSystems()
    {
        Assert.True(_scanner.IsSystemScannable("Nintendo 64"));
        Assert.True(_scanner.IsSystemScannable("NES"));
        Assert.False(_scanner.IsSystemScannable("Microsoft Windows"));
        Assert.False(_scanner.IsSystemScannable(""));
    }

    /// <summary>
    /// Verifies that scanning all systems skips unsupported systems and only persists
    /// results for the supported ones.
    /// </summary>
    [Fact]
    public async Task ScanAllSystemsAsync_SkipsUnsupportedSystems()
    {
        File.WriteAllText(Path.Combine(_romsFolder, "Game.zip"), "rom");

        var targets = new List<RaHashScanTarget>
        {
            new()
            {
                SystemName = "Nintendo 64",
                SystemFolders = [_romsFolder],
                FileFormatsToSearch = [".zip"],
                DisableRecursiveSearch = true
            },
            new()
            {
                SystemName = "Microsoft Windows",
                SystemFolders = [_romsFolder],
                FileFormatsToSearch = [".zip"],
                DisableRecursiveSearch = true
            }
        };

        var completedSystems = new List<string>();
        var started = await _scanner.ScanAllSystemsAsync(targets, completedSystems.Add);

        Assert.True(started);
        Assert.Contains("Nintendo 64", completedSystems, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("Microsoft Windows", completedSystems, StringComparer.OrdinalIgnoreCase);

        Assert.True(_store.HasSystemHashes("Nintendo 64"));
        Assert.False(_store.HasSystemHashes("Microsoft Windows"));
    }

    /// <summary>
    /// A fake file hasher that returns a deterministic hash per file name.
    /// </summary>
    private sealed class FakeFileHasher : IRetroAchievementsFileHasher
    {
        private readonly TaskCompletionSource<bool> _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Gets the number of times <see cref="CalculateHashAsync"/> was called.
        /// </summary>
        public int HashCallCount { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether every hash calculation returns null.
        /// </summary>
        public bool FailAll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hash calls block until <see cref="ReleaseBlock"/> is called.
        /// </summary>
        public bool BlockCalls { get; set; }

        public async Task<string?> CalculateHashAsync(string filePath, string systemName)
        {
            HashCallCount++;

            if (BlockCalls)
            {
                await _gate.Task;
            }

            if (FailAll) return null;

            return $"hash-{Path.GetFileNameWithoutExtension(filePath)}";
        }

        public void ReleaseBlock()
        {
            _gate.TrySetResult(true);
        }
    }

    /// <summary>
    /// A fake file enumeration service that lists files by extension from a single folder.
    /// </summary>
    private sealed class FakeGetListOfFilesService : IGetListOfFilesService
    {
        public Task<IList<string>> GetFilesAsync(
            string directoryPath,
            IList<string> fileExtensions,
            bool disableRecursiveSearch,
            bool groupByFolder,
            CancellationToken cancellationToken = default)
        {
            var files = Directory
                .EnumerateFiles(directoryPath, "*", disableRecursiveSearch ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories)
                .Where(f => fileExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult<IList<string>>(files);
        }
    }
}