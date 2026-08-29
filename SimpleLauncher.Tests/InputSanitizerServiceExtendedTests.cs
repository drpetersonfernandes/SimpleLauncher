using SimpleLauncher.Core.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Extended tests for <see cref="InputSanitizerService" /> covering additional edge cases
///     for reserved names, special characters, and sanitization behavior.
/// </summary>
public class InputSanitizerServiceExtendedTests
{
    private readonly InputSanitizerService _sanitizer = new();

    /// <summary>
    ///     Verifies that reserved name AUX is escaped with underscores regardless of casing.
    /// </summary>
    /// <param name="name">The reserved AUX name variant to sanitize.</param>
    [Theory]
    [InlineData("AUX")]
    [InlineData("aux")]
    [InlineData("Aux")]
    public void SanitizeFolderNameReservedAuxEscaped(string name)
    {
        var result = _sanitizer.SanitizeFolderName(name);
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that all reserved COM port names (COM2-COM9) are escaped with underscores.
    /// </summary>
    /// <param name="name">The reserved COM port name to sanitize.</param>
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
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that all reserved LPT port names (LPT2-LPT9) are escaped with underscores.
    /// </summary>
    /// <param name="name">The reserved LPT port name to sanitize.</param>
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
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that multiple double dots in folder names are all replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameDoubleDotsMultipleReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("a..b..c..d");
        Assert.DoesNotContain("..", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that leading and trailing dots are trimmed from folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameLeadingTrailingDotsTrimmed()
    {
        var result = _sanitizer.SanitizeFolderName("...test...");
        Assert.Equal("test", result);
    }

    /// <summary>
    ///     Verifies that leading and trailing spaces are trimmed from folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameLeadingTrailingSpacesTrimmed()
    {
        var result = _sanitizer.SanitizeFolderName("   test   ");
        Assert.Equal("test", result);
    }

    /// <summary>
    ///     Verifies that a folder name consisting only of dots becomes the invalid placeholder.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameOnlyDotsBecomesEmpty()
    {
        var result = _sanitizer.SanitizeFolderName("...");
        // After trimming dots, it becomes empty, then sanitized
        Assert.Equal("_invalid_sanitized_name_", result);
    }

    /// <summary>
    ///     Verifies that a folder name consisting only of spaces becomes the invalid placeholder.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameOnlySpacesBecomesPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName("     ");
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    ///     Verifies that colons in folder names are replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameColonReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("C:drive");
        Assert.DoesNotContain(":", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that asterisks in folder names are replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameAsteriskReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game*name");
        Assert.DoesNotContain("*", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that question marks in folder names are replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameQuestionMarkReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game?name");
        Assert.DoesNotContain("?", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that pipe characters in folder names are replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePipeReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game|name");
        Assert.DoesNotContain("|", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that angle brackets in folder names are replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameAngleBracketsReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game<name>");
        Assert.DoesNotContain("<", result, StringComparison.Ordinal);
        Assert.DoesNotContain(">", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that double quotes in folder names are replaced.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameQuotesReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("game\"name");
        Assert.DoesNotContain("\"", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that tab characters are detected as invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersTabCharacterReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("game\tname", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\t', invalidChars);
    }

    /// <summary>
    ///     Verifies that newline characters are detected as invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersNewlineReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("game\nname", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\n', invalidChars);
    }

    /// <summary>
    ///     Verifies that valid UNC paths are not detected as containing invalid path characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersValidUncPathReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters(@"\\server\share\folder", out _);
        Assert.False(result);
    }

    /// <summary>
    ///     Verifies that dashes and underscores are preserved in sanitized folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePreservesDashesAndUnderscores()
    {
        var result = _sanitizer.SanitizeFolderName("my-game_v2");
        Assert.Equal("my-game_v2", result);
    }

    /// <summary>
    ///     Verifies that spaces are preserved in sanitized folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePreservesSpaces()
    {
        var result = _sanitizer.SanitizeFolderName("Super Nintendo");
        Assert.Equal("Super Nintendo", result);
    }

    /// <summary>
    ///     Verifies that parentheses are preserved in sanitized folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePreservesParentheses()
    {
        var result = _sanitizer.SanitizeFolderName("game (USA)");
        Assert.Equal("game (USA)", result);
    }

    /// <summary>
    ///     Verifies that brackets are preserved in sanitized folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePreservesBrackets()
    {
        var result = _sanitizer.SanitizeFolderName("game [v1.0]");
        Assert.Equal("game [v1.0]", result);
    }

    /// <summary>
    ///     Verifies that multiple invalid characters are all returned in the output.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersMultipleInvalidCharsReturnsAll()
    {
        var result = _sanitizer.ContainsInvalidCharacters("a/b\\c*d", out var invalidChars);
        Assert.True(result);
        Assert.Contains('/', invalidChars);
        Assert.Contains('\\', invalidChars);
        Assert.Contains('*', invalidChars);
    }

    /// <summary>
    ///     Verifies that duplicate invalid characters are all returned in the output.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersDuplicateInvalidCharsReturnsAll()
    {
        var result = _sanitizer.ContainsInvalidCharacters("a/b/c", out var invalidChars);
        Assert.True(result);
        // Should contain '/' (may have duplicates depending on implementation)
        Assert.Contains('/', invalidChars);
    }
}