using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.DownloadService;

/// <inheritdoc />
/// <summary>
/// Manages the download and extraction of files with progress reporting and cancellation support.
/// </summary>
public class DownloadManager : IDisposable
{
    // Events
    /// <summary>
    /// Event raised when download progress changes.
    /// </summary>
    public event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;

    // Volatile backing fields for thread-safe state access across async/thread boundaries
    private volatile bool _isDownloadCompleted;
    private volatile bool _isUserCancellation;
    private volatile bool _isFileLockedDuringDownload;

    // Constants
    private const int RetryMaxAttempts = 3;
    private const int RetryBaseDelayMs = 1000;

    // Private fields
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExtractionService _extractionService;
    private readonly ILogErrors _logErrors;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the DownloadManager.
    /// </summary>
    public DownloadManager(IHttpClientFactory httpClientFactory, IExtractionService extractionService, ILogErrors logErrors)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));

        // Initialize temp folder
        TempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

        try
        {
            Directory.CreateDirectory(TempFolder);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, $"Error creating temp folder: {TempFolder}");
        }

        // Get HttpClient from the factory
        _httpClient = _httpClientFactory?.CreateClient();

        // Initialize cancellation token source
        _cancellationTokenSource = new CancellationTokenSource();
    }

    // Properties
    /// <summary>
    /// Gets a value indicating whether the download was completed successfully.
    /// </summary>
    internal bool IsDownloadCompleted
    {
        get => _isDownloadCompleted;
        private set => _isDownloadCompleted = value;
    }

    /// <summary>
    /// Gets a value indicating whether the download was canceled by the user.
    /// </summary>
    internal bool IsUserCancellation
    {
        get => _isUserCancellation;
        private set => _isUserCancellation = value;
    }

    /// <summary>
    /// Gets the temporary folder used for downloads.
    /// </summary>
    internal string TempFolder { get; }

    // Methods
    /// <summary>
    /// Cancels any ongoing download operation.
    /// </summary>
    internal void CancelDownload()
    {
        CancellationTokenSource cts;
        lock (_lock)
        {
            if (_disposed)
                return;

            IsUserCancellation = true;
            cts = _cancellationTokenSource;
        }

        // Cancel outside the lock to prevent blocking and handle disposal races
        try
        {
            cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Ignore - the CTS was already disposed
        }
    }

    // Existing property moved to use backing field for write access
    internal bool IsFileLockedDuringDownload
    {
        get => _isFileLockedDuringDownload;
        private set => _isFileLockedDuringDownload = value;
    }

    private void ResetCancellationToken()
    {
        CancellationTokenSource oldCts;
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(DownloadManager));

            oldCts = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // Dispose outside the lock to prevent deadlock and ObjectDisposedException races
        try
        {
            oldCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        oldCts?.Dispose();
    }

    /// <summary>
    /// Downloads a file from the specified URL to a temporary location.
    /// </summary>
    /// <param name="downloadUrl">The URL to download from.</param>
    /// <param name="fileName">Optional custom file name to use.</param>
    /// <returns>The path to the downloaded file, or null if the download failed.</returns>
    internal async Task<string> DownloadFileAsync(string downloadUrl, string fileName = null)
    {
        // Reset the cancellation token source at the beginning of every download attempt.
        ResetCancellationToken();

        IsDownloadCompleted = false;
        IsUserCancellation = false;
        IsFileLockedDuringDownload = false; // Reset at the start of each download attempt

        // Determine file name if not provided
        if (string.IsNullOrEmpty(fileName))
        {
            try
            {
                fileName = Path.GetFileName(downloadUrl);
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = "download_" + Guid.NewGuid().ToString("N");
                }
            }
            catch
            {
                fileName = "download_" + Guid.NewGuid().ToString("N");
            }
        }

        // Create temp file path
        var downloadFilePath = Path.Combine(TempFolder, fileName);

        // Check disk space
        var diskSpaceCheckResult = CheckAvailableDiskSpace(TempFolder);
        switch (diskSpaceCheckResult)
        {
            case false:
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("InsufficientdiskspaceinSimpleLauncherHDD", "Insufficient disk space.")
                });
                throw new IOException("Insufficient disk space in 'Simple Launcher' HDD.");
            case null:
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("CannotCheckDiskSpace", "Cannot check available disk space. The path may be inaccessible or you may lack permissions.")
                });
                throw new IOException("Cannot check disk space for 'Simple Launcher' HDD. The path may be inaccessible or you may lack permissions.");
        }

        CancellationToken token;
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed || _cancellationTokenSource == null, nameof(DownloadManager));

            token = _cancellationTokenSource.Token;
        }

        var success = false;
        var currentRetry = 0;

        while (currentRetry < RetryMaxAttempts && !IsUserCancellation)
        {
            try
            {
                // We pass 'currentRetry > 0' to attempt resuming if a partial file exists from a previous attempt
                await DownloadWithProgressAsync(downloadUrl, downloadFilePath, currentRetry > 0, token);

                if (IsDownloadCompleted)
                {
                    success = true;
                    break;
                }

                currentRetry++;
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or TaskCanceledException)
            {
                if (IsUserCancellation)
                {
                    break;
                }

                // Check for file lock specifically
                if (ex.Message.Contains("being used by another process", StringComparison.OrdinalIgnoreCase))
                {
                    IsFileLockedDuringDownload = true;
                    throw; // Exit loop to show the specific "File Locked" message box
                }

                currentRetry++;
                if (currentRetry >= RetryMaxAttempts) throw;

                var delay = RetryBaseDelayMs * (int)Math.Pow(2, currentRetry - 1);
                var retryMsg = ex.Message.Contains("prematurely", StringComparison.OrdinalIgnoreCase)
                    ? "Connection dropped, attempting to resume..."
                    : $"Download error, retrying ({currentRetry}/{RetryMaxAttempts})...";

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = retryMsg
                });

                await Task.Delay(delay, token);
            }
        }

        if (success && IsDownloadCompleted)
        {
            return downloadFilePath;
        }

        // Cleanup on failure
        if (!IsDownloadCompleted && !IsFileLockedDuringDownload)
        {
            DeleteFiles.TryDeleteFile(downloadFilePath);
        }

        return null;
    }


    /// <summary>
    /// Extracts a compressed file to the specified destination.
    /// </summary>
    /// <param name="filePath">The path to the compressed file.</param>
    /// <param name="destinationPath">The destination path to extract to.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    internal async Task<bool> ExtractFileAsync(string filePath, string destinationPath)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("Extracting",
                        $"Extracting to {destinationPath}...")
                });
            });

            var result = await _extractionService.ExtractToFolderAsync(filePath, destinationPath);

            if (result)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 100,
                        StatusMessage = GetResourceString("ExtractionCompleted", "Extraction completed successfully.")
                    });
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("ExtractionFailed", "Extraction failed.")
                    });
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("ExtractionError", $"Extraction error: {ex.Message}")
                });
            });

            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, $"Error extracting file: {filePath} to {destinationPath}");

            return false;
        }
    }

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, bool tryResume, CancellationToken cancellationToken)
    {
        long existingLength = 0;
        if (tryResume && File.Exists(destinationPath))
        {
            existingLength = new FileInfo(destinationPath).Length;
        }

        // Create request with optional Range header
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        if (existingLength > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);
            DebugLogger.Log($"[DownloadManager] Attempting to resume {Path.GetFileName(downloadUrl)} from byte {existingLength}");
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // 416 means the file is likely already fully downloaded
        if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
        {
            IsDownloadCompleted = true;
            return;
        }

        response.EnsureSuccessStatusCode();

        // Check if server actually accepted the range (206 Partial Content)
        var isResuming = response.StatusCode == HttpStatusCode.PartialContent;
        var totalBytes = response.Content.Headers.ContentLength;

        if (isResuming)
        {
            totalBytes += existingLength;
        }
        else
        {
            existingLength = 0; // Server ignored range, starting from scratch
        }

        // Open stream: Append if resuming, Create if starting fresh
        await using var fileStream = new FileStream(destinationPath, isResuming ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 8192, true);
        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var buffer = new byte[8192];
        var totalBytesRead = existingLength;
        int bytesRead;
        var lastProgressUpdate = DateTime.Now;

        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (IsUserCancellation) throw new TaskCanceledException();

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytesRead += bytesRead;

            // Throttle UI updates to 10fps
            if ((DateTime.Now - lastProgressUpdate).TotalMilliseconds >= 100)
            {
                var progressPercentage = totalBytes.HasValue ? (double)totalBytesRead / totalBytes.Value * 100 : 0;
                var sizeStatus = totalBytes.HasValue
                    ? $"{FormatFileSize.FormatToHumanReadable(totalBytesRead)} of {FormatFileSize.FormatToHumanReadable(totalBytes.Value)}"
                    : $"{FormatFileSize.FormatToHumanReadable(totalBytesRead)}";

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = progressPercentage,
                    StatusMessage = $"{GetResourceString("Downloading", "Downloading")}: {sizeStatus} ({progressPercentage:F1}%)"
                });
                lastProgressUpdate = DateTime.Now;
            }
        }

        // Final validation
        if (!totalBytes.HasValue || totalBytesRead >= totalBytes.Value)
        {
            IsDownloadCompleted = true;
            OnProgressChanged(new DownloadProgressEventArgs
            {
                BytesReceived = totalBytesRead,
                TotalBytesToReceive = totalBytes,
                ProgressPercentage = 100,
                StatusMessage = $"{GetResourceString("Downloadcomplete2", "Download complete")}: {FormatFileSize.FormatToHumanReadable(totalBytesRead)}"
            });
        }
    }


    /// <summary>
    /// Checks if there is enough disk space available in the specified folder.
    /// </summary>
    /// <param name="folderPath">The folder path to check.</param>
    /// <param name="requiredSpace">The required space in bytes (default is 5GB).</param>
    /// <returns>True if enough space is available, false if insufficient, null if cannot be checked.</returns>
    private static bool? CheckAvailableDiskSpace(string folderPath, long requiredSpace = 5368709120)
    {
        try
        {
            folderPath = Path.GetFullPath(folderPath);
            var driveInfo = new DriveInfo(Path.GetPathRoot(folderPath) ?? throw new InvalidOperationException("Could not get the drive info"));
            return driveInfo.AvailableFreeSpace > requiredSpace;
        }
        catch
        {
            // If we can't check disk space (e.g., network drive issues, permissions),
            // return null to indicate inability to check rather than insufficient space.
            return null;
        }
    }

    /// <summary>
    /// Gets a localized string from resources.
    /// </summary>
    /// <param name="resourceKey">The resource key.</param>
    /// <param name="defaultValue">The default value if the resource is not found.</param>
    /// <returns>The localized string or the default value.</returns>
    private static string GetResourceString(string resourceKey, string defaultValue)
    {
        return (string)Application.Current.TryFindResource(resourceKey) ?? defaultValue;
    }

    /// <summary>
    /// Raises the DownloadProgressChanged event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnProgressChanged(DownloadProgressEventArgs e)
    {
        DownloadProgressChanged?.Invoke(this, e);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CancellationTokenSource cts;
        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
            cts = _cancellationTokenSource;
            _cancellationTokenSource = null;
        }

        // Cancel and dispose outside the lock to prevent deadlock
        try
        {
            cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Ignore
        }

        cts?.Dispose();

        GC.SuppressFinalize(this);
    }
}