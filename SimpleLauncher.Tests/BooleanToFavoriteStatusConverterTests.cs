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

    [Fact]
    public void ConvertBackThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack("Favorite", typeof(string), null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertTrueReturnsNonNullString()
    {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public void ConvertFalseReturnsNonNullString()
    {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public void ConvertTrueAndFalseReturnDifferentStrings()
    {
        var trueResult = (string)_converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture)!;
        var falseResult = (string)_converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture)!;
        Assert.NotEqual(trueResult, falseResult);
    }

    [Fact]
    public void ConvertNonBoolReturnsNonNullString()
    {
        var result = _converter.Convert("invalid", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public void ConvertNullReturnsNonNullString()
    {
        var result = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }
}
