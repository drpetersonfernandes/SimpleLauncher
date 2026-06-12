using System.Windows.Data;
using SimpleLauncher.Services.Converters;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the <see cref="InverseBooleanConverter"/> class covering additional edge cases.
/// </summary>
public class InverseBooleanConverterExtendedTests
{
    private readonly InverseBooleanConverter _converter = new();

    /// <summary>
    /// Verifies that Convert returns false for true input.
    /// </summary>
    [Fact]
    public void ConvertTrueReturnsFalse()
    {
        var result = _converter.Convert(true, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    /// <summary>
    /// Verifies that Convert returns true for false input.
    /// </summary>
    [Fact]
    public void ConvertFalseReturnsTrue()
    {
        var result = _converter.Convert(false, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    /// <summary>
    /// Verifies that ConvertBack returns false for true input.
    /// </summary>
    [Fact]
    public void ConvertBackTrueReturnsFalse()
    {
        var result = _converter.ConvertBack(true, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    /// <summary>
    /// Verifies that ConvertBack returns true for false input.
    /// </summary>
    [Fact]
    public void ConvertBackFalseReturnsTrue()
    {
        var result = _converter.ConvertBack(false, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    /// <summary>
    /// Verifies that Convert followed by ConvertBack returns the original value.
    /// </summary>
    [Fact]
    public void ConvertAndConvertBackAreInverse()
    {
        const bool original = true;
        var converted = _converter.Convert(original, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        var roundTrip = _converter.ConvertBack(converted, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(original, roundTrip);
    }

    /// <summary>
    /// Verifies that ConvertBack followed by Convert returns the original value.
    /// </summary>
    [Fact]
    public void ConvertBackAndConvertAreInverse()
    {
        const bool original = false;
        var converted = _converter.ConvertBack(original, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        var roundTrip = _converter.Convert(converted, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(original, roundTrip);
    }

    /// <summary>
    /// Verifies that Convert returns the original value for non-bool input.
    /// </summary>
    [Fact]
    public void ConvertNonBoolReturnsOriginalValue()
    {
        var result = _converter.Convert("true", typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal("true", result);
    }

    /// <summary>
    /// Verifies that Convert returns the original value for integer input.
    /// </summary>
    [Fact]
    public void ConvertIntReturnsOriginalValue()
    {
        var result = _converter.Convert(1, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(1, result);
    }

    /// <summary>
    /// Verifies that Convert returns null for null input.
    /// </summary>
    [Fact]
    public void ConvertNullReturnsNull()
    {
        var result = _converter.Convert(null, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that InverseBooleanConverter implements IValueConverter.
    /// </summary>
    [Fact]
    public void ImplementsIValueConverter()
    {
        Assert.IsAssignableFrom<IValueConverter>(_converter);
    }
}
