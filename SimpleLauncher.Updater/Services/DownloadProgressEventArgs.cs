namespace SimpleLauncher.Updater.Services;

/// <summary>
/// Event arguments for download progress updates.
/// </summary>
internal class DownloadProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the download completion percentage (0-100), or -1 if the total size is unknown.
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes downloaded so far.
    /// </summary>
    public long BytesRead { get; set; }

    /// <summary>
    /// Gets or sets the total number of bytes to download, or 0 if unknown.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Gets or sets the human-readable status text describing the download progress.
    /// </summary>
    public string StatusText { get; set; } = "";
}
