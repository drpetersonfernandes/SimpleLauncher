using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameCacheServiceTests : IDisposable
{
    private readonly GameCacheService _cache;

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpDebugLogger : IDebugLogger
    {
        public void Log(string message)
        {
        }

        public void LogException(Exception ex, string? contextMessage = null)
        {
        }
    }

    public GameCacheServiceTests()
    {
        ServiceProviderMock.Install();
        _cache = new GameCacheService(new NoOpLogErrors(), new NoOpDebugLogger());
    }

    public void Dispose()
    {
        _cache.Dispose();
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAllGamesAsyncReturnsEmptyListInitially()
    {
        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSearchResultsAsyncReturnsEmptyListInitially()
    {
        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SetAllGamesAsyncStoresGames()
    {
        var games = new List<string> { "game1.zip", "game2.nes" };
        await _cache.SetAllGamesAsync(games, "NES", CancellationToken.None);

        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
        Assert.Contains("game1.zip", result);
        Assert.Contains("game2.nes", result);
    }

    [Fact]
    public async Task SetSearchResultsAsyncStoresResults()
    {
        var results = new List<string> { "mario.zip", "luigi.zip" };
        await _cache.SetSearchResultsAsync(results, CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsFalseWhenEmpty()
    {
        var result = await _cache.IsCachePopulatedForSystemAsync("NES", CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsTrueForMatchingSystem()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("NES", CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsFalseForDifferentSystem()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("SNES", CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task IsCachePopulatedForSystemAsyncCaseInsensitive()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("nes", CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task InvalidateAsyncClearsCache()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        await _cache.InvalidateAsync(CancellationToken.None);

        var games = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(games);
    }

    [Fact]
    public async Task InvalidateAsyncClearsSearchResults()
    {
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);
        await _cache.InvalidateAsync(CancellationToken.None);

        var results = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    [Fact]
    public void ClearSyncDoesNotThrow()
    {
        var exception = Record.Exception(() => _cache.ClearSync());
        Assert.Null(exception);
    }

    [Fact]
    public async Task ClearSyncClearsData()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        _cache.ClearSync();

        var games = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetAllGamesAsyncReturnsCopy()
    {
        var games = new List<string> { "game1.zip" };
        await _cache.SetAllGamesAsync(games, "NES", CancellationToken.None);

        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        result.Add("modified.zip");

        var result2 = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Single(result2);
    }

    [Fact]
    public async Task GetSearchResultsAsyncReturnsCopy()
    {
        var results = new List<string> { "mario.zip" };
        await _cache.SetSearchResultsAsync(results, CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        result.Add("modified.zip");

        var result2 = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Single(result2);
    }

    [Fact]
    public async Task GetResortSourceAsyncWithoutFilterReturnsAllGames()
    {
        var games = new List<string> { "game1.zip", "game2.nes" };
        await _cache.SetAllGamesAsync(games, "NES", CancellationToken.None);

        var (allGames, _) = await _cache.GetResortSourceAsync(false, CancellationToken.None);
        Assert.Equal(2, allGames.Count);
    }

    [Fact]
    public async Task GetResortSourceAsyncWithFilterReturnsSearchResults()
    {
        await _cache.SetAllGamesAsync(["game1.zip", "game2.nes"], "NES", CancellationToken.None);
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var (source, _) = await _cache.GetResortSourceAsync(true, CancellationToken.None);
        Assert.Single(source);
        Assert.Equal("mario.zip", source[0]);
    }

    [Fact]
    public void SelectedSystemDefaultsToEmpty()
    {
        Assert.Equal("", _cache.SelectedSystem);
    }

    [Fact]
    public async Task SelectedSystemUpdatesOnSetAllGames()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "SNES", CancellationToken.None);
        Assert.Equal("SNES", _cache.SelectedSystem);
    }

    [Fact]
    public void DisposeDoesNotThrow()
    {
        var cache = new GameCacheService(new NoOpLogErrors(), new NoOpDebugLogger());
        var exception = Record.Exception(cache.Dispose);
        Assert.Null(exception);
    }

    [Fact]
    public async Task SetAllGamesWithEmptyList()
    {
        await _cache.SetAllGamesAsync([], "NES", CancellationToken.None);
        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(result);
    }
}
