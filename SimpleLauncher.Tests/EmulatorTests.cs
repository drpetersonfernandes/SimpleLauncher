using SimpleLauncher.Services.SystemManager;
using Xunit;

namespace SimpleLauncher.Tests;

public class EmulatorTests
{
    [Fact]
    public void DefaultPropertiesAreNull()
    {
        var emulator = new Emulator();

        Assert.Null(emulator.EmulatorName);
        Assert.Null(emulator.EmulatorLocation);
        Assert.Null(emulator.EmulatorParameters);
        Assert.Null(emulator.ImagePackDownloadLink);
        Assert.Null(emulator.ImagePackDownloadLink2);
        Assert.Null(emulator.ImagePackDownloadLink3);
        Assert.Null(emulator.ImagePackDownloadLink4);
        Assert.Null(emulator.ImagePackDownloadLink5);
        Assert.Null(emulator.ImagePackDownloadExtractPath);
        Assert.False(emulator.ReceiveANotificationOnEmulatorError);
    }

    [Fact]
    public void InitPropertiesCanBeSet()
    {
        var emulator = new Emulator
        {
            EmulatorName = "RetroArch",
            EmulatorLocation = @"C:\emu\retroarch.exe",
            EmulatorParameters = "-L cores\\nestopia_libretro.dll",
            ReceiveANotificationOnEmulatorError = true,
            ImagePackDownloadLink = "https://example.com/pack1.zip",
            ImagePackDownloadLink2 = "https://example.com/pack2.zip",
            ImagePackDownloadLink3 = "https://example.com/pack3.zip",
            ImagePackDownloadLink4 = "https://example.com/pack4.zip",
            ImagePackDownloadLink5 = "https://example.com/pack5.zip",
            ImagePackDownloadExtractPath = @"C:\images\nes"
        };

        Assert.Equal("RetroArch", emulator.EmulatorName);
        Assert.Equal(@"C:\emu\retroarch.exe", emulator.EmulatorLocation);
        Assert.Equal("-L cores\\nestopia_libretro.dll", emulator.EmulatorParameters);
        Assert.True(emulator.ReceiveANotificationOnEmulatorError);
        Assert.Equal("https://example.com/pack1.zip", emulator.ImagePackDownloadLink);
        Assert.Equal("https://example.com/pack2.zip", emulator.ImagePackDownloadLink2);
        Assert.Equal("https://example.com/pack3.zip", emulator.ImagePackDownloadLink3);
        Assert.Equal("https://example.com/pack4.zip", emulator.ImagePackDownloadLink4);
        Assert.Equal("https://example.com/pack5.zip", emulator.ImagePackDownloadLink5);
        Assert.Equal(@"C:\images\nes", emulator.ImagePackDownloadExtractPath);
    }

    [Fact]
    public void PartialInitSetsOnlySpecifiedProperties()
    {
        var emulator = new Emulator
        {
            EmulatorName = "Mesen",
            ImagePackDownloadLink = "https://example.com/pack.zip"
        };

        Assert.Equal("Mesen", emulator.EmulatorName);
        Assert.Equal("https://example.com/pack.zip", emulator.ImagePackDownloadLink);
        Assert.Null(emulator.EmulatorLocation);
        Assert.Null(emulator.ImagePackDownloadLink2);
    }
}
