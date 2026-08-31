using System.Globalization;
using Avalonia.Data.Converters;

namespace SimpleLauncher.Avalonia.Converters;

/// <summary>
///     Converts null to collapsed, non-null to IsVisible.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}