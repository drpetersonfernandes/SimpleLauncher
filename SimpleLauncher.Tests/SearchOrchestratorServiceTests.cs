using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.SearchOrchestrator;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the SearchOrchestratorService for validating and preparing search queries.
/// </summary>
public class SearchOrchestratorServiceTests
{
    private readonly SearchOrchestratorService _service;
    private readonly GameCacheServiceForTest _gameCacheService;

    public SearchOrchestratorServiceTests()
    {
        _gameCacheService = new GameCacheServiceForTest();
        _service = new SearchOrchestratorService(_gameCacheService);
    }

    /// <summary>
    /// Verifies that validation fails when the system name is null.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncReturnsFailureForNullSystem()
    {
        var result = await _service.ValidateAndPrepareAsync("mario", null, CancellationToken.None);
        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Verifies that validation fails when the system name is empty.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncReturnsFailureForEmptySystem()
    {
        var result = await _service.ValidateAndPrepareAsync("mario", "", CancellationToken.None);
        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Verifies that validation fails when the search query is null.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncReturnsFailureForNullQuery()
    {
        var result = await _service.ValidateAndPrepareAsync(null, "NES", CancellationToken.None);
        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Verifies that validation fails when the search query is empty.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncReturnsFailureForEmptyQuery()
    {
        var result = await _service.ValidateAndPrepareAsync("", "NES", CancellationToken.None);
        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Verifies that validation fails when the search query is whitespace only.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncReturnsFailureForWhitespaceQuery()
    {
        var result = await _service.ValidateAndPrepareAsync("   ", "NES", CancellationToken.None);
        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Verifies that validation succeeds and returns the query for valid input.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncReturnsSuccessForValidInput()
    {
        var result = await _service.ValidateAndPrepareAsync("mario", "NES", CancellationToken.None);
        Assert.True(result.IsValid);
        Assert.Equal("mario", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that leading and trailing whitespace is trimmed from the search query.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncTrimsQuery()
    {
        var result = await _service.ValidateAndPrepareAsync("  mario  ", "NES", CancellationToken.None);
        Assert.Equal("mario", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that successful validation clears previous search results in the cache.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncClearsSearchResults()
    {
        await _service.ValidateAndPrepareAsync("mario", "NES", CancellationToken.None);
        Assert.True(_gameCacheService.SearchResultsCleared);
    }

    /// <summary>
    /// Verifies that failed validation does not clear search results in the cache.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncFailureDoesNotClearSearchResults()
    {
        _gameCacheService.SearchResultsCleared = false;
        await _service.ValidateAndPrepareAsync(null, "NES", CancellationToken.None);
        Assert.False(_gameCacheService.SearchResultsCleared);
    }

    /// <summary>
    /// Verifies that search queries with special characters like parentheses are handled correctly.
    /// </summary>
    [Fact]
    public async Task ValidateAndPrepareAsyncWithSpecialCharacters()
    {
        var result = await _service.ValidateAndPrepareAsync("mega man x2 (usa)", "SNES", CancellationToken.None);
        Assert.True(result.IsValid);
        Assert.Equal("mega man x2 (usa)", result.ValidatedQuery);
    }

    private class GameCacheServiceForTest : IGameCacheService
    {
        public bool SearchResultsCleared { get; set; }
        public string SelectedSystem { get; set; } = "";

        public Task<List<string>> GetAllGamesAsync(CancellationToken ct)
        {
            return Task.FromResult(new List<string>());
        }

        public Task<List<string>> GetSearchResultsAsync(CancellationToken ct)
        {
            return Task.FromResult(new List<string>());
        }

        public Task<bool> IsCachePopulatedForSystemAsync(string systemName, CancellationToken ct)
        {
            return Task.FromResult(false);
        }

        public Task SetAllGamesAsync(List<string> games, string systemName, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task SetSearchResultsAsync(List<string> results, CancellationToken ct)
        {
            SearchResultsCleared = true;
            return Task.CompletedTask;
        }

        public Task<(List<string> allGames, List<string> searchResults)> GetResortSourceAsync(bool hasActiveFilter, CancellationToken ct)
        {
            return Task.FromResult((new List<string>(), new List<string>()));
        }

        public Task PopulateFromDiskAsync(Services.SystemManager.SystemManager config, IGetListOfFilesService fileService, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public void ClearSync()
        {
        }
    }
}
