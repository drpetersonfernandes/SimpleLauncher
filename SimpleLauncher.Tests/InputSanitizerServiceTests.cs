using SimpleLauncher.Services.SanitizeInputString;
using Xunit;

namespace SimpleLauncher.Tests;

public class InputSanitizerServiceTests
{
    private readonly InputSanitizerService _sanitizer = new();

    [Fact]
    public void ContainsInvalidCharactersValidNameReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters("NES", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithBackslashReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("NES\\evil", out var invalidChars);
        Assert.True(result);
        Assert.Contains('\\', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWithSlashReturnsTrue()
    {
        var result = _sanitizer.ContainsInvalidCharacters("NES/evil", out var invalidChars);
        Assert.True(result);
        Assert.Contains('/', invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersEmptyReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersNullReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters(null, out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidCharactersWhitespaceReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidCharacters("   ", out _);
        Assert.False(result);
    }

    [Fact]
    public void ContainsInvalidPathCharactersValidPathReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters(@"C:\roms\NES", out _);
        Assert.False(result);
    }

    [Fact]
    public void ContainsInvalidPathCharactersEmptyReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters("", out var invalidChars);
        Assert.False(result);
        Assert.Empty(invalidChars);
    }

    [Fact]
    public void ContainsInvalidPathCharactersNullReturnsFalse()
    {
        var result = _sanitizer.ContainsInvalidPathCharacters(null, out _);
        Assert.False(result);
    }

    [Fact]
    public void SanitizeFolderNameValidNameReturnsUnchanged()
    {
        var result = _sanitizer.SanitizeFolderName("NES");
        Assert.Equal("NES", result);
    }

    [Fact]
    public void SanitizeFolderNameWithSpacesReturnsUnchanged()
    {
        var result = _sanitizer.SanitizeFolderName("Super Nintendo");
        Assert.Equal("Super Nintendo", result);
    }

    [Fact]
    public void SanitizeFolderNameEmptyReturnsPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName("");
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameNullReturnsPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName(null);
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameWhitespaceReturnsPlaceholder()
    {
        var result = _sanitizer.SanitizeFolderName("   ");
        Assert.Equal("_invalid_empty_name_", result);
    }

    [Fact]
    public void SanitizeFolderNameDoubleDotsReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("NES..SNES");
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedConEscaped()
    {
        var result = _sanitizer.SanitizeFolderName("CON");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedPrnEscaped()
    {
        var result = _sanitizer.SanitizeFolderName("PRN");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedNulEscaped()
    {
        var result = _sanitizer.SanitizeFolderName("NUL");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameReservedCaseInsensitive()
    {
        var result = _sanitizer.SanitizeFolderName("con");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameCom1Reserved()
    {
        var result = _sanitizer.SanitizeFolderName("COM1");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameLpt1Reserved()
    {
        var result = _sanitizer.SanitizeFolderName("LPT1");
        Assert.StartsWith("_", result);
        Assert.EndsWith("_", result);
    }

    [Fact]
    public void SanitizeFolderNameInvalidCharsReplaced()
    {
        var result = _sanitizer.SanitizeFolderName("NES/SNES");
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void SanitizeFolderNameTrimsDotsAndSpaces()
    {
        var result = _sanitizer.SanitizeFolderName(" NES..");
        Assert.Equal("NES", result);
    }

    [Fact]
    public void SanitizeFolderNameTraversalAttack()
    {
        var result = _sanitizer.SanitizeFolderName("../../../etc/passwd");
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void SanitizeFolderNamePreservesValidCharacters()
    {
        var result = _sanitizer.SanitizeFolderName("NES-GBA_v2.0");
        Assert.Equal("NES-GBA_v2.0", result);
    }
}
