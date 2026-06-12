using SimpleLauncher.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Provides extended test coverage for SanitizeInputSystemName, covering edge cases for invalid character detection, path character validation, and folder name sanitization.
/// </summary>
public class SanitizeInputSystemNameExtendedTests
{
    // ContainsInvalidCharacters Tests

    /// <summary>
    /// Verifies that a simple valid name like NES returns false for invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersValidNameReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that names containing spaces are not flagged as invalid.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithSpacesReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("Super Nintendo", out _);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that names containing dots are not flagged as invalid.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithDotsReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("N.E.S.", out _);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that a backslash in a system name is detected as an invalid character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithBackslashReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES\\SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\\', invalidChars);
    }

    /// <summary>
    /// Verifies that a forward slash in a system name is detected as an invalid character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithSlashReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES/SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('/', invalidChars);
    }

    /// <summary>
    /// Verifies that a colon in a system name is detected as an invalid character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithColonReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES:SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains(':', invalidChars);
    }

    /// <summary>
    /// Verifies that an asterisk in a system name is detected as an invalid character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithAsteriskReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES*SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('*', invalidChars);
    }

    /// <summary>
    /// Verifies that a question mark in a system name is detected as an invalid character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithQuestionMarkReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES?SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('?', invalidChars);
    }

    /// <summary>
    /// Verifies that angle brackets in a system name are detected as invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithAngleBracketsReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES<SNES", out _);
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a pipe character in a system name is detected as an invalid character.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWithPipeReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES|SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('|', invalidChars);
    }

    /// <summary>
    /// Verifies that an empty string returns false for invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersEmptyReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that a null input returns false for invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersNullReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters(null, out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that a whitespace-only string returns false for invalid characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidCharactersWhitespaceReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("   ", out _);
        Assert.False(result);
    }

    // ContainsInvalidPathCharacters Tests

    /// <summary>
    /// Verifies that a valid Windows path does not contain invalid path characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersValidPathReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidPathCharacters(@"C:\roms\NES", out _);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that an empty string returns false for invalid path characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersEmptyReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidPathCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    /// <summary>
    /// Verifies that a null input returns false for invalid path characters.
    /// </summary>
    [Fact]
    public void ContainsInvalidPathCharactersNullReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidPathCharacters(null, out _);
        Assert.False(result);
    }

    // SanitizeFolderName Tests

    /// <summary>
    /// Verifies that a valid folder name like NES is returned unchanged.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameValidNameReturnsUnchanged()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES");
        Assert.Equal("NES", result);
    }

    /// <summary>
    /// Verifies that a name with spaces is returned unchanged.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWithSpacesReturnsUnchanged()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("Super Nintendo");
        Assert.Equal("Super Nintendo", result);
    }

    /// <summary>
    /// Verifies that double dots in a folder name are replaced to prevent path traversal.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWithDoubleDotsReplacesTraversal()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES..SNES");
        Assert.DoesNotContain("..", result);
    }

    /// <summary>
    /// Verifies that an empty string returns a placeholder name.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameEmptyReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("");
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that null input returns a placeholder name.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameNullReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(null);
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that whitespace-only input returns a placeholder name.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWhitespaceReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("   ");
        Assert.Equal("_invalid_empty_name_", result);
    }

    /// <summary>
    /// Verifies that a single dot returns a sanitized placeholder name.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameJustDotsReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(".");
        Assert.Equal("_invalid_sanitized_name_", result);
    }

    /// <summary>
    /// Verifies that double dots return an underscore placeholder.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameDoubleDotsReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("..");
        Assert.Equal("_", result);
    }

    /// <summary>
    /// Verifies that the Windows reserved name CON is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedCon()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("CON");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that the Windows reserved name PRN is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedPrn()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("PRN");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that the Windows reserved name AUX is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedAux()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("AUX");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that the Windows reserved name NUL is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedNul()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NUL");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that the Windows reserved name COM1 is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedCom1()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("COM1");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that the Windows reserved name LPT1 is escaped with underscores.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedLpt1()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("LPT1");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that Windows reserved names are detected case-insensitively.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameReservedCaseInsensitive()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("con");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    /// <summary>
    /// Verifies that invalid characters like slashes are removed from folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWithInvalidChars()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES/SNES");
        Assert.DoesNotContain("/", result);
    }

    /// <summary>
    /// Verifies that leading and trailing dots and spaces are trimmed from folder names.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameTrimsDotsAndSpaces()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(" NES ");
        Assert.Equal("NES", result);
    }

    /// <summary>
    /// Verifies that a directory traversal attack path is sanitized to remove traversal sequences.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameTraversalAttack()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("../../../etc/passwd");
        Assert.DoesNotContain("..", result);
    }

    /// <summary>
    /// Verifies that all invalid filename characters are replaced when multiple are present.
    /// </summary>
    [Fact]
    public void SanitizeFolderNameWithMultipleInvalidChars()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES<>:\"/\\|?*SNES");
        // All invalid filename chars should be replaced
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

    /// <summary>
    /// Verifies that valid characters such as hyphens, underscores, and dots are preserved.
    /// </summary>
    [Fact]
    public void SanitizeFolderNamePreservesValidCharacters()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES-GBA_v2.0");
        Assert.Equal("NES-GBA_v2.0", result);
    }
}
