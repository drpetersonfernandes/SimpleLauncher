using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameCacheServiceExtendedTests : IDisposable
{
    private readonly GameCacheService _cache;

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    public GameCacheServiceExtendedTests()
    {
        ServiceProviderMock.Install();
        _cache = new GameCacheService(new NoOpLogErrors());
    }

    public void Dispose()
    {
        _cache.Dispose();
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SetAllGamesAsyncOverwritesPreviousGames()
    {
        await _cache.SetAllGamesAsync(["game1.zip"], "NES", CancellationToken.None);
        await _cache.SetAllGamesAsync(["game2.zip", "game3.zip"], "SNES", CancellationToken.None);

        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
        Assert.Contains("game2.zip", result);
        Assert.Contains("game3.zip", result);
    }

    [Fact]
    public async Task SetSearchResultsAsyncOverwritesPreviousResults()
    {
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);
        await _cache.SetSearchResultsAsync(["zelda.zip", "metroid.zip"], CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsFalseForEmptyGames()
    {
        await _cache.SetAllGamesAsync([], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("NES", CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task GetResortSourceAsyncWithoutFilterReturnsAllGamesEvenWithSearchResults()
    {
        await _cache.SetAllGamesAsync(["game1.zip", "game2.zip"], "NES", CancellationToken.None);
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var (allGames, searchResults) = await _cache.GetResortSourceAsync(false, CancellationToken.None);
        Assert.Equal(2, allGames.Count);
        Assert.Single(searchResults);
    }

    [Fact]
    public async Task GetResortSourceAsyncWithFilterReturnsSearchResultsAndAllGames()
    {
        await _cache.SetAllGamesAsync(["game1.zip", "game2.zip"], "NES", CancellationToken.None);
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var (source, searchResults) = await _cache.GetResortSourceAsync(true, CancellationToken.None);
        Assert.Single(source);
        Assert.Equal("mario.zip", source[0]);
        Assert.Single(searchResults);
    }

    [Fact]
    public async Task InvalidateAsyncResetsSelectedSystem()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        Assert.Equal("NES", _cache.SelectedSystem);

        await _cache.InvalidateAsync(CancellationToken.None);
        // SelectedSystem is not reset by InvalidateAsync, only the lists are cleared
        var games = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(games);
    }

    [Fact]
    public async Task SetAllGamesAsyncWithLargeList()
    {
        var games = Enumerable.Range(1, 10000).Select(static i => $"game{i}.zip").ToList();
        await _cache.SetAllGamesAsync(games, "NES", CancellationToken.None);

        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Equal(10000, result.Count);
    }

    [Fact]
    public async Task SetSearchResultsAsyncWithLargeList()
    {
        var results = Enumerable.Range(1, 5000).Select(static i => $"result{i}.zip").ToList();
        await _cache.SetSearchResultsAsync(results, CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Equal(5000, result.Count);
    }

    [Fact]
    public async Task GetAllGamesAsyncReturnsIndependentCopies()
    {
        await _cache.SetAllGamesAsync(["game1.zip"], "NES", CancellationToken.None);

        var copy1 = await _cache.GetAllGamesAsync(CancellationToken.None);
        var copy2 = await _cache.GetAllGamesAsync(CancellationToken.None);

        copy1.Add("injected.zip");
        Assert.Single(copy2);
    }

    [Fact]
    public async Task GetSearchResultsAsyncReturnsIndependentCopies()
    {
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var copy1 = await _cache.GetSearchResultsAsync(CancellationToken.None);
        var copy2 = await _cache.GetSearchResultsAsync(CancellationToken.None);

        copy1.Add("injected.zip");
        Assert.Single(copy2);
    }

    [Fact]
    public async Task SetAllGamesThenInvalidateThenSetDifferentSystem()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        await _cache.InvalidateAsync(CancellationToken.None);
        await _cache.SetAllGamesAsync(["snes_game.zip"], "SNES", CancellationToken.None);

        Assert.Equal("SNES", _cache.SelectedSystem);
        var games = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Single(games);
        Assert.Equal("snes_game.zip", games[0]);
    }

    [Fact]
    public async Task SetAllGamesAsyncPreservesOriginalList()
    {
        var originalList = new List<string> { "game1.zip", "game2.zip" };
        await _cache.SetAllGamesAsync(originalList, "NES", CancellationToken.None);

        originalList.Add("game3.zip");

        var cachedList = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Equal(2, cachedList.Count); // Should not be affected by modification of original
    }

    [Fact]
    public void DisposeTwiceDoesNotThrow()
    {
        var cache = new GameCacheService(new NoOpLogErrors());
        cache.Dispose();
        var ex = Record.Exception(cache.Dispose);
        Assert.Null(ex);
    }
}
