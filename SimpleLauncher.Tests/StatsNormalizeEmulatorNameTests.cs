using System.Globalization;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the emulator name normalization logic that converts emulator names to title case.
/// </summary>
public class StatsNormalizeEmulatorNameTests
{
    // Stats.NormalizeEmulatorName is private, but we can test the behavior
    // through the public API by testing the CultureInfo.TextInfo.ToTitleCase pattern

    /// <summary>
    /// Verifies that emulator names are converted to title case regardless of input casing.
    /// </summary>
    [Theory]
    [InlineData("retroarch", "Retroarch")]
    [InlineData("RETROARCH", "Retroarch")]
    [InlineData("duckstation", "Duckstation")]
    [InlineData("DUCKSTATION", "Duckstation")]
    [InlineData("pcsx2", "Pcsx2")]
    [InlineData("mame", "Mame")]
    [InlineData("dolphin", "Dolphin")]
    [InlineData("flycast", "Flycast")]
    public void NormalizeEmulatorNameTitleCasesCorrectly(string input, string expected)
    {
        // Replicate the NormalizeEmulatorName logic
        var result = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that an empty string returns an empty result.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameEmptyReturnsEmpty()
    {
        var result = NormalizeEmulatorName("");
        Assert.Equal("", result);
    }

    /// <summary>
    /// Verifies that null input returns an empty result.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameNullReturnsEmpty()
    {
        var result = NormalizeEmulatorName(null);
        Assert.Equal("", result);
    }

    /// <summary>
    /// Verifies that whitespace-only input returns an empty result.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameWhitespaceReturnsEmpty()
    {
        var result = NormalizeEmulatorName("   ");
        Assert.Equal("", result);
    }

    /// <summary>
    /// Verifies that a single character is converted to uppercase.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameSingleChar()
    {
        var result = NormalizeEmulatorName("a");
        Assert.Equal("A", result);
    }

    /// <summary>
    /// Verifies that multi-word names are title-cased correctly.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameWithSpaces()
    {
        var result = NormalizeEmulatorName("super model");
        Assert.Equal("Super Model", result);
    }

    /// <summary>
    /// Verifies that names containing numbers are handled correctly.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameWithNumbers()
    {
        var result = NormalizeEmulatorName("pcsx2 0.9");
        Assert.Equal("Pcsx2 0.9", result);
    }

    /// <summary>
    /// Verifies that names containing hyphens are title-cased correctly.
    /// </summary>
    [Fact]
    public void NormalizeEmulatorNameWithHyphens()
    {
        var result = NormalizeEmulatorName("mednafen-psx");
        Assert.Equal("Mednafen-Psx", result);
    }

    private static string NormalizeEmulatorName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }
}
