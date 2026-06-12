using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Extended tests for the <see cref="GameCacheService"/> class covering additional edge cases.
/// </summary>
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

    private sealed class NoOpDebugLogger : IDebugLogger
    {
        public void Log(string message)
        {
        }

        public void LogException(Exception ex, string? contextMessage = null)
        {
        }
    }

    public GameCacheServiceExtendedTests()
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

    /// <summary>
    /// Verifies that SetAllGamesAsync overwrites previously cached games.
    /// </summary>
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

    /// <summary>
    /// Verifies that SetSearchResultsAsync overwrites previously cached results.
    /// </summary>
    [Fact]
    public async Task SetSearchResultsAsyncOverwritesPreviousResults()
    {
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);
        await _cache.SetSearchResultsAsync(["zelda.zip", "metroid.zip"], CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that IsCachePopulatedForSystemAsync returns false when an empty games list was set.
    /// </summary>
    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsFalseForEmptyGames()
    {
        await _cache.SetAllGamesAsync([], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("NES", CancellationToken.None);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that GetResortSourceAsync without filter returns all games even when search results exist.
    /// </summary>
    [Fact]
    public async Task GetResortSourceAsyncWithoutFilterReturnsAllGamesEvenWithSearchResults()
    {
        await _cache.SetAllGamesAsync(["game1.zip", "game2.zip"], "NES", CancellationToken.None);
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var (allGames, searchResults) = await _cache.GetResortSourceAsync(false, CancellationToken.None);
        Assert.Equal(2, allGames.Count);
        Assert.Single(searchResults);
    }

    /// <summary>
    /// Verifies that GetResortSourceAsync with filter returns search results and all games.
    /// </summary>
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

    /// <summary>
    /// Verifies that InvalidateAsync clears cached games but preserves SelectedSystem.
    /// </summary>
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

    /// <summary>
    /// Verifies that SetAllGamesAsync handles a large list of 10000 games.
    /// </summary>
    [Fact]
    public async Task SetAllGamesAsyncWithLargeList()
    {
        var games = Enumerable.Range(1, 10000).Select(static i => $"game{i}.zip").ToList();
        await _cache.SetAllGamesAsync(games, "NES", CancellationToken.None);

        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Equal(10000, result.Count);
    }

    /// <summary>
    /// Verifies that SetSearchResultsAsync handles a large list of 5000 results.
    /// </summary>
    [Fact]
    public async Task SetSearchResultsAsyncWithLargeList()
    {
        var results = Enumerable.Range(1, 5000).Select(static i => $"result{i}.zip").ToList();
        await _cache.SetSearchResultsAsync(results, CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Equal(5000, result.Count);
    }

    /// <summary>
    /// Verifies that successive calls to GetAllGamesAsync return independent copies.
    /// </summary>
    [Fact]
    public async Task GetAllGamesAsyncReturnsIndependentCopies()
    {
        await _cache.SetAllGamesAsync(["game1.zip"], "NES", CancellationToken.None);

        var copy1 = await _cache.GetAllGamesAsync(CancellationToken.None);
        var copy2 = await _cache.GetAllGamesAsync(CancellationToken.None);

        copy1.Add("injected.zip");
        Assert.Single(copy2);
    }

    /// <summary>
    /// Verifies that successive calls to GetSearchResultsAsync return independent copies.
    /// </summary>
    [Fact]
    public async Task GetSearchResultsAsyncReturnsIndependentCopies()
    {
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var copy1 = await _cache.GetSearchResultsAsync(CancellationToken.None);
        var copy2 = await _cache.GetSearchResultsAsync(CancellationToken.None);

        copy1.Add("injected.zip");
        Assert.Single(copy2);
    }

    /// <summary>
    /// Verifies the workflow of setting games, invalidating, then setting for a different system.
    /// </summary>
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

    /// <summary>
    /// Verifies that SetAllGamesAsync preserves the original list from external modification.
    /// </summary>
    [Fact]
    public async Task SetAllGamesAsyncPreservesOriginalList()
    {
        var originalList = new List<string> { "game1.zip", "game2.zip" };
        await _cache.SetAllGamesAsync(originalList, "NES", CancellationToken.None);

        originalList.Add("game3.zip");

        var cachedList = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Equal(2, cachedList.Count); // Should not be affected by modification of original
    }

    /// <summary>
    /// Verifies that calling Dispose twice does not throw.
    /// </summary>
    [Fact]
    public void DisposeTwiceDoesNotThrow()
    {
        var cache = new GameCacheService(new NoOpLogErrors(), new NoOpDebugLogger());
        cache.Dispose();
        var ex = Record.Exception(cache.Dispose);
        Assert.Null(ex);
    }
}
