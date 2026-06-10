using SimpleLauncher.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

public class SanitizeInputSystemNameExtendedTests
{
    // ContainsInvalidCharacters Tests

    [Fact]
    public void ContainsInvalidCharactersValidNameReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithSpacesReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("Super Nintendo", out _);
        Assert.False(result);
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
    public void ContainsInvalidCharactersWithAngleBracketsReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES<SNES", out _);
        Assert.True(result);
    }

    [Fact]
    public void ContainsInvalidCharactersWithPipeReturnsTrue()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("NES|SNES", out var invalidChars);
        Assert.True(result);
        Assert.Contains('|', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersEmptyReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersNullReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters(null, out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWhitespaceReturnsFalse()
    {
        var result = SanitizeInputSystemName.ContainsInvalidCharacters("   ", out _);
        Assert.False(result);
    }

    // ContainsInvalidPathCharacters Tests

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

    // SanitizeFolderName Tests

    [Fact]
    public void SanitizeFolderNameValidNameReturnsUnchanged()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES");
        Assert.Equal("NES", result);
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
    public void SanitizeFolderNameEmptyReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("");
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameNullReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(null);
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameWhitespaceReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("   ");
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameJustDotsReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName(".");
        Assert.Equal("_invalid_sanitized_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameDoubleDotsReturnsPlaceholder()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("..");
        Assert.Equal("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedCon()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("CON");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedPrn()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("PRN");
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
    public void SanitizeFolderNameTraversalAttack()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("../../../etc/passwd");
        Assert.DoesNotContain("..", result);
    }

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

    [Fact]
    public void SanitizeFolderNamePreservesValidCharacters()
    {
        var result = SanitizeInputSystemName.SanitizeFolderName("NES-GBA_v2.0");
        Assert.Equal("NES-GBA_v2.0", result);
    }
}