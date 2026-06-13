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

    [Fact]
    public void ContainsInvalidCharactersWithDotsReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("N.E.S.", out _);
        Assert.False(result);
    }

    [Fact]
    public void ContainsInvalidCharactersWithBackslashReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES\\SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\\', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithSlashReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES/SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('/', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithColonReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES:SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains(':', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithAsteriskReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES*SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('*', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithQuestionMarkReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES?SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('?', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithPipeReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES|SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('|', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWhitespaceReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("   ", out _);
        Assert.False(result);
    }

    [Fact]
    public void ContainsInvalidPathCharactersValidPathReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidPathCharacters(@"C:\roms\NES", out _);
        Assert.False(result);
    }

    [Fact]
    public void ContainsInvalidPathCharactersEmptyReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidPathCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidPathCharactersNullReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidPathCharacters(null, out _);
        Assert.False(result);
    }

    [Fact]
    public void SanitizeFolderNameWithSpacesReturnsUnchanged()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("Super Nintendo");
        Assert.Equal("Super Nintendo", result);
    }

    [Fact]
    public void SanitizeFolderNameWithDoubleDotsReplacesTraversal()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES..SNES");
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedCon()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("CON");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedAux()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("AUX");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedNul()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NUL");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedCom1()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("COM1");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedLpt1()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("LPT1");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedCaseInsensitive()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("con");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameWithInvalidChars()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES/SNES");
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void SanitizeFolderNameTrimsDotsAndSpaces()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(" NES ");
        Assert.Equal("NES", result);
    }

    [Fact]
    public void SanitizeFolderNameWithMultipleInvalidChars()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES<>:\"/\\|?*SNES");
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.DoesNotContain(":", result);
        Assert.DoesNotContain("\"", result);
        Assert.DoesNotContain("/", result);
        Assert.DoesNotContain("\\", result);
        Assert.DoesNotContain("|", result);
        Assert.DoesNotContain("?", result);
        Assert.DoesNotContain("*", result);
    }

    [Fact]
    public void SanitizeFolderNamePreservesValidCharacters()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES-GBA_v2.0");
        Assert.Equal("NES-GBA_v2.0", result);
    }
}
