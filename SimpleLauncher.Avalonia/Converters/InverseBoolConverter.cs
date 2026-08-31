using System.Globalization;
using Avalonia.Data.Converters;

namespace SimpleLauncher.Avalonia.Converters;

/// <summary>
///     Inverts a boolean value. Used for IsEnabled bindings where a "downloaded" state should disable the button.
/// </summary>
public class InverseBoolConverter : IValueConverter
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