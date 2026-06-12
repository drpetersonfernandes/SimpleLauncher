using System.Globalization;
using SimpleLauncher.Services.Converters;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="ImageUrlConverter"/> class.
/// </summary>
public class ImageUrlConverterTests
{
    private readonly ImageUrlConverter _converter = new();

    [Fact]
    public void ConvertBackThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack("test", typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertNullValueReturnsPlaceholder()
    {
        var result = _converter.Convert(null, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
    }

    [Fact]
    public void ConvertEmptyStringReturnsPlaceholder()
    {
        var result = _converter.Convert("", typeof(object), null, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
    }

    [Fact]
    public void ConvertWhitespaceStringReturnsPlaceholder()
    {
        var result = _converter.Convert("   ", typeof(object), null, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
    }

    [Fact]
    public void ConvertNonStringValueReturnsPlaceholder()
    {
        var result = _converter.Convert(42, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
    }

    [Fact]
    public void ConvertInvalidUrlReturnsPlaceholder()
    {
        var result = _converter.Convert("not-a-valid-url", typeof(object), null, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
    }

    [Fact]
    public void ConvertReturnsSamePlaceholderForNullAndEmpty()
    {
        var result1 = _converter.Convert(null, typeof(object), null, CultureInfo.InvariantCulture);
        var result2 = _converter.Convert("", typeof(object), null, CultureInfo.InvariantCulture);
        Assert.Same(result1, result2);
    }
}
