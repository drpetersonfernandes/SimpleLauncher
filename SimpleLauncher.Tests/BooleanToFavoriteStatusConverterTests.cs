using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SimpleLauncher.Services.Converters;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="BooleanToFavoriteStatusConverter"/> class.
/// </summary>
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class BooleanToFavoriteStatusConverterTests
{
    private readonly BooleanToFavoriteStatusConverter _converter = new();

    /// <summary>
    /// Verifies that ConvertBack throws NotSupportedException since the converter only supports one-way conversion.
    /// </summary>
    [Fact]
    public void ConvertBackThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack("Favorite", typeof(string), null!, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Verifies that converting a true boolean value returns a non-null string.
    /// </summary>
    [Fact]
    public void ConvertTrueReturnsNonNullString()
    {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    /// <summary>
    /// Verifies that converting a false boolean value returns a non-null string.
    /// </summary>
    [Fact]
    public void ConvertFalseReturnsNonNullString()
    {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    /// <summary>
    /// Verifies that converting true and false produces two distinct string representations.
    /// </summary>
    [Fact]
    public void ConvertTrueAndFalseReturnDifferentStrings()
    {
        var trueResult = (string)_converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        var falseResult = (string)_converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotEqual(trueResult, falseResult, StringComparer.Ordinal);
    }

    /// <summary>
    /// Verifies that converting a non-boolean value returns a non-null string without throwing.
    /// </summary>
    [Fact]
    public void ConvertNonBoolReturnsNonNullString()
    {
        var result = _converter.Convert("invalid", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    /// <summary>
    /// Verifies that converting a null value returns a non-null string without throwing.
    /// </summary>
    [Fact]
    public void ConvertNullReturnsNonNullString()
    {
        var result = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }
}
