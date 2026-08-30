using System.Collections.Concurrent;
using Avalonia.Media.Imaging;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
///     Loads remote (HTTP) images as <see cref="Bitmap" /> with a bounded cache.
///     Shared by the RetroAchievements UI and the RemoteImage control.
/// </summary>
public static class RemoteImageLoader
{
    private const int MaxCacheEntries = 512;
    private static readonly HttpClient Http = new();
    private static readonly ConcurrentDictionary<string, Bitmap> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Lock CacheLock = new();

    /// <summary>
    ///     Loads an image from a URL (with cache). Returns null on failure.
    /// </summary>
    public static async Task<Bitmap?> LoadAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (Cache.TryGetValue(url, out var cached))
            return cached;

        try
        {
            var bytes = await Http.GetByteArrayAsync(url).ConfigureAwait(false);
            using var ms = new MemoryStream(bytes);
            var bitmap = Bitmap.DecodeToWidth(ms, 400);

            lock (CacheLock)
            {
                if (Cache.Count >= MaxCacheEntries) Cache.Clear();

                Cache[url] = bitmap;
            }

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Gets a cached image, if present.
    /// </summary>
    public static Bitmap? GetCached(string? url)
    {
        return url is not null && Cache.TryGetValue(url, out var cached) ? cached : null;
    }
}