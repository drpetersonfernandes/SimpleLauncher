using System.Diagnostics;
using System.Net.Http;
using System.IO;

namespace Updater.Services;

/// <summary>
/// Provides progress information for download operations.
/// </summary>
public class DownloadProgressInfo
{
    /// <summary>
    /// The percentage of completion (0-100), or -1 if the total size is unknown.
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// The number of bytes read so far.
    /// </summary>
    public long BytesRead { get; set; }

    /// <summary>
    /// The total number of bytes to download, or 0 if unknown.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// The download speed in bytes per second.
    /// </summary>
    public double BytesPerSecond { get; set; }
}

/// <summary>
/// Service for downloading files with progress reporting.
/// </summary>
public class DownloadService
{
    private const int FileBufferSize = 81920; // 80KB buffer for efficient file I/O

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Event raised when download progress changes.
    /// </summary>
    public event Action<DownloadProgressInfo>? ProgressChanged;

    /// <summary>
    /// Event raised when a log message needs to be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Initializes a new instance of the DownloadService class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for downloads.</param>
    public DownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Downloads a file to a memory stream with progress reporting.
    /// </summary>
    /// <param name="url">The URL to download from.</param>
    /// <param name="cancellationToken">Token to cancel the download operation.</param>
    /// <returns>A MemoryStream containing the downloaded file.</returns>
    /// <exception cref="HttpRequestException">Thrown when the download fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task<MemoryStream> DownloadToMemoryAsync(string url, CancellationToken cancellationToken = default)
    {
        LogMessage?.Invoke("Downloading the update file...");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                throw new HttpRequestException($"Failed to download the update file. Status Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await BugReportService.ReportBugAsync(ex, $"HTTP request failed for URL: {url}");
            throw;
        }

        using (response)
        {
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var memoryStream = new MemoryStream();
            var buffer = new byte[FileBufferSize];
            var totalBytesRead = 0L;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                while (true)
                {
                    try
                    {
                        var bytesRead = await contentStream.ReadAsync(buffer, cancellationToken);
                        if (bytesRead == 0) break;

                        await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        totalBytesRead += bytesRead;

                        // Calculate and report progress
                        if (totalBytes > 0)
                        {
                            var percentage = (double)totalBytesRead / totalBytes * 100;
                            var elapsedSeconds = Math.Max(stopwatch.ElapsedMilliseconds / 1000.0, 0.001);
                            var speed = totalBytesRead / elapsedSeconds;

                            ProgressChanged?.Invoke(new DownloadProgressInfo
                            {
                                Percentage = percentage,
                                BytesRead = totalBytesRead,
                                TotalBytes = totalBytes,
                                BytesPerSecond = speed
                            });
                        }
                        else
                        {
                            // Unknown file size - report bytes downloaded only
                            ProgressChanged?.Invoke(new DownloadProgressInfo
                            {
                                Percentage = -1,
                                BytesRead = totalBytesRead,
                                TotalBytes = 0,
                                BytesPerSecond = 0
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await BugReportService.ReportBugAsync(ex, "Error reading from download stream or writing to memory stream");
                        throw;
                    }
                }

                stopwatch.Stop();
                memoryStream.Position = 0;
                LogMessage?.Invoke("Download complete");
                return memoryStream;
            }
            catch (Exception ex)
            {
                await memoryStream.DisposeAsync();
                // Don't report here if it was already reported in the inner catch
                if (ex is not HttpRequestException and not IOException)
                {
                    await BugReportService.ReportBugAsync(ex, "Error downloading update file (outer catch)");
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Formats a byte count into a human-readable string.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A formatted string (e.g., "1.5 MB").</returns>
    public static string FormatBytes(double bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;

        return bytes switch
        {
            >= gb => $"{bytes / gb:F2} GB",
            >= mb => $"{bytes / mb:F2} MB",
            >= kb => $"{bytes / kb:F2} KB",
            _ => $"{bytes:F0} B"
        };
    }
}
