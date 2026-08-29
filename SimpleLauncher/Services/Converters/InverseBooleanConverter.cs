using System.Globalization;
using System.Windows.Data;

namespace SimpleLauncher.Services.Converters;

/// <summary>
///     Converts a boolean value to its inverse.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    ///     Converts a boolean value to its inverse.
    /// </summary>
    /// <param name="value">The boolean value to invert.</param>
    /// <param name="targetType">The target type of the conversion (unused).</param>
    /// <param name="parameter">The converter parameter (unused).</param>
    /// <param name="culture">The culture used for the conversion (unused).</param>
    /// <returns>The inverted boolean value, or the original value if it is not a boolean.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;

        return value!; // Return original value if not a boolean
    }

    /// <summary>
    ///     Converts a boolean value back to its inverse.
    /// </summary>
    /// <param name="value">The boolean value to invert.</param>
    /// <param name="targetType">The target type of the conversion (unused).</param>
    /// <param name="parameter">The converter parameter (unused).</param>
    /// <param name="culture">The culture used for the conversion (unused).</param>
    /// <returns>The inverted boolean value, or the original value if it is not a boolean.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;

        return value!; // Return original value if not a boolean
    }
}