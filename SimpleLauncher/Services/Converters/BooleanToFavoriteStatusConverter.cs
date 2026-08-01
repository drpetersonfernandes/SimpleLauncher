using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleLauncher.Services.Converters;

/// <summary>
/// Converts a boolean value to a localized string indicating favorite status for accessibility.
/// </summary>
public class BooleanToFavoriteStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isFavorite)
        {
            return isFavorite
                ? (Application.Current?.TryFindResource("FavoriteStatusLabel") as string ?? "Favorite")
                : (Application.Current?.TryFindResource("NotFavoriteStatusLabel") as string ?? "Not Favorite");
        }

        return Application.Current?.TryFindResource("UnknownFavoriteStatusLabel") as string ?? "Unknown Favorite Status";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
