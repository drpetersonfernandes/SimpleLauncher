namespace SimpleLauncher.Avalonia.Updater.Services;

/// <summary>
///     Provides progress information for download operations.
/// </summary>
internal class DownloadProgressInfo
{
    /// <summary>
    ///     The percentage of completion (0-100), or -1 if the total size is unknown.
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    ///     The number of bytes read so far.
    /// </summary>
    public long BytesRead { get; set; }

    /// <summary>
    ///     The total number of bytes to download, or 0 if unknown.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    ///     The download speed in bytes per second.
    /// </summary>
    public double BytesPerSecond { get; set; }
}