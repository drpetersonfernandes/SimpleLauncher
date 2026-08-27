using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleLauncher.Services.Converters;

/// <summary>
/// Converts a boolean value to a localized string indicating favorite status for accessibility.
/// </summary>
public class BooleanToFavoriteStatusConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a localized favorite status string.
    /// </summary>
    /// <param name="value">The boolean value indicating whether the game is a favorite.</param>
    /// <param name="targetType">The target type of the conversion (unused).</param>
    /// <param name="parameter">The converter parameter (unused).</param>
    /// <param name="culture">The culture used for the conversion (unused).</param>
    /// <returns>A localized favorite status string.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isFavorite)
        {
            return isFavorite
                ? (Application.Current?.TryFindResource("FavoriteStatusLabel") as string ?? "Favorite")
                : (Application.Current?.TryFindResource("NotFavoriteStatusLabel") as string ?? "Not Favorite");
        }

        return Application.Current?.TryFindResource("UnknownFavoriteStatusLabel") as string ??
               "Unknown Favorite Status";
    }

    /// <summary>
    /// Converts a favorite status string back to a boolean value.
    /// </summary>
    /// <param name="value">The value to convert (unused).</param>
    /// <param name="targetType">The target type of the conversion (unused).</param>
    /// <param name="parameter">The converter parameter (unused).</param>
    /// <param name="culture">The culture used for the conversion (unused).</param>
    /// <returns>This method is not supported and always throws.</returns>
    /// <exception cref="NotSupportedException">Thrown because the conversion is not supported.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}