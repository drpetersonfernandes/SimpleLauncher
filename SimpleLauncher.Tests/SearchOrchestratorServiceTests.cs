using SimpleLauncher.Core.Interfaces;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchOrchestratorServiceTests"/> class,
    /// creating the game cache service and search orchestrator service instances.
    /// </summary>
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
        var result = await _service.ValidateAndPrepareAsync(null!, "NES", CancellationToken.None);
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
        await _service.ValidateAndPrepareAsync(null!, "NES", CancellationToken.None);
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
        /// <summary>
        /// Gets or sets a value indicating whether the orchestrator replaced the cached search results.
        /// </summary>
        public bool SearchResultsCleared { get; set; }

        /// <summary>
        /// Gets or sets the currently selected system name.
        /// </summary>
        public string SelectedSystem { get; set; } = "";

        /// <summary>
        /// Returns an empty list of cached games.
        /// </summary>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A task producing an empty game list.</returns>
        public Task<IList<string>> GetAllGamesAsync(CancellationToken ct)
        {
            return Task.FromResult<IList<string>>([]);
        }

        /// <summary>
        /// Returns an empty list of cached search results.
        /// </summary>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A task producing an empty search result list.</returns>
        public Task<IList<string>> GetSearchResultsAsync(CancellationToken ct)
        {
            return Task.FromResult<IList<string>>([]);
        }

        /// <summary>
        /// Always reports that the cache is not populated for the requested system.
        /// </summary>
        /// <param name="systemName">The system name to check.</param>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A task producing <c>false</c>.</returns>
        public Task<bool> IsCachePopulatedForSystemAsync(string systemName, CancellationToken ct)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Ignores the supplied game list and completes immediately.
        /// </summary>
        /// <param name="games">The games that would be cached.</param>
        /// <param name="systemName">The system the games belong to.</param>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task SetAllGamesAsync(IList<string> games, string systemName, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Flags that the cached search results were replaced and completes immediately.
        /// </summary>
        /// <param name="results">The search results that would be cached.</param>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task SetSearchResultsAsync(IList<string> results, CancellationToken ct)
        {
            SearchResultsCleared = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns empty game and search result lists to use as a re-sort source.
        /// </summary>
        /// <param name="hasActiveFilter">Whether a filter is currently applied.</param>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A task producing empty game and search result lists.</returns>
        public Task<(List<string> allGames, List<string> searchResults)> GetResortSourceAsync(bool hasActiveFilter, CancellationToken ct)
        {
            return Task.FromResult((new List<string>(), new List<string>()));
        }

        /// <summary>
        /// Ignores the request to populate the cache from disk and completes immediately.
        /// </summary>
        /// <param name="config">The system configuration that would be scanned.</param>
        /// <param name="fileService">The service that would enumerate the files on disk.</param>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task PopulateFromDiskAsync(Services.SystemManager.SystemManagerService config, IGetListOfFilesService fileService, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ignores the cache invalidation request and completes immediately.
        /// </summary>
        /// <param name="ct">A token to observe for cancellation.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task InvalidateAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Does nothing; the test cache holds no state that needs clearing synchronously.
        /// </summary>
        public void ClearSync()
        {
        }
    }
}
