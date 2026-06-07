using SimpleLauncher.Core.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

public class SanitizeInputSystemNameTests
{
    [Theory]
    [InlineData("Nintendo NES", false)]
    [InlineData("Sega Genesis", false)]
    [InlineData("Game<Name>", true)]
    [InlineData("Game:Name", true)]
    [InlineData("Game/Name", true)]
    [InlineData("Game\\Name", true)]
    [InlineData("Game|Name", true)]
    [InlineData("Game*Name", true)]
    [InlineData("Game?Name", true)]
    [InlineData("Game\"Name", true)]
    [InlineData("", false)]
    public void ContainsInvalidCharactersReturnsExpected(string name, bool expectedHasInvalid)
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters(name, out var invalidChars);
        Assert.Equal(expectedHasInvalid, result);
        if (expectedHasInvalid)
        {
            Assert.NotEmpty(invalidChars);
        }
        else
        {
            Assert.Empty(invalidChars);
        }
    }

    [Fact]
    public void ContainsInvalidCharactersNullReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters(null, out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Theory]
    [InlineData("Nintendo NES", "Nintendo NES")]
    [InlineData("Game<Name>", "Game_Name_")]
    [InlineData("Game:Name", "Game_Name")]
    [InlineData("Game/Name", "Game_Name")]
    [InlineData("Game\\Name", "Game_Name")]
    [InlineData("Game|Name", "Game_Name")]
    [InlineData("Game*Name", "Game_Name")]
    [InlineData("Game?Name", "Game_Name")]
    [InlineData("Game\"Name", "Game_Name")]
    [InlineData("CON", "_CON_")]
    [InlineData("com1", "_com1_")]
    [InlineData("..", "_")]
    [InlineData(".", "_invalid_sanitized_name_")]
    [InlineData("", "_invalid_empty_name_")]
    [InlineData("  ", "_invalid_empty_name_")]
    [InlineData(" Game Name ", "Game Name")]
    public void SanitizeFolderNameReturnsExpected(string name, string expected)
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeFolderNameNullReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(null);
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedNameIsEscaped()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("PRN");
        Assert.Equal("_PRN_", result);
    }

    [Fact]
    public void SanitizeFolderNameDirectoryTraversalIsReplaced()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("../etc/passwd");
        Assert.DoesNotContain("..", result);
    }
}
