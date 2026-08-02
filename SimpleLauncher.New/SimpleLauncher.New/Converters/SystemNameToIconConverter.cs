using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SimpleLauncher.New.Converters;

/// <summary>
/// Converts a system name to a 20x20 sidebar icon from images/systems/.
/// Falls back to images/systems/default.png if the named icon is missing.
/// </summary>
[ValueConversion(typeof(string), typeof(BitmapImage))]
public class SystemNameToIconConverter : IValueConverter
{
    private static readonly string IconsPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "images", "systems");

    private static readonly Dictionary<string, BitmapImage?> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static BitmapImage? _defaultIcon;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string systemName || string.IsNullOrWhiteSpace(systemName))
            return GetDefaultIcon();

        if (Cache.TryGetValue(systemName, out var cached))
            return cached ?? GetDefaultIcon();

        var icon = LoadIcon(systemName);
        Cache[systemName] = icon;
        return icon ?? GetDefaultIcon();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static BitmapImage? LoadIcon(string systemName)
    {
        // Try exact match first
        var path = Path.Combine(IconsPath, systemName + ".png");
        if (File.Exists(path)) return CreateImage(path);

        // Try with common aliases
        var aliases = GetAliases(systemName);
        foreach (var alias in aliases)
        {
            path = Path.Combine(IconsPath, alias + ".png");
            if (File.Exists(path)) return CreateImage(path);
        }

        return null;
    }

    private static string[] GetAliases(string systemName)
    {
        // Strip manufacturer prefix (e.g., "Nintendo NES" → "NES")
        var prefixes = new[]
        {
            "Nintendo ", "Sega ", "Sony ", "NEC ", "SNK ", "Microsoft ",
            "Atari ", "Bandai ", "Commodore ", "Panasonic ", "Philips ",
            "Sinclair ", "Magnavox ", "Mattel ", "Casio "
        };
        var aliases = new List<string>();
        foreach (var prefix in prefixes)
        {
            if (systemName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                aliases.Add(systemName[prefix.Length..]);
                break;
            }
        }

        aliases.Add(systemName); // original name
        return aliases.ToArray();
    }

    private static BitmapImage? CreateImage(string path)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 40;
            image.UriSource = new Uri(path);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to create system icon image");
            return null;
        }
    }

    private static BitmapImage GetDefaultIcon()
    {
        if (_defaultIcon is not null) return _defaultIcon;

        var defaultPath = Path.Combine(IconsPath, "default.png");
        _defaultIcon = File.Exists(defaultPath) ? CreateImage(defaultPath) : null;
        return _defaultIcon!;
    }
}
