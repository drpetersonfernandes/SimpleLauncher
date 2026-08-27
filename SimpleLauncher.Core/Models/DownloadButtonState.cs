namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents the current state of a download button in the UI.
/// </summary>
public enum DownloadButtonState
{
    /// <summary>
    /// Ready to download. Button is enabled.
    /// </summary>
    Idle,

    /// <summary>
    /// Download is in progress. Button is disabled.
    /// </summary>
    Downloading,

    /// <summary>
    /// Download completed successfully. Button is disabled.
    /// </summary>
    Downloaded,

    /// <summary>
    /// Download failed or was cancelled. Button is enabled.
    /// </summary>
    Failed
}