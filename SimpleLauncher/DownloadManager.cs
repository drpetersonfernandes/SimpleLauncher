using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

public class DownloadManager
{
    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath,
        IProgress<DownloadProgressInfo> progress, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var unknownsize2 = (string)Application.Current.TryFindResource("unknownsize") ?? "unknown size";
            var totalBytes = response.Content.Headers.ContentLength;
            var totalSizeFormatted = totalBytes.HasValue
                ? FormatFileSize(totalBytes.Value)
                : unknownsize2;

            var startingdownload2 = (string)Application.Current.TryFindResource("Startingdownload2") ?? "Starting download";
            progress.Report(new DownloadProgressInfo
            {
                BytesReceived = 0,
                TotalBytesToReceive = totalBytes,
                ProgressPercentage = 0,
                StatusMessage = $"{startingdownload2}: {Path.GetFileName(downloadUrl)} ({totalSizeFormatted})"
            });

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create,
                FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;
            var lastProgressUpdate = DateTime.Now;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                // Limit progress updates to reduce UI thread congestion (update every ~100ms)
                var now = DateTime.Now;
                if ((now - lastProgressUpdate).TotalMilliseconds >= 100)
                {
                    var progressPercentage = totalBytes.HasValue
                        ? (double)totalBytesRead / totalBytes.Value * 100
                        : 0;

                    var sizeStatus = totalBytes.HasValue
                        ? $"{FormatFileSize(totalBytesRead)} of {FormatFileSize(totalBytes.Value)}"
                        : $"{FormatFileSize(totalBytesRead)} of {totalSizeFormatted}";

                    var downloading2 = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
                    progress.Report(new DownloadProgressInfo
                    {
                        BytesReceived = totalBytesRead,
                        TotalBytesToReceive = totalBytes,
                        ProgressPercentage = progressPercentage,
                        StatusMessage = $"{downloading2}: {sizeStatus} ({progressPercentage:F1}%)"
                    });

                    lastProgressUpdate = now;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
            {
                _isDownloadCompleted = true;
                var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete2") ?? "Download complete";
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 100,
                    StatusMessage = $"{downloadcomplete2}: {FormatFileSize(totalBytesRead)}"
                });
            }
            else if (totalBytes.HasValue)
            {
                _isDownloadCompleted = false;
                var downloadincomplete2 = (string)Application.Current.TryFindResource("Downloadincomplete") ?? "Download incomplete";
                var expected2 = (string)Application.Current.TryFindResource("Expected") ?? "Expected";
                var butreceived2 = (string)Application.Current.TryFindResource("butreceived") ?? "but received";
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 0,
                    StatusMessage = $"{downloadincomplete2}: {expected2} {FormatFileSize(totalBytes.Value)} {butreceived2} {FormatFileSize(totalBytesRead)}"
                });
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }
            else
            {
                // If the server didn't provide a content length, we assume the download is complete
                _isDownloadCompleted = true;
                var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete2") ?? "Download complete";
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytesRead, // Use received as total
                    ProgressPercentage = 100,
                    StatusMessage = $"{downloadcomplete2}: {FormatFileSize(totalBytesRead)}"
                });
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Notify developer
            var contextMessage = $"The requested file was not available on the server.\n" +
                                 $"URL: {downloadUrl}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.DownloadErrorMessageBox();
            var errorFilenotfoundontheserver2 = (string)Application.Current.TryFindResource("ErrorFilenotfoundontheserver") ?? "Error: File not found on the server";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = errorFilenotfoundontheserver2
            });
        }
        catch (HttpRequestException ex)
        {
            // Notify developer
            var contextMessage = $"Network error during file download.\n" +
                                 $"URL: {downloadUrl}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.DownloadErrorMessageBox();
            var networkerror2 = (string)Application.Current.TryFindResource("Networkerror") ?? "Network error";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"{networkerror2}: {ex.Message}"
            });
        }
        catch (IOException ex)
        {
            // Notify developer
            var contextMessage = $"File read/write error after file download.\n" +
                                 $"URL: {downloadUrl}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.IoExceptionMessageBox(_tempFolder);
            var fileerror2 = (string)Application.Current.TryFindResource("Fileerror") ?? "File error";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"{fileerror2}: {ex.Message}"
            });
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Notify developer
                var contextMessage = $"Download was canceled by the user. User was not notified.\n" +
                                     $"URL: {downloadUrl}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                var downloadcanceledbyuser2 = (string)Application.Current.TryFindResource("Downloadcanceledbyuser2") ?? "Download canceled by user";
                progress.Report(new DownloadProgressInfo
                {
                    ProgressPercentage = 0,
                    StatusMessage = downloadcanceledbyuser2
                });
            }
            else
            {
                // Notify developer
                var contextMessage = $"Download timed out or was canceled unexpectedly.\n" +
                                     $"URL: {downloadUrl}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.DownloadErrorMessageBox();
                var downloadtimedoutorwascanceledunexpectedly2 = (string)Application.Current.TryFindResource("Downloadtimedoutorwascanceledunexpectedly") ?? "Download timed out or was canceled unexpectedly";
                progress.Report(new DownloadProgressInfo
                {
                    ProgressPercentage = 0,
                    StatusMessage = downloadtimedoutorwascanceledunexpectedly2
                });
            }

            TryToDeleteDownloadedFile(destinationPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Generic download error.\n" +
                                 $"URL: {downloadUrl}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.DownloadErrorMessageBox();
            var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"{error2}: {ex.Message}"
            });
        }
    }

    // Progress information class
    private class DownloadProgressInfo
    {
        public long BytesReceived { get; set; }
        public long? TotalBytesToReceive { get; set; }
        public double ProgressPercentage { get; set; }
        public string StatusMessage { get; set; }
    }
}