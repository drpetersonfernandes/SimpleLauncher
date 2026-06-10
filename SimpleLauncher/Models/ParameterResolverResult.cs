namespace SimpleLauncher.Models;

/// <summary>
/// Contains the result of resolving emulator launch parameters.
/// </summary>
public class ParameterResolverResult
{
    /// <summary>
    /// Gets or sets the suggested CLI parameter string for the emulator.
    /// </summary>
    public string SuggestedParameter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable explanation of why this parameter was chosen.
    /// </summary>
    public string Explanation { get; set; }
}