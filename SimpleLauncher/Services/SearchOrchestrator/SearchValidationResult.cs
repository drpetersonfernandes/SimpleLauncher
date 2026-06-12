namespace SimpleLauncher.Services.SearchOrchestrator;

/// <summary>
/// Represents the result of a search validation, indicating validity and the sanitized query.
/// </summary>
public class SearchValidationResult
{
    /// <summary>
    /// Gets whether the search query passed validation.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the trimmed and validated search query.
    /// </summary>
    public string ValidatedQuery { get; init; }

    /// <summary>
    /// Creates a successful validation result with the sanitized query.
    /// </summary>
    public static SearchValidationResult Success(string query)
    {
        return new SearchValidationResult
        {
            IsValid = true,
            ValidatedQuery = query
        };
    }

    /// <summary>
    /// Creates a failed validation result indicating the query or system was invalid.
    /// </summary>
    public static SearchValidationResult Failure()
    {
        return new SearchValidationResult
        {
            IsValid = false
        };
    }
}
