using System;

namespace SimpleLauncher.Models;

/// <inheritdoc />
/// <summary>
/// Event args for download progress updates.
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the number of bytes received.
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// Gets or sets the total number of bytes to receive.
    /// </summary>
    public long? TotalBytesToReceive { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage (0-100).
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage { get; set; }
}