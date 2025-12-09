namespace SimpleLauncher.Models.RetroAchievements;

/// <summary>
/// Represents the result of a RetroAchievements hash calculation, including the hash and any temporary extraction path.
/// </summary>
public struct RaHashResult
{
    public string Hash { get; }
    public string TempExtractionPath { get; }
    public bool IsExtractionSuccessful { get; }
    public string ExtractionErrorMessage { get; }

    public RaHashResult(string hash, string tempExtractionPath, bool isExtractionSuccessful = true, string extractionErrorMessage = null)
    {
        Hash = hash;
        TempExtractionPath = tempExtractionPath;
        IsExtractionSuccessful = isExtractionSuccessful;
        ExtractionErrorMessage = extractionErrorMessage;
    }
}