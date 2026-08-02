using System.Globalization;
using System.Windows.Data;

namespace SimpleLauncher.New.Converters;

/// <summary>
/// Normalizes game titles: all-uppercase → Title Case, all-lowercase → Title Case, mixed case → as-is.
/// </summary>
[ValueConversion(typeof(string), typeof(string))]
public class SmartTitleCaseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string title || string.IsNullOrWhiteSpace(title))
            return value;

        var isAllUpper = title.All(c => !char.IsLetter(c) || char.IsUpper(c));
        var isAllLower = title.All(c => !char.IsLetter(c) || char.IsLower(c));

        if (isAllUpper || isAllLower)
        {
            return culture.TextInfo.ToTitleCase(title.ToLower(culture));
        }

        return title;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
