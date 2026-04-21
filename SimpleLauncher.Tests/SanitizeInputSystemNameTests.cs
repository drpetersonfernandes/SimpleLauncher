using SimpleLauncher.Services.SanitizeInputString;
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
    public void ContainsInvalidCharacters_ReturnsExpected(string name, bool expectedHasInvalid)
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
    public void ContainsInvalidCharacters_Null_ReturnsFalse()
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
    public void SanitizeFolderName_ReturnsExpected(string name, string expected)
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeFolderName_Null_ReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(null);
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderName_ReservedName_IsEscaped()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("PRN");
        Assert.Equal("_PRN_", result);
    }

    [Fact]
    public void SanitizeFolderName_DirectoryTraversal_IsReplaced()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("../etc/passwd");
        Assert.DoesNotContain("..", result);
    }
}
