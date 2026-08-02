using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Orchestrates the validation and preparation of game search queries.
/// </summary>
public interface ISearchOrchestratorService
{
    /// <summary>
    /// Validates the search query and selected system, then clears previous search results from the cache.
    /// </summary>
    /// <param name="searchQuery">The search query entered by the user.</param>
    /// <param name="selectedSystem">The name of the currently selected system, if any.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="SearchValidationResult"/> describing the validation outcome.</returns>
    Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string? selectedSystem, CancellationToken cancellationToken);
}
