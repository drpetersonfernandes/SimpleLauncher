using System.Globalization;
using System.Windows.Media;
using SimpleLauncher.New.Converters;

namespace SimpleLauncher.New.Tests;

public class PathToImageConverterTests
{
    [Fact]
    public void Convert_NullPath_ReturnsPlaceholderWithoutThrowing()
    {
        var converter = new PathToImageConverter();

        var result = converter.Convert(null, typeof(ImageSource), null, CultureInfo.InvariantCulture);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ImageSource>(result);
    }

    [Fact]
    public void Convert_EmptyPath_ReturnsPlaceholderWithoutThrowing()
    {
        var converter = new PathToImageConverter();

        var result = converter.Convert("", typeof(ImageSource), null, CultureInfo.InvariantCulture);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ImageSource>(result);
    }

    [Fact]
    public void Convert_MissingFile_ReturnsPlaceholderWithoutThrowing()
    {
        var converter = new PathToImageConverter();

        var result = converter.Convert(@"C:\nonexistent\cover.png", typeof(ImageSource), null, CultureInfo.InvariantCulture);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ImageSource>(result);
    }
}
