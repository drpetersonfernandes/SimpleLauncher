using System.Globalization;
using SimpleLauncher.Services.Converters;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="InverseBooleanConverter"/> class.
/// </summary>
public class InverseBooleanConverterTests
{
    private readonly InverseBooleanConverter _converter = new();

    /// <summary>
    /// Verifies that Convert returns the inverse of a boolean value.
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBooleanReturnsInverse(bool input, bool expected)
    {
        var result = _converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that ConvertBack returns the inverse of a boolean value.
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBackBooleanReturnsInverse(bool input, bool expected)
    {
        var result = _converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that Convert returns the original value for non-boolean input.
    /// </summary>
    [Fact]
    public void ConvertNonBooleanReturnsOriginalValue()
    {
        var result = _converter.Convert("hello", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("hello", result);
    }

    /// <summary>
    /// Verifies that ConvertBack returns the original value for non-boolean input.
    /// </summary>
    [Fact]
    public void ConvertBackNonBooleanReturnsOriginalValue()
    {
        var result = _converter.ConvertBack(42, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertAndConvertBackAreInverse()
    {
        const bool original = true;
        var converted = _converter.Convert(original, typeof(bool), null, CultureInfo.InvariantCulture);
        var roundTrip = _converter.ConvertBack(converted, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void ConvertBackAndConvertAreInverse()
    {
        const bool original = false;
        var converted = _converter.ConvertBack(original, typeof(bool), null, CultureInfo.InvariantCulture);
        var roundTrip = _converter.Convert(converted, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void ConvertIntReturnsOriginalValue()
    {
        var result = _converter.Convert(1, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(1, result);
    }

    [Fact]
    public void ConvertNullReturnsNull()
    {
        var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }
}
