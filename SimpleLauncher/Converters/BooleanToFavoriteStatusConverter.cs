using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleLauncher.Converters;

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
                ? (string)Application.Current.TryFindResource("FavoriteStatusLabel") ?? "Favorite"
                : (string)Application.Current.TryFindResource("NotFavoriteStatusLabel") ?? "Not Favorite";
        }

        return (string)Application.Current.TryFindResource("UnknownFavoriteStatusLabel") ?? "Unknown Favorite Status";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}