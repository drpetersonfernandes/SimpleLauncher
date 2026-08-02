using System.Globalization;
using SimpleLauncher.New.Converters;

namespace SimpleLauncher.New.Tests;

public class SmartTitleCaseConverterTests
{
    private readonly SmartTitleCaseConverter _converter = new();

    [Fact]
    public void Convert_AllUppercase_ReturnsTitleCase()
    {
        var result = _converter.Convert("SUPER MARIO BROS", null!, null!, CultureInfo.InvariantCulture);
        Assert.Equal("Super Mario Bros", result);
    }

    [Fact]
    public void Convert_AllLowercase_ReturnsTitleCase()
    {
        var result = _converter.Convert("the legend of zelda", null!, null!, CultureInfo.InvariantCulture);
        Assert.Equal("The Legend Of Zelda", result);
    }

    [Fact]
    public void Convert_MixedCase_ReturnsAsIs()
    {
        var result = _converter.Convert("Final Fantasy VII", null!, null!, CultureInfo.InvariantCulture);
        Assert.Equal("Final Fantasy VII", result);
    }

    [Fact]
    public void Convert_NullOrEmpty_ReturnsOriginal()
    {
        Assert.Null(_converter.Convert(null, null!, null!, CultureInfo.InvariantCulture));
        Assert.Equal("", _converter.Convert("", null!, null!, CultureInfo.InvariantCulture));
        Assert.Equal("   ", _converter.Convert("   ", null!, null!, CultureInfo.InvariantCulture));
    }
}
