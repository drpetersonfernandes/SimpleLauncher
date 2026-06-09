using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace SimpleLauncher.Avalonia.Converters;

public class ImageUrlConverter : IValueConverter
{
    public static readonly ImageUrlConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string url && !string.IsNullOrWhiteSpace(url))
        {
            try
            {
                return new Bitmap(url);
            }
            catch
            {
                // If URL is invalid, return null
            }
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported for ImageUrlConverter");
    }
}
