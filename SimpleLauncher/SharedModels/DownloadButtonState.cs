namespace SimpleLauncher.SharedModels;

public enum DownloadButtonState
{
    Idle, // Ready to download, button enabled
    Downloading, // Download in progress, button disabled
    Downloaded, // Successfully downloaded, button disabled
    Failed // Download failed/cancelled, button enabled
}