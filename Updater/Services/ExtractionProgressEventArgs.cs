namespace Updater.Services;

/// <summary>
/// Event arguments for extraction progress updates.
/// </summary>
public class ExtractionProgressEventArgs : EventArgs
{
    public string? CurrentFile { get; set; }
    public int ExtractedCount { get; set; }
    public string StatusText { get; set; } = "";
}
