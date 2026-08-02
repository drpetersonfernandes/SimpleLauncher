using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="LaunchContext"/> model class covering default values, property assignment, and independence of instances.
/// </summary>
public class LaunchContextTests
{
    /// <summary>
    /// Verifies that all properties on a new LaunchContext default to empty strings or null.
    /// </summary>
    [Fact]
    public void DefaultPropertiesAreEmptyStrings()
    {
        var context = new LaunchContext();

        Assert.Equal("", context.FilePath);
        Assert.Equal("", context.ResolvedFilePath);
        Assert.Equal("", context.EmulatorName);
        Assert.Equal("", context.SystemName);
        Assert.Equal("", context.Parameters);
        Assert.Null(context.SystemManagerService);
        Assert.Null(context.EmulatorManager);
        Assert.Null(context.Settings);
        Assert.Null(context.WindowContext);
        Assert.Null(context.LoadingState);
    }

    /// <summary>
    /// Verifies that LaunchContext properties can be set and retrieved correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that an empty FilePath is preserved as-is.
    /// </summary>
    [Fact]
    public void EmptyFilePathReturnsEmpty()
    {
        var context = new LaunchContext { FilePath = "" };
        Assert.Equal("", context.FilePath);
    }

    /// <summary>
    /// Verifies that Parameters containing quotes are preserved correctly.
    /// </summary>
    [Fact]
    public void ParametersWithQuotesIsPreserved()
    {
        var context = new LaunchContext
        {
            Parameters = "-L \"C:\\cores\\nestopia_libretro.dll\""
        };

        Assert.Contains("nestopia_libretro.dll", context.Parameters, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that multiple LaunchContext instances are independent of each other.
    /// </summary>
    [Fact]
    public void MultipleInstancesAreIndependent()
    {
        var c1 = new LaunchContext { FilePath = "a" };
        var c2 = new LaunchContext { FilePath = "b" };

        Assert.NotEqual(c1.FilePath, c2.FilePath, StringComparer.Ordinal);
    }
}
