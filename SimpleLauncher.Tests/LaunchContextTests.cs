using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class LaunchContextTests
{
    [Fact]
    public void DefaultPropertiesAreEmptyStrings()
    {
        var context = new LaunchContext();

        Assert.Equal("", context.FilePath);
        Assert.Equal("", context.ResolvedFilePath);
        Assert.Equal("", context.EmulatorName);
        Assert.Equal("", context.SystemName);
        Assert.Equal("", context.Parameters);
        Assert.Null(context.SystemManager);
        Assert.Null(context.EmulatorManager);
        Assert.Null(context.Settings);
        Assert.Null(context.WindowContext);
        Assert.Null(context.LoadingState);
    }

    [Fact]
    public void PropertiesCanBeSet()
    {
        var context = new LaunchContext
        {
            FilePath = @"C:\roms\game.zip",
            ResolvedFilePath = @"C:\temp\game.nes",
            EmulatorName = "Mesen",
            SystemName = "NES",
            Parameters = "--fullscreen"
        };

        Assert.Equal(@"C:\roms\game.zip", context.FilePath);
        Assert.Equal(@"C:\temp\game.nes", context.ResolvedFilePath);
        Assert.Equal("Mesen", context.EmulatorName);
        Assert.Equal("NES", context.SystemName);
        Assert.Equal("--fullscreen", context.Parameters);
    }

    [Fact]
    public void EmptyFilePathReturnsEmpty()
    {
        var context = new LaunchContext { FilePath = "" };
        Assert.Equal("", context.FilePath);
    }

    [Fact]
    public void ParametersWithQuotesIsPreserved()
    {
        var context = new LaunchContext
        {
            Parameters = "-L \"C:\\cores\\nestopia_libretro.dll\""
        };

        Assert.Contains("nestopia_libretro.dll", context.Parameters);
    }

    [Fact]
    public void MultipleInstancesAreIndependent()
    {
        var c1 = new LaunchContext { FilePath = "a" };
        var c2 = new LaunchContext { FilePath = "b" };

        Assert.NotEqual(c1.FilePath, c2.FilePath);
    }
}
