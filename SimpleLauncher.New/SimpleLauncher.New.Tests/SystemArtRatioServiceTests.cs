using SimpleLauncher.New.Services;

namespace SimpleLauncher.New.Tests;

public class SystemArtRatioServiceTests
{
    private readonly SystemArtRatioService _service = new();

    [Fact]
    public void GetRatio_KnownSystem_ReturnsCorrectRatio()
    {
        Assert.Equal(0.75, _service.GetRatio("NES"));
        Assert.Equal(0.75, _service.GetRatio("Atari 2600"));
        Assert.Equal(1.0, _service.GetRatio("SNES"));
        Assert.Equal(0.70, _service.GetRatio("PS1"));
        Assert.Equal(0.70, _service.GetRatio("Sega Saturn"));
        Assert.Equal(0.80, _service.GetRatio("Sega Genesis"));
        Assert.Equal(1.41, _service.GetRatio("Arcade"));
    }

    [Fact]
    public void GetRatio_UnknownSystem_ReturnsDefault()
    {
        Assert.Equal(1.0, _service.GetRatio("UnknownConsole"));
        Assert.Equal(1.0, _service.GetRatio(""));
        Assert.Equal(1.0, _service.GetRatio(null!));
    }

    [Fact]
    public void GetRatio_CaseInsensitive()
    {
        Assert.Equal(0.75, _service.GetRatio("nes"));
        Assert.Equal(0.75, _service.GetRatio("Nes"));
        Assert.Equal(0.70, _service.GetRatio("sega saturn"));
    }

    [Fact]
    public void GetArtHeight_CalculatesCorrectly()
    {
        var height = _service.GetArtHeight(200, "NES");
        Assert.Equal(150, height); // 200 * 0.75

        var squareHeight = _service.GetArtHeight(200, "SNES");
        Assert.Equal(200, squareHeight); // 200 * 1.0

        var arcadeHeight = _service.GetArtHeight(200, "Arcade");
        Assert.Equal(282, arcadeHeight); // 200 * 1.41
    }

    [Fact]
    public void GetArtHeight_MixedView_UsesUniformRatio()
    {
        var height = _service.GetArtHeight(200, "NES", true);
        Assert.Equal(146, height); // 200 * 0.73

        var height2 = _service.GetArtHeight(200, "Arcade", true);
        Assert.Equal(146, height2); // Same uniform ratio regardless of system
    }
}
