using SimpleLauncher.Services.CheckPaths;
using Xunit;

namespace SimpleLauncher.Tests;

public class PathHelperTests
{
    [Theory]
    [InlineData("%BASEFOLDER%\\tools", false)]
    [InlineData("tools", true)]
    [InlineData(".", true)]
    [InlineData("..", true)]
    [InlineData("C:\\Windows", false)]
    [InlineData("%BASEFOLDER%", false)]
    [InlineData("", false)]
    public void IsRelativePathWithoutBaseFolder_ReturnsExpected(string path, bool expected)
    {
        var result = PathHelper.IsRelativePathWithoutBaseFolder(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:\\MyFolder\\", "C:\\MyFolder")]
    [InlineData("C:\\MyFolder//", "C:\\MyFolder")]
    [InlineData("C:\\MyFolder", "C:\\MyFolder")]
    [InlineData("", "")]
    public void SanitizePathToken_RemovesTrailingSeparators(string input, string expected)
    {
        var result = PathHelper.SanitizePathToken(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizePathToken_Null_ReturnsEmpty()
    {
        var result = PathHelper.SanitizePathToken(null);
        Assert.Equal("", result);
    }

    [Theory]
    [InlineData("C:\\test.txt", "test")]
    [InlineData("C:\\folder\\file.zip", "file")]
    [InlineData("file", "file")]
    public void GetFileNameWithoutExtension_ReturnsExpected(string path, string expected)
    {
        var result = PathHelper.GetFileNameWithoutExtension(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:\\test.txt", "test.txt")]
    [InlineData("C:\\folder\\file.zip", "file.zip")]
    [InlineData("file", "file")]
    public void GetFileName_ReturnsExpected(string path, string expected)
    {
        var result = PathHelper.GetFileName(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("%ROM%", true)]
    [InlineData("%GAME%", true)]
    [InlineData("$rom$", true)]
    [InlineData("{rom}", true)]
    [InlineData("C:\\test.txt", false)]
    [InlineData("-f", false)]
    [InlineData("", false)]
    public void ContainsGameSpecificPlaceholder_ReturnsExpected(string text, bool expected)
    {
        var result = PathHelper.ContainsGameSpecificPlaceholder(text);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ContainsGameSpecificPlaceholder_Null_ReturnsFalse()
    {
        var result = PathHelper.ContainsGameSpecificPlaceholder(null);
        Assert.False(result);
    }

    [Theory]
    [InlineData("C:\\very\\long\\path", "\\\\?\\C:\\very\\long\\path")]
    [InlineData("\\\\server\\share", "\\\\?\\UNC\\server\\share")]
    [InlineData("\\\\?\\C:\\already\\extended", "\\\\?\\C:\\already\\extended")]
    [InlineData("", "")]
    public void GetLongPath_ReturnsExpected(string path, string expected)
    {
        var result = PathHelper.GetLongPath(path);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLongPath_Null_ReturnsNull()
    {
        var result = PathHelper.GetLongPath(null);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectory_EmptyString_ReturnsEmpty()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory("");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectory_Null_ReturnsEmpty()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectory_RelativePath_ReturnsAbsolute()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory(".");
        Assert.True(Path.IsPathRooted(result));
    }
}
