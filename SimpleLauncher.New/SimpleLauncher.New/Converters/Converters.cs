using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleLauncher.New.Converters;

/// <summary>
/// Converts a boolean to Visibility.Visible (true) or Visibility.Collapsed (false).
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}

/// <summary>
/// Converts a boolean to Visibility.Collapsed (true) or Visibility.Visible (false) — inverse of BoolToVisibility.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not Visibility.Visible;
    }
}

/// <summary>
/// Converts null to Visibility.Collapsed, non-null to Visibility.Visible.
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Inverse of NullToVisibilityConverter: non-null collapses, null shows.
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts a string to Visibility.Collapsed if null or empty, Visibility.Visible otherwise.
/// </summary>
[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Freezable proxy that exposes a Data property for use in MultiBinding scenarios
/// where DynamicResource values need to be consumed by converters.
/// </summary>
public class BindingProxy : Freezable
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy),
            new UIPropertyMetadata(null));

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
