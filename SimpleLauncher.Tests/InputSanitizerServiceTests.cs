using SimpleLauncher.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="InputSanitizerService"/> class covering folder name sanitization and invalid character detection.
/// </summary>
public class InputSanitizerServiceTests
{
    private readonly InputSanitizerService _sanitizer = new();

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns false for a valid name.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersValidNameReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters("NES", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns true for a backslash character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithBackslashReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("NES\\evil", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\\', invalidChars);
    }

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns true for a forward slash character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithSlashReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("NES/evil", out var invalidChars);
        Assert.True(result);
        Assert.Contains('/', invalidChars);
    }

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns false for an empty string.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersEmptyReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns false for a null string.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersNullReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters(null!, out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that ContainsInvalidCharacters returns false for a whitespace-only string.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWhitespaceReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters("   ", out _);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ContainsInvalidPathCharacters returns false for a valid path.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersValidPathReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters(@"C:\roms\NES", out _);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ContainsInvalidPathCharacters returns false for an empty string.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersEmptyReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that ContainsInvalidPathCharacters returns false for a null string.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersNullReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters(null!, out _);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName returns a valid name unchanged.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameValidNameReturnsUnchanged()
    {
        var result = _sanitizer.SanitizeFolderName("NES");
        Assert.Equal("NES", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName returns a name with spaces unchanged.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWithSpacesReturnsUnchanged()
    {
        var result = _sanitizer.SanitizeFolderName("Super Nintendo");
        Assert.Equal("Super Nintendo", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName returns a placeholder for an empty string.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameEmptyReturnsPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName("");
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName returns a placeholder for a null string.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameNullReturnsPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName(null!);
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName returns a placeholder for a whitespace-only string.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWhitespaceReturnsPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName("   ");
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName replaces double dots in folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameDoubleDotsReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("NES..SNES");
        Assert.DoesNotContain("..", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the reserved name CON is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedConEscaped()
    {
        var result = _sanitizer.SanitizeFolderName("CON");
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the reserved name PRN is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedPrnEscaped()
    {
        var result = _sanitizer.SanitizeFolderName("PRN");
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the reserved name NUL is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedNulEscaped()
    {
        var result = _sanitizer.SanitizeFolderName("NUL");
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that reserved names are matched case-insensitively.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedCaseInsensitive()
    {
        var result = _sanitizer.SanitizeFolderName("con");
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the reserved COM1 port name is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameCom1Reserved()
    {
        var result = _sanitizer.SanitizeFolderName("COM1");
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the reserved LPT1 port name is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameLpt1Reserved()
    {
        var result = _sanitizer.SanitizeFolderName("LPT1");
        Assert.StartsWith("_", result, StringComparison.Ordinal);
        Assert.EndsWith("_", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that invalid characters like forward slashes are replaced in folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameInvalidCharsReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("NES/SNES");
        Assert.DoesNotContain("/", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that SanitizeFolderName trims dots and spaces from folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameTrimsDotsAndSpaces()
    {
        var result = _sanitizer.SanitizeFolderName(" NES..");
        Assert.Equal("NES", result);
    }

    /// <summary>
    /// Verifies that path traversal attacks using double dots are neutralized.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameTraversalAttack()
    {
        var result = _sanitizer.SanitizeFolderName("../../../etc/passwd");
        Assert.DoesNotContain("..", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that valid characters like dashes, underscores, and dots are preserved in folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePreservesValidCharacters()
    {
        var result = _sanitizer.SanitizeFolderName("NES-GBA_v2.0");
        Assert.Equal("NES-GBA_v2.0", result);
    }
}
