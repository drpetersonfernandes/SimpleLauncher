using System.Collections.Concurrent;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SimpleLauncher.Avalonia.Converters;

/// <summary>
/// Two-tier image cache converter: weak ConcurrentDictionary + LRU strong tier (1500 entries).
/// Ported from Emutastic pattern. Loads cover art images asynchronously.
/// </summary>
public class PathToImageConverter : IValueConverter
{
    // Weak cache: allows GC to reclaim unused images
    private static readonly ConcurrentDictionary<string, WeakReference<Bitmap>> WeakCache = new();

    // Prune the weak cache when it grows past this size (dead weak references accumulate
    // until GC runs; this bounds the weak-reference table without blocking the UI thread).
    private const int WeakCachePruneThreshold = 4096;

    // Strong LRU cache: keeps recent images alive
    private static readonly LinkedList<(string Path, Bitmap Img)> LruList = new();
    private static readonly Dictionary<string, LinkedListNode<(string Path, Bitmap Img)>> LruIndex = new();
    private const int LruCapacity = 1500;
    private static readonly object LruLock = new();

    // Default placeholder
    private static Bitmap? _placeholder;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrEmpty(path))
            return GetPlaceholder();

        // Check weak cache
        if (WeakCache.TryGetValue(path, out var weakRef) && weakRef.TryGetTarget(out var cached))
            return cached;

        // Check strong LRU cache
        lock (LruLock)
        {
            if (LruIndex.TryGetValue(path, out var node))
            {
                TouchLru(node);
                return node.Value.Img;
            }
        }

        // Load asynchronously via binding
        // Return placeholder immediately; the binding system will update when loaded
        var image = LoadImage(path);
        return image ?? GetPlaceholder();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Clears all caches.
    /// </summary>
    public static void ClearCache()
    {
        WeakCache.Clear();
        lock (LruLock)
        {
            LruList.Clear();
            LruIndex.Clear();
        }
    }

    private static Bitmap? LoadImage(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;

            // Avalonia Bitmap is immutable and safe for cross-thread use once created.
            using var stream = File.OpenRead(path);
            var image = Bitmap.DecodeToWidth(stream, 300);

            // Add to caches
            WeakCache[path] = new WeakReference<Bitmap>(image);
            PruneWeakCacheIfNeeded();

            lock (LruLock)
            {
                if (LruIndex.TryGetValue(path, out var existing))
                {
                    LruList.Remove(existing);
                }
                else if (LruList.Count >= LruCapacity)
                {
                    var oldest = LruList.Last!;
                    LruIndex.Remove(oldest.Value.Path);
                    LruList.RemoveLast();
                }

                var node = LruList.AddFirst((path, image));
                LruIndex[path] = node;
            }

            return image;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to load image in PathToImageConverter");
            return null;
        }
    }

    private static void TouchLru(LinkedListNode<(string, Bitmap)> node)
    {
        LruList.Remove(node);
        LruList.AddFirst(node);
    }

    /// <summary>
    /// Removes dead entries (GC-collected images) from the weak cache once it grows
    /// past the threshold, so the weak-reference table stays bounded.
    /// </summary>
    private static void PruneWeakCacheIfNeeded()
    {
        if (WeakCache.Count < WeakCachePruneThreshold) return;

        foreach (var entry in WeakCache)
        {
            if (!entry.Value.TryGetTarget(out _))
            {
                WeakCache.TryRemove(entry.Key, out _);
            }
        }
    }

    private static Bitmap? GetPlaceholder()
    {
        if (_placeholder is not null) return _placeholder;

        // Use the bundled default image when available
        var defaultPath = Path.Combine(AppContext.BaseDirectory, "images", "default.png");
        try
        {
            if (File.Exists(defaultPath))
            {
                using var stream = File.OpenRead(defaultPath);
                _placeholder = Bitmap.DecodeToWidth(stream, 300);
            }
            else
            {
                // No default image available — 1x1 transparent placeholder (zeroed pixels = transparent)
                _placeholder = new WriteableBitmap(
                    new PixelSize(1, 1),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul);
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to create placeholder image");
        }

        return _placeholder;
    }
}