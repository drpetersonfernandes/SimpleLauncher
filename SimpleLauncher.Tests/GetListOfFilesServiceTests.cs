using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GetListOfFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="GetListOfFilesService"/> covering file enumeration with various
/// extension filters, recursive/non-recursive modes, and edge cases.
/// </summary>
public class GetListOfFilesServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly GetListOfFilesService _service;

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    public GetListOfFilesServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_GetListOfFiles_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
        _service = new GetListOfFilesService(new NoOpLogErrors());
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
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetFilesAsyncNonExistentDirectoryReturnsEmptyList()
    {
        var result = await _service.GetFilesAsync(@"C:\nonexistent\dir", ["zip"], false, false);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesAsyncEmptyDirectoryReturnsEmptyList()
    {
        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesAsyncFindsFilesWithMatchingExtension()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game1.zip"), "fake");
        File.WriteAllText(Path.Combine(_testDirectory, "game2.zip"), "fake");
        File.WriteAllText(Path.Combine(_testDirectory, "game.nes"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.EndsWith(".zip", f));
    }

    [Fact]
    public async Task GetFilesAsyncFindsFilesWithMultipleExtensions()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.zip"), "fake");
        File.WriteAllText(Path.Combine(_testDirectory, "game.nes"), "fake");
        File.WriteAllText(Path.Combine(_testDirectory, "game.smc"), "fake");
        File.WriteAllText(Path.Combine(_testDirectory, "readme.txt"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip", "nes", "smc"], false, false);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetListOfFilesServiceExtensionFilterMatchesUpperCaseExtension()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.ZIP"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Single(result);
        Assert.All(result, f => Assert.EndsWith(".ZIP", f, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetFilesAsyncRecursiveSearchFindsFilesInSubdirectories()
    {
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);

        File.WriteAllText(Path.Combine(_testDirectory, "game1.zip"), "fake");
        File.WriteAllText(Path.Combine(subDir, "game2.zip"), "fake");

        // Recursive: disableRecursiveSearch=false means recursive
        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetFilesAsyncNonRecursiveSearchDoesNotSearchSubdirectories()
    {
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);

        File.WriteAllText(Path.Combine(_testDirectory, "game1.zip"), "fake");
        File.WriteAllText(Path.Combine(subDir, "game2.zip"), "fake");

        // disableRecursiveSearch=true, groupByFolder=false means non-recursive
        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], true, false);

        Assert.Single(result);
        Assert.Contains("game1.zip", result[0]);
    }

    [Fact]
    public async Task GetFilesAsyncGroupByFolderOverridesDisableRecursiveSearch()
    {
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);

        File.WriteAllText(Path.Combine(_testDirectory, "game1.zip"), "fake");
        File.WriteAllText(Path.Combine(subDir, "game2.zip"), "fake");

        // disableRecursiveSearch=true, groupByFolder=true => doRecurse=true (overrides)
        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], true, true);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetFilesAsyncNoMatchingExtensionsReturnsEmptyList()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.nes"), "fake");
        File.WriteAllText(Path.Combine(_testDirectory, "game.smc"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesAsyncEmptyExtensionListReturnsEmptyList()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.zip"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, [], false, false);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetListOfFilesServiceCancellationThrowsCancellationException()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.zip"), "fake");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.GetFilesAsync(_testDirectory, ["zip"], false, false, cts.Token));
    }

    [Fact]
    public async Task GetFilesAsyncReturnsFullPaths()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.zip"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Single(result);
        Assert.Equal(Path.Combine(_testDirectory, "game.zip"), result[0]);
    }

    [Fact]
    public async Task GetFilesAsyncDeepNestedDirectoryStructure()
    {
        var deepDir = Path.Combine(_testDirectory, "a", "b", "c", "d");
        Directory.CreateDirectory(deepDir);
        File.WriteAllText(Path.Combine(deepDir, "deep.zip"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Single(result);
        Assert.Contains("deep.zip", result[0]);
    }

    [Fact]
    public async Task GetFilesAsyncMultipleFilesInMultipleSubdirectories()
    {
        var sub1 = Path.Combine(_testDirectory, "sub1");
        var sub2 = Path.Combine(_testDirectory, "sub2");
        Directory.CreateDirectory(sub1);
        Directory.CreateDirectory(sub2);

        File.WriteAllText(Path.Combine(_testDirectory, "root.zip"), "fake");
        File.WriteAllText(Path.Combine(sub1, "sub1.zip"), "fake");
        File.WriteAllText(Path.Combine(sub2, "sub2.zip"), "fake");
        File.WriteAllText(Path.Combine(sub2, "sub2.nes"), "fake");

        var result = await _service.GetFilesAsync(_testDirectory, ["zip"], false, false);

        Assert.Equal(3, result.Count);
    }
}
