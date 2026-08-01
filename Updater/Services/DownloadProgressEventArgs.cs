namespace Updater.Services;

/// <summary>
/// Event arguments for download progress updates.
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    public double Percentage { get; set; }
    public long BytesRead { get; set; }
    public long TotalBytes { get; set; }
    public string StatusText { get; set; } = "";
}
