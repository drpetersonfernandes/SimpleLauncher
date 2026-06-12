using SimpleLauncher.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the SanitizeInputSystemName utility for detecting invalid characters and sanitizing folder names.
/// </summary>
public class SanitizeInputSystemNameTests
{
    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns the expected result for various system names including names with invalid filesystem characters.
    /// </summary>
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

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns false when given a null input.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersNullReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters(null, out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName produces the expected sanitized output for various inputs including reserved names and special characters.
    /// </summary>
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

    /// <summary>
    /// Verifies that SanitizeFolderName returns a placeholder when given null input.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameNullReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(null);
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName escapes Windows reserved names like PRN.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedNameIsEscaped()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("PRN");
        Assert.Equal("_PRN_", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName removes directory traversal sequences from the input.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameDirectoryTraversalIsReplaced()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("../etc/passwd");
        Assert.DoesNotContain("..", result);
    }
}
