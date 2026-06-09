using SimpleLauncher.Core.Services.DownloadService.Models;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaDownloadManager : IDisposable
{
    private readonly HttpClient _httpClient = new();
    private CancellationTokenSource _cts = new();
    private bool _disposed;

    public async Task DownloadAndExtractImagePackAsync(
        ImagePackDownloadItem item,
        Action<double> progressCallback,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(item.DownloadUrl))
            throw new InvalidOperationException("Download URL is not set.");

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        var token = linkedCts.Token;

        var tempPath = Path.Combine(Path.GetTempPath(), "SimpleLauncher", "ImagePacks");
        Directory.CreateDirectory(tempPath);

        var fileName = Path.GetFileName(new Uri(item.DownloadUrl).LocalPath);
        var filePath = Path.Combine(tempPath, fileName);

        try
        {
            using var response = await _httpClient.GetAsync(item.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var totalBytesRead = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(token);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, token)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                totalBytesRead += bytesRead;

                if (totalBytes > 0)
                {
                    var progress = (double)totalBytesRead / totalBytes * 100;
                    progressCallback?.Invoke(progress);
                }
            }

            progressCallback?.Invoke(100);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to download image pack: {ex.Message}", ex);
        }
    }

    public void Cancel()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _cts.Dispose();
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
