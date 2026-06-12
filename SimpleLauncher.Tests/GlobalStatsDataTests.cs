using SimpleLauncher.Services.GlobalStats.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="GlobalStatsData"/> model.
/// </summary>
public class GlobalStatsDataTests
{
    /// <summary>
    /// Verifies that a new GlobalStatsData can be created with init-only properties.
    /// </summary>
    [Fact]
    public void CanCreateWithInitProperties()
    {
        var data = new GlobalStatsData
        {
            TotalSystems = 10,
            TotalEmulators = 5,
            TotalGames = 100,
            TotalImages = 80,
            TotalDiskSize = 1024L * 1024 * 100,
            TotalSystemsWithMissingImages = 2
        };

        Assert.Equal(10, data.TotalSystems);
        Assert.Equal(5, data.TotalEmulators);
        Assert.Equal(100, data.TotalGames);
        Assert.Equal(80, data.TotalImages);
        Assert.Equal(1024L * 1024 * 100, data.TotalDiskSize);
        Assert.Equal(2, data.TotalSystemsWithMissingImages);
    }

    /// <summary>
    /// Verifies that default values are zero for all properties.
    /// </summary>
    [Fact]
    public void DefaultValuesAreZero()
    {
        var data = new GlobalStatsData();

        Assert.Equal(0, data.TotalSystems);
        Assert.Equal(0, data.TotalEmulators);
        Assert.Equal(0, data.TotalGames);
        Assert.Equal(0, data.TotalImages);
        Assert.Equal(0L, data.TotalDiskSize);
        Assert.Equal(0, data.TotalSystemsWithMissingImages);
    }

    /// <summary>
    /// Verifies that TotalDiskSize can hold large values.
    /// </summary>
    [Fact]
    public void TotalDiskSizeSupportsLargeValues()
    {
        var data = new GlobalStatsData { TotalDiskSize = long.MaxValue };
        Assert.Equal(long.MaxValue, data.TotalDiskSize);
    }

    /// <summary>
    /// Verifies that properties can be set independently.
    /// </summary>
    [Fact]
    public void PropertiesCanBeSetIndependently()
    {
        var data = new GlobalStatsData { TotalSystems = 42 };
        Assert.Equal(42, data.TotalSystems);
        Assert.Equal(0, data.TotalEmulators);
        Assert.Equal(0, data.TotalGames);
    }

    /// <summary>
    /// Verifies that GlobalStatsData is a sealed class.
    /// </summary>
    [Fact]
    public void ClassIsSealed()
    {
        Assert.True(typeof(GlobalStatsData).IsSealed);
    }
}
