using System.Globalization;
using SimpleLauncher.Services.Converters;
using Xunit;

namespace SimpleLauncher.Tests;

public class InverseBooleanConverterTests
{
    private readonly InverseBooleanConverter _converter = new();

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBooleanReturnsInverse(bool input, bool expected)
    {
        var result = _converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBackBooleanReturnsInverse(bool input, bool expected)
    {
        var result = _converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertNonBooleanReturnsOriginalValue()
    {
        var result = _converter.Convert("hello", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ConvertBackNonBooleanReturnsOriginalValue()
    {
        var result = _converter.ConvertBack(42, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(42, result);
    }
}
