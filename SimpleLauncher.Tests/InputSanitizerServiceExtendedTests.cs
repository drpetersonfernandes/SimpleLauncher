using SimpleLauncher.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="InputSanitizerService"/> covering additional edge cases
/// for reserved names, special characters, and sanitization behavior.
/// </summary>
public class InputSanitizerServiceExtendedTests
{
    private readonly InputSanitizerService _sanitizer = new();

    [Theory]
    [InlineData("AUX")]
    [InlineData("aux")]
    [InlineData("Aux")]
    public void SanitizeFolderNameReservedAuxEscaped(string name)
    {
        var result = _sanitizer.SanitizeFolderName(name);
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Theory]
    [InlineData("COM2")]
    [InlineData("COM3")]
    [InlineData("COM4")]
    [InlineData("COM5")]
    [InlineData("COM6")]
    [InlineData("COM7")]
    [InlineData("COM8")]
    [InlineData("COM9")]
    public void SanitizeFolderNameReservedComAllEscaped(string name)
    {
        var result = _sanitizer.SanitizeFolderName(name);
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Theory]
    [InlineData("LPT2")]
    [InlineData("LPT3")]
    [InlineData("LPT4")]
    [InlineData("LPT5")]
    [InlineData("LPT6")]
    [InlineData("LPT7")]
    [InlineData("LPT8")]
    [InlineData("LPT9")]
    public void SanitizeFolderNameReservedLptAllEscaped(string name)
    {
        var result = _sanitizer.SanitizeFolderName(name);
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameDoubleDotsMultipleReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("a..b..c..d");
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void SanitizeFolderNameLeadingTrailingDotsTrimmed()
    {
        var result = _sanitizer.SanitizeFolderName("...test...");
        Assert.Equal("test", result);
    }

    [Fact]
    public void SanitizeFolderNameLeadingTrailingSpacesTrimmed()
    {
        var result = _sanitizer.SanitizeFolderName("   test   ");
        Assert.Equal("test", result);
    }

    [Fact]
    public void SanitizeFolderNameOnlyDotsBecomesEmpty()
    {
        var result = _sanitizer.SanitizeFolderName("...");
        // After trimming dots, it becomes empty, then sanitized
        Assert.Equal("_invalid_sanitized_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameOnlySpacesBecomesPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName("     ");
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameColonReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("C:drive");
        Assert.DoesNotContain(":", result);
    }

    [Fact]
    public void SanitizeFolderNameAsteriskReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game*name");
        Assert.DoesNotContain("*", result);
    }

    [Fact]
    public void SanitizeFolderNameQuestionMarkReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game?name");
        Assert.DoesNotContain("?", result);
    }

    [Fact]
    public void SanitizeFolderNamePipeReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game|name");
        Assert.DoesNotContain("|", result);
    }

    [Fact]
    public void SanitizeFolderNameAngleBracketsReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game<name>");
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
    }

    [Fact]
    public void SanitizeFolderNameQuotesReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game\"name");
        Assert.DoesNotContain("\"", result);
    }

    [Fact]
    public void ContainsInvalidCharactersTabCharacterReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("game\tname", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\t', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersNewlineReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("game\nname", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\n', invalidChars);
    }

    [Fact]
    public void ContainsInvalidPathCharactersValidUncPathReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters(@"\\server\share\folder", out _);
        Assert.False(result);
    }

    [Fact]
    public void SanitizeFolderNamePreservesDashesAndUnderscores()
    {
        var result = _sanitizer.SanitizeFolderName("my-game_v2");
        Assert.Equal("my-game_v2", result);
    }

    [Fact]
    public void SanitizeFolderNamePreservesSpaces()
    {
        var result = _sanitizer.SanitizeFolderName("Super Nintendo");
        Assert.Equal("Super Nintendo", result);
    }

    [Fact]
    public void SanitizeFolderNamePreservesParentheses()
    {
        var result = _sanitizer.SanitizeFolderName("game (USA)");
        Assert.Equal("game (USA)", result);
    }

    [Fact]
    public void SanitizeFolderNamePreservesBrackets()
    {
        var result = _sanitizer.SanitizeFolderName("game [v1.0]");
        Assert.Equal("game [v1.0]", result);
    }

    [Fact]
    public void ContainsInvalidCharactersMultipleInvalidCharsReturnsAll()
    {
        var result = _sanitizer.ContainsInvalidCharacters("a/b\\c*d", out var invalidChars);
        Assert.True(result);
        Assert.Contains('/', invalidChars);
        Assert.Contains('\\', invalidChars);
        Assert.Contains('*', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersDuplicateInvalidCharsReturnsAll()
    {
        var result = _sanitizer.ContainsInvalidCharacters("a/b/c", out var invalidChars);
        Assert.True(result);
        // Should contain '/' (may have duplicates depending on implementation)
        Assert.Contains('/', invalidChars);
    }
}
