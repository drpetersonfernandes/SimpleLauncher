using System.Globalization;
using Avalonia.Data.Converters;

namespace SimpleLauncher.Avalonia.Converters;

/// <summary>
///     Converts a boolean to IsVisible (true) / collapsed (false).
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true;
    }
}