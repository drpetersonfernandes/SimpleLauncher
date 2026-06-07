using System.Windows.Data;
using SimpleLauncher.Services.Converters;
using Xunit;

namespace SimpleLauncher.Tests;

public class InverseBooleanConverterExtendedTests
{
    private readonly InverseBooleanConverter _converter = new();

    [Fact]
    public void ConvertTrueReturnsFalse()
    {
        var result = _converter.Convert(true, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertFalseReturnsTrue()
    {
        var result = _converter.Convert(false, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void ConvertBackTrueReturnsFalse()
    {
        var result = _converter.ConvertBack(true, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBackFalseReturnsTrue()
    {
        var result = _converter.ConvertBack(false, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void ConvertAndConvertBackAreInverse()
    {
        const bool original = true;
        var converted = _converter.Convert(original, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        var roundTrip = _converter.ConvertBack(converted, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void ConvertBackAndConvertAreInverse()
    {
        const bool original = false;
        var converted = _converter.ConvertBack(original, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        var roundTrip = _converter.Convert(converted, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void ConvertNonBoolReturnsOriginalValue()
    {
        var result = _converter.Convert("true", typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal("true", result);
    }

    [Fact]
    public void ConvertIntReturnsOriginalValue()
    {
        var result = _converter.Convert(1, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(1, result);
    }

    [Fact]
    public void ConvertNullReturnsNull()
    {
        var result = _converter.Convert(null, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    [Fact]
    public void ImplementsIValueConverter()
    {
        Assert.IsAssignableFrom<IValueConverter>(_converter);
    }
}
