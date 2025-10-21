using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleLauncher.Converters;

/// <summary>
/// Converts a boolean value to its inverse.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }

        return value; // Return original value if not a boolean
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }

        return value; // Return original value if not a boolean
    }
}