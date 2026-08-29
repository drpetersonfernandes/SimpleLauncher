namespace SimpleLauncher.Avalonia.Updater.Services;

/// <summary>
///     Provides progress information for ZIP extraction operations.
/// </summary>
internal class ExtractionProgressInfo
{
    /// <summary>
    ///     The current file being extracted.
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    ///     The number of files extracted so far.
    /// </summary>
    public int ExtractedCount { get; set; }
}