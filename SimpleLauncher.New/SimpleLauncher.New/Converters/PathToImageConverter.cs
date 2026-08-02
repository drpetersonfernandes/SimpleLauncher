using System.Collections.Concurrent;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleLauncher.New.Converters;

/// <summary>
/// Two-tier image cache converter: weak ConcurrentDictionary + LRU strong tier (1500 entries).
/// Ported from Emutastic pattern. Loads cover art images asynchronously.
/// </summary>
[ValueConversion(typeof(string), typeof(BitmapImage))]
public class PathToImageConverter : IValueConverter
{
    // Weak cache: allows GC to reclaim unused images
    private static readonly ConcurrentDictionary<string, WeakReference<BitmapImage>> WeakCache = new();

    // Strong LRU cache: keeps recent images alive
    private static readonly LinkedList<(string Path, BitmapImage Img)> LruList = new();
    private static readonly Dictionary<string, LinkedListNode<(string Path, BitmapImage Img)>> LruIndex = new();
    private const int LruCapacity = 1500;
    private static readonly object LruLock = new();

    // Default placeholder
    private static BitmapSource? _placeholder;

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
    /// Preloads images into the cache for fast display.
    /// </summary>
    public static async Task PreloadAsync(IEnumerable<string> paths)
    {
        foreach (var path in paths.Take(LruCapacity))
        {
            if (File.Exists(path))
            {
                await Task.Run(() => LoadImage(path));
            }
        }
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

    private static BitmapImage? LoadImage(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 300;
            image.UriSource = new Uri(path);
            image.EndInit();
            image.Freeze(); // Allow cross-thread access

            // Add to caches
            WeakCache[path] = new WeakReference<BitmapImage>(image);

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

    private static void TouchLru(LinkedListNode<(string, BitmapImage)> node)
    {
        LruList.Remove(node);
        LruList.AddFirst(node);
    }

    private static BitmapSource? GetPlaceholder()
    {
        if (_placeholder is not null) return _placeholder;

        // Use the bundled default image when available
        var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");
        try
        {
            if (File.Exists(defaultPath))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = 300;
                image.UriSource = new Uri(defaultPath);
                image.EndInit();
                image.Freeze();
                _placeholder = image;
            }
            else
            {
                // No default image available — 1x1 transparent placeholder
                var pixel = new byte[4]; // BGRA transparent black
                var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, pixel, 4);
                source.Freeze();
                _placeholder = source;
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to create placeholder image");
        }

        return _placeholder;
    }
}
