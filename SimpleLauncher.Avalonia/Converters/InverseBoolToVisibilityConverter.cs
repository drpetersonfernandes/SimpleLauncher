using System.Globalization;
using Avalonia.Data.Converters;

namespace SimpleLauncher.Avalonia.Converters;

/// <summary>
///     Converts a boolean to collapsed (true) / IsVisible (false) — inverse of BoolToVisibilityConverter.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not true;
    }
}