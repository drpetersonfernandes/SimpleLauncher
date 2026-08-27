namespace SimpleLauncher.Updater.Services;

/// <summary>
/// Event arguments for extraction progress updates.
/// </summary>
internal class ExtractionProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the name of the file currently being extracted.
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    /// Gets or sets the number of files extracted so far.
    /// </summary>
    public int ExtractedCount { get; set; }

    /// <summary>
    /// Gets or sets the human-readable status text describing the extraction progress.
    /// </summary>
    public string StatusText { get; set; } = "";
}