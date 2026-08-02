namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents the result of a RetroAchievements hash calculation, including the hash and any temporary extraction path.
/// </summary>
public struct RaHashResult
{
    /// <summary>
    /// Gets the calculated hash of the game ROM.
    /// </summary>
    public string? Hash { get; }

    /// <summary>
    /// Gets the temporary path where the ROM was extracted for hashing, if applicable.
    /// </summary>
    public string? TempExtractionPath { get; }

    /// <summary>
    /// Gets whether the extraction required for hashing was successful.
    /// </summary>
    public bool IsExtractionSuccessful { get; }

    /// <summary>
    /// Gets an error message if the extraction failed, otherwise null.
    /// </summary>
    public string? ExtractionErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RaHashResult"/> struct.
    /// </summary>
    /// <param name="hash">The calculated hash of the game ROM.</param>
    /// <param name="tempExtractionPath">The temporary path where the ROM was extracted, if applicable.</param>
    /// <param name="isExtractionSuccessful">Whether the extraction was successful.</param>
    /// <param name="extractionErrorMessage">An error message if the extraction failed.</param>
    public RaHashResult(string? hash, string? tempExtractionPath, bool isExtractionSuccessful = true, string? extractionErrorMessage = null)
    {
        Hash = hash;
        TempExtractionPath = tempExtractionPath;
        IsExtractionSuccessful = isExtractionSuccessful;
        ExtractionErrorMessage = extractionErrorMessage;
    }
}
