using System.Globalization;
using System.Windows;

namespace SimpleLauncher.New.Tests;

public class ConverterTests
{
    [Fact]
    public void BoolToVisibility_True_ReturnsVisible()
    {
        var converter = new Converters.BoolToVisibilityConverter();
        Assert.Equal(Visibility.Visible, converter.Convert(true, null!, null!, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert(false, null!, null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void InverseBoolToVisibility_True_ReturnsCollapsed()
    {
        var converter = new Converters.InverseBoolToVisibilityConverter();
        Assert.Equal(Visibility.Collapsed, converter.Convert(true, null!, null!, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert(false, null!, null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullToVisibility_Null_ReturnsCollapsed()
    {
        var converter = new Converters.NullToVisibilityConverter();
        Assert.Equal(Visibility.Collapsed, converter.Convert(null, null!, null!, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert("something", null!, null!, CultureInfo.InvariantCulture));
    }
}
