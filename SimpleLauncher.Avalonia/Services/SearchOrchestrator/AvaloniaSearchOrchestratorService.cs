namespace SimpleLauncher.Avalonia.Services.SearchOrchestrator;

/// <summary>
/// Validates search queries and coordinates search execution.
/// Extracted from the inline search logic in MainViewModel.
/// Mirrors the WPF SearchOrchestratorService.
/// </summary>
public class AvaloniaSearchOrchestratorService
{
    /// <summary>
    /// Validates a search query before execution.
    /// </summary>
    /// <param name="searchQuery">The raw search query from the UI.</param>
    /// <param name="selectedSystem">The currently selected system (null or empty = all systems).</param>
    /// <returns>A validation result indicating whether the search should proceed.</returns>
    public SearchValidationResult ValidateAndPrepare(string searchQuery, string? selectedSystem)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return SearchValidationResult.Failure();
        }

        return SearchValidationResult.Success(searchQuery.Trim());
    }
}

/// <summary>
/// Result of validating a search query.
/// </summary>
public class SearchValidationResult
{
    public bool IsValid { get; init; }
    public string ValidatedQuery { get; init; } = "";

    public static SearchValidationResult Success(string query)
    {
        return new SearchValidationResult { IsValid = true, ValidatedQuery = query };
    }

    public static SearchValidationResult Failure()
    {
        return new SearchValidationResult { IsValid = false };
    }
}
