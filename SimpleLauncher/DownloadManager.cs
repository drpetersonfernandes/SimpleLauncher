using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Services;

namespace SimpleLauncher;

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

    // Constants
    private const int HttpTimeoutSeconds = 60;
    private const int RetryMaxAttempts = 3;
    private const int RetryBaseDelayMs = 1000;

    // Private fields
    private readonly HttpClient _httpClient;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the DownloadManager.
    /// </summary>
    public DownloadManager()
    {
        // Initialize temp folder
        TempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        IoOperations.CreateDirectory(TempFolder);

        // Initialize the HTTP client with custom handler for TLS configuration
        var handler = new HttpClientHandler();

        // Configure TLS 1.2 and 1.3 if available
        handler.SslProtocols = SslProtocols.None;

        // Initialize HTTP client
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
        };

        // Initialize cancellation token source
        _cancellationTokenSource = new CancellationTokenSource();
    }

    // Properties
    /// <summary>
    /// Gets a value indicating whether the download was completed successfully.
    /// </summary>
    public bool IsDownloadCompleted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the download was canceled by the user.
    /// </summary>
    public bool IsUserCancellation { get; private set; }

    /// <summary>
    /// Gets the temporary folder used for downloads.
    /// </summary>
    private string TempFolder { get; }

    // Methods
    /// <summary>
    /// Cancels any ongoing download operation.
    /// </summary>
    public void CancelDownload()
    {
        IsUserCancellation = true;
        _cancellationTokenSource?.Cancel();
        ResetCancellationToken();
    }

    private void ResetCancellationToken()
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Downloads a file from the specified URL to a temporary location.
    /// </summary>
    /// <param name="downloadUrl">The URL to download from.</param>
    /// <param name="fileName">Optional custom file name to use.</param>
    /// <returns>The path to the downloaded file, or null if the download failed.</returns>
    public async Task<string> DownloadFileAsync(string downloadUrl, string fileName = null)
    {
        IsDownloadCompleted = false;
        IsUserCancellation = false;

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
        if (!CheckAvailableDiskSpace(TempFolder))
        {
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = GetResourceString("InsufficientDiskSpace", "Insufficient disk space.")
            });

            throw new IOException("Insufficient disk space in 'Simple Launcher' HDD or disk space *cannot* be checked.");
        }

        try
        {
            // Perform download with retry logic
            var currentRetry = 0;

            while (currentRetry < RetryMaxAttempts && !IsUserCancellation)
            {
                try
                {
                    await DownloadWithProgressAsync(downloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                    if (IsDownloadCompleted)
                        break;

                    currentRetry++;
                    if (currentRetry >= RetryMaxAttempts || IsUserCancellation) continue;

                    // Calculate delay with exponential backoff
                    var delay = RetryBaseDelayMs * (int)Math.Pow(2, currentRetry - 1);

                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("RetryingDownload",
                            $"Download incomplete, retrying ({currentRetry}/{RetryMaxAttempts})...")
                    });

                    await Task.Delay(delay, _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    if (IsUserCancellation)
                        break;

                    currentRetry++;
                    if (currentRetry < RetryMaxAttempts)
                    {
                        // Calculate delay with exponential backoff
                        var delay = RetryBaseDelayMs * (int)Math.Pow(2, currentRetry - 1);

                        OnProgressChanged(new DownloadProgressEventArgs
                        {
                            ProgressPercentage = 0,
                            StatusMessage = GetResourceString("RetryingDownloadTimeout",
                                $"Connection timeout, retrying ({currentRetry}/{RetryMaxAttempts})...")
                        });

                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Log error
                    await LogErrors.LogErrorAsync(ex, $"HTTP error during download attempt {currentRetry + 1}: {ex.Message}");

                    currentRetry++;
                    if (currentRetry < RetryMaxAttempts && !IsUserCancellation)
                    {
                        // Calculate delay with exponential backoff
                        var delay = RetryBaseDelayMs * (int)Math.Pow(2, currentRetry - 1);

                        OnProgressChanged(new DownloadProgressEventArgs
                        {
                            ProgressPercentage = 0,
                            StatusMessage = GetResourceString("RetryingDownloadError",
                                $"Connection error, retrying ({currentRetry}/{RetryMaxAttempts})...")
                        });

                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (IsDownloadCompleted)
            {
                return downloadFilePath;
            }
            else
            {
                if (IsUserCancellation)
                {
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("Downloadcanceledbyuser", "Download canceled by user.")
                    });
                }
                else
                {
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("DownloadFailedAfterRetries",
                            $"Download failed after {RetryMaxAttempts} attempts.")
                    });
                }

                // Clean up failed download
                DeleteFiles.TryDeleteFile(downloadFilePath);
                return null;
            }
        }
        catch (Exception ex)
        {
            // Clean up failed download
            DeleteFiles.TryDeleteFile(downloadFilePath);

            // Reset flags
            IsDownloadCompleted = false;

            switch (ex)
            {
                // Handle specific exceptions
                case HttpRequestException { StatusCode: HttpStatusCode.NotFound }:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("ErrorFilenotfoundontheserver", "Error: File not found on the server.")
                    });
                    break;
                case HttpRequestException { InnerException: AuthenticationException }:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("ErrorSSLConnection", "SSL/TLS connection issue.")
                    });
                    break;
                case HttpRequestException httpEx:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("Networkerror", $"Network error: {httpEx.Message}")
                    });
                    break;
                case IOException:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("Fileerror", $"File error: {ex.Message}")
                    });
                    break;
                case TaskCanceledException when IsUserCancellation:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("Downloadcanceledbyuser", "Download canceled by user.")
                    });
                    break;
                case TaskCanceledException:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("ErrorDownloadtimedout", "Download timed out.")
                    });
                    break;
                default:
                    OnProgressChanged(new DownloadProgressEventArgs
                    {
                        ProgressPercentage = 0,
                        StatusMessage = GetResourceString("Error", $"Error: {ex.Message}")
                    });
                    break;
            }

            // Log error
            await LogErrors.LogErrorAsync(ex, $"Error downloading file: {downloadUrl}");

            throw;
        }
    }

    /// <summary>
    /// Extracts a compressed file to the specified destination.
    /// </summary>
    /// <param name="filePath">The path to the compressed file.</param>
    /// <param name="destinationPath">The destination path to extract to.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    public async Task<bool> ExtractFileAsync(string filePath, string destinationPath)
    {
        try
        {
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = GetResourceString("Extracting",
                    $"Extracting to {destinationPath}...")
            });

            var result = await ExtractCompressedFile.ExtractDownloadFilesAsync(filePath, destinationPath);

            if (result)
            {
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 100,
                    StatusMessage = GetResourceString("ExtractionCompleted",
                        "Extraction completed successfully.")
                });
            }
            else
            {
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("ExtractionFailed",
                        "Extraction failed.")
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = GetResourceString("ExtractionError",
                    $"Extraction error: {ex.Message}")
            });

            // Log error
            await LogErrors.LogErrorAsync(ex, $"Error extracting file: {filePath} to {destinationPath}");

            return false;
        }
    }

    /// <summary>
    /// Downloads a file and extracts it to the specified destination in a single operation.
    /// </summary>
    /// <param name="downloadUrl">The URL to download from.</param>
    /// <param name="extractionPath">The destination path to extract to.</param>
    /// <param name="fileName">Optional custom file name to use.</param>
    /// <returns>True if the download and extraction were successful, otherwise false.</returns>
    public async Task<bool> DownloadAndExtractAsync(string downloadUrl, string extractionPath, string fileName = null)
    {
        try
        {
            // Reset flags
            IsDownloadCompleted = false;
            IsUserCancellation = false;

            // Download file
            var downloadedFilePath = await DownloadFileAsync(downloadUrl, fileName);

            if (string.IsNullOrEmpty(downloadedFilePath) || !IsDownloadCompleted)
            {
                return false;
            }

            try
            {
                // Extract the file
                var extractionResult = await ExtractFileAsync(downloadedFilePath, extractionPath);

                // Clean up downloaded file
                DeleteFiles.TryDeleteFile(downloadedFilePath);

                return extractionResult;
            }
            catch (Exception ex)
            {
                // Log error
                await LogErrors.LogErrorAsync(ex, $"Error during extraction: {downloadedFilePath} to {extractionPath}");

                // Clean up downloaded file
                DeleteFiles.TryDeleteFile(downloadedFilePath);

                return false;
            }
        }
        catch (Exception ex)
        {
            // Log error
            await LogErrors.LogErrorAsync(ex, $"Error during download and extract: {downloadUrl} to {extractionPath}");

            return false;
        }
    }

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            var totalSizeFormatted = totalBytes.HasValue
                ? FormatFileSize(totalBytes.Value)
                : GetResourceString("unknownsize", "unknown size");

            // Report initial progress
            OnProgressChanged(new DownloadProgressEventArgs
            {
                BytesReceived = 0,
                TotalBytesToReceive = totalBytes,
                ProgressPercentage = 0,
                StatusMessage = $"{GetResourceString("Startingdownload2", "Starting download")}: {Path.GetFileName(downloadUrl)} ({totalSizeFormatted})"
            });

            // Open file stream for writing
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 8192, true);

            // Set up buffer and tracking variables
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;
            var lastProgressUpdate = DateTime.Now;

            // Read and write the data in chunks
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                // Limit progress updates to reduce UI thread congestion (update every ~100ms)
                var now = DateTime.Now;
                if (!((now - lastProgressUpdate).TotalMilliseconds >= 100)) continue;

                var progressPercentage = totalBytes.HasValue
                    ? (double)totalBytesRead / totalBytes.Value * 100
                    : 0;

                var sizeStatus = totalBytes.HasValue
                    ? $"{FormatFileSize(totalBytesRead)} of {FormatFileSize(totalBytes.Value)}"
                    : $"{FormatFileSize(totalBytesRead)} of {totalSizeFormatted}";

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = progressPercentage,
                    StatusMessage = $"{GetResourceString("Downloading", "Downloading")}: {sizeStatus} ({progressPercentage:F1}%)"
                });

                lastProgressUpdate = now;
            }

            // Check if the file was fully downloaded
            if (totalBytesRead == totalBytes)
            {
                IsDownloadCompleted = true;

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 100,
                    StatusMessage = $"{GetResourceString("Downloadcomplete2", "Download complete")}: {FormatFileSize(totalBytesRead)}"
                });
            }
            else if (totalBytes.HasValue)
            {
                IsDownloadCompleted = false;

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 0,
                    StatusMessage = $"{GetResourceString("Downloadincomplete", "Download incomplete")}: " +
                                    $"{GetResourceString("Expected", "Expected")} {FormatFileSize(totalBytes.Value)} " +
                                    $"{GetResourceString("butreceived", "but received")} {FormatFileSize(totalBytesRead)}"
                });

                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }
            else
            {
                // If the server didn't provide a content length, assume the download is complete
                IsDownloadCompleted = true;

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytesRead,
                    ProgressPercentage = 100,
                    StatusMessage = $"{GetResourceString("Downloadcomplete2", "Download complete")}: {FormatFileSize(totalBytesRead)}"
                });
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Notify developer
            await LogErrors.LogErrorAsync(ex, $"The requested file was not available on the server.\nURL: {downloadUrl}");

            // Notify user
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = GetResourceString("ErrorFilenotfoundontheserver", "Error: File not found on the server.")
            });
            throw;
        }
        catch (HttpRequestException ex)
        {
            // Notify developer
            await LogErrors.LogErrorAsync(ex, $"Network error during file download.\nURL: {downloadUrl}");

            // Notify user
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = $"{GetResourceString("Networkerror", "Network error")}: {ex.Message}"
            });
            throw;
        }
        catch (IOException ex)
        {
            // Notify developer
            await LogErrors.LogErrorAsync(ex, $"File read/write error after file download.\nURL: {downloadUrl}");

            // Notify user
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = $"{GetResourceString("Fileerror", "File error")}: {ex.Message}"
            });
            throw;
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Notify developer
                await LogErrors.LogErrorAsync(ex, $"Download was canceled by the user.\nURL: {downloadUrl}");

                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("Downloadcanceledbyuser", "Download canceled by user")
                });
            }
            else
            {
                // Notify developer
                await LogErrors.LogErrorAsync(ex, $"Download timed out or was canceled unexpectedly.\nURL: {downloadUrl}");

                // Notify user
                OnProgressChanged(new DownloadProgressEventArgs
                {
                    ProgressPercentage = 0,
                    StatusMessage = GetResourceString("ErrorDownloadtimedout", "Download timed out or was canceled unexpectedly")
                });
            }

            throw;
        }
        catch (Exception ex)
        {
            // Notify developer
            await LogErrors.LogErrorAsync(ex, $"Generic download error.\nURL: {downloadUrl}");

            // Notify user
            OnProgressChanged(new DownloadProgressEventArgs
            {
                ProgressPercentage = 0,
                StatusMessage = $"{GetResourceString("Error", "Error")}: {ex.Message}"
            });
            throw;
        }
    }

    /// <summary>
    /// Checks if there is enough disk space available in the specified folder.
    /// </summary>
    /// <param name="folderPath">The folder path to check.</param>
    /// <param name="requiredSpace">The required space in bytes (default is 5GB).</param>
    /// <returns>True if enough space is available, otherwise false.</returns>
    private static bool CheckAvailableDiskSpace(string folderPath, long requiredSpace = 5368709120)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(folderPath) ?? throw new InvalidOperationException("Could not get the drive info"));
            return driveInfo.AvailableFreeSpace > requiredSpace;
        }
        catch
        {
            // If we can't check disk space, assume it's false
            // If disk space *cannot* be checked (e.g., network drive issues, permissions), assumes there's not enough space.
            return false;
        }
    }

    /// <summary>
    /// Formats a byte size into a human-readable format.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A formatted string representation of the size.</returns>
    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var counter = 0;
        double size = bytes;

        while (size > 1024 && counter < suffixes.Length - 1)
        {
            size /= 1024;
            counter++;
        }

        return $"{size:F2} {suffixes[counter]}";
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

    #region IDisposable Implementation

    /// <inheritdoc />
    /// <summary>
    /// Disposes the DownloadManager.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the DownloadManager resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _cancellationTokenSource?.Dispose();
            _httpClient?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~DownloadManager()
    {
        Dispose(false);
    }

    #endregion
}