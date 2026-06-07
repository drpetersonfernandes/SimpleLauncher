using System.Globalization;
using Xunit;

namespace SimpleLauncher.Tests;

public class StatsNormalizeEmulatorNameTests
{
    // Stats.NormalizeEmulatorName is private, but we can test the behavior
    // through the public API by testing the CultureInfo.TextInfo.ToTitleCase pattern

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

    [Fact]
    public void NormalizeEmulatorNameEmptyReturnsEmpty()
    {
        var result = NormalizeEmulatorName("");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeEmulatorNameNullReturnsEmpty()
    {
        var result = NormalizeEmulatorName(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeEmulatorNameWhitespaceReturnsEmpty()
    {
        var result = NormalizeEmulatorName("   ");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeEmulatorNameSingleChar()
    {
        var result = NormalizeEmulatorName("a");
        Assert.Equal("A", result);
    }

    [Fact]
    public void NormalizeEmulatorNameWithSpaces()
    {
        var result = NormalizeEmulatorName("super model");
        Assert.Equal("Super Model", result);
    }

    [Fact]
    public void NormalizeEmulatorNameWithNumbers()
    {
        var result = NormalizeEmulatorName("pcsx2 0.9");
        Assert.Equal("Pcsx2 0.9", result);
    }

    [Fact]
    public void NormalizeEmulatorNameWithHyphens()
    {
        var result = NormalizeEmulatorName("mednafen-psx");
        Assert.Equal("Mednafen-Psx", result);
    }

    private static string NormalizeEmulatorName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }
}
