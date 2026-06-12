using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests for the <see cref="GameCacheService"/> class.
/// </summary>
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

    /// <summary>
    /// Verifies that GetAllGamesAsync returns an empty list when no games are cached.
    /// </summary>
    [Fact]
    public async Task GetAllGamesAsyncReturnsEmptyListInitially()
    {
        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetSearchResultsAsync returns an empty list when no results are cached.
    /// </summary>
    [Fact]
    public async Task GetSearchResultsAsyncReturnsEmptyListInitially()
    {
        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that SetAllGamesAsync stores and retrieves games correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that SetSearchResultsAsync stores and retrieves search results.
    /// </summary>
    [Fact]
    public async Task SetSearchResultsAsyncStoresResults()
    {
        var results = new List<string> { "mario.zip", "luigi.zip" };
        await _cache.SetSearchResultsAsync(results, CancellationToken.None);

        var result = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that IsCachePopulatedForSystemAsync returns false when cache is empty.
    /// </summary>
    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsFalseWhenEmpty()
    {
        var result = await _cache.IsCachePopulatedForSystemAsync("NES", CancellationToken.None);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that IsCachePopulatedForSystemAsync returns true for the matching system.
    /// </summary>
    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsTrueForMatchingSystem()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("NES", CancellationToken.None);
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that IsCachePopulatedForSystemAsync returns false for a different system.
    /// </summary>
    [Fact]
    public async Task IsCachePopulatedForSystemAsyncReturnsFalseForDifferentSystem()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("SNES", CancellationToken.None);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that IsCachePopulatedForSystemAsync performs case-insensitive system name comparison.
    /// </summary>
    [Fact]
    public async Task IsCachePopulatedForSystemAsyncCaseInsensitive()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        var result = await _cache.IsCachePopulatedForSystemAsync("nes", CancellationToken.None);
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that InvalidateAsync clears the cached games.
    /// </summary>
    [Fact]
    public async Task InvalidateAsyncClearsCache()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        await _cache.InvalidateAsync(CancellationToken.None);

        var games = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(games);
    }

    /// <summary>
    /// Verifies that InvalidateAsync clears the cached search results.
    /// </summary>
    [Fact]
    public async Task InvalidateAsyncClearsSearchResults()
    {
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);
        await _cache.InvalidateAsync(CancellationToken.None);

        var results = await _cache.GetSearchResultsAsync(CancellationToken.None);
        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that ClearSync does not throw when called.
    /// </summary>
    [Fact]
    public void ClearSyncDoesNotThrow()
    {
        var exception = Record.Exception(() => _cache.ClearSync());
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that ClearSync clears all cached data.
    /// </summary>
    [Fact]
    public async Task ClearSyncClearsData()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "NES", CancellationToken.None);
        _cache.ClearSync();

        var games = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(games);
    }

    /// <summary>
    /// Verifies that GetAllGamesAsync returns a defensive copy of the cached games.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetSearchResultsAsync returns a defensive copy of the cached results.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetResortSourceAsync without filter returns all games.
    /// </summary>
    [Fact]
    public async Task GetResortSourceAsyncWithoutFilterReturnsAllGames()
    {
        var games = new List<string> { "game1.zip", "game2.nes" };
        await _cache.SetAllGamesAsync(games, "NES", CancellationToken.None);

        var (allGames, _) = await _cache.GetResortSourceAsync(false, CancellationToken.None);
        Assert.Equal(2, allGames.Count);
    }

    /// <summary>
    /// Verifies that GetResortSourceAsync with filter returns search results.
    /// </summary>
    [Fact]
    public async Task GetResortSourceAsyncWithFilterReturnsSearchResults()
    {
        await _cache.SetAllGamesAsync(["game1.zip", "game2.nes"], "NES", CancellationToken.None);
        await _cache.SetSearchResultsAsync(["mario.zip"], CancellationToken.None);

        var (source, _) = await _cache.GetResortSourceAsync(true, CancellationToken.None);
        Assert.Single(source);
        Assert.Equal("mario.zip", source[0]);
    }

    /// <summary>
    /// Verifies that SelectedSystem defaults to an empty string.
    /// </summary>
    [Fact]
    public void SelectedSystemDefaultsToEmpty()
    {
        Assert.Equal("", _cache.SelectedSystem);
    }

    /// <summary>
    /// Verifies that SelectedSystem is updated when SetAllGamesAsync is called.
    /// </summary>
    [Fact]
    public async Task SelectedSystemUpdatesOnSetAllGames()
    {
        await _cache.SetAllGamesAsync(["game.zip"], "SNES", CancellationToken.None);
        Assert.Equal("SNES", _cache.SelectedSystem);
    }

    /// <summary>
    /// Verifies that Dispose does not throw.
    /// </summary>
    [Fact]
    public void DisposeDoesNotThrow()
    {
        var cache = new GameCacheService(new NoOpLogErrors(), new NoOpDebugLogger());
        var exception = Record.Exception(cache.Dispose);
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that SetAllGamesAsync with an empty list results in empty cache.
    /// </summary>
    [Fact]
    public async Task SetAllGamesWithEmptyList()
    {
        await _cache.SetAllGamesAsync([], "NES", CancellationToken.None);
        var result = await _cache.GetAllGamesAsync(CancellationToken.None);
        Assert.Empty(result);
    }
}
