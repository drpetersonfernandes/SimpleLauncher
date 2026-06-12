using SimpleLauncher.Services.GlobalStats.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class SystemStatsDataTests
{
    [Fact]
    public void DefaultPropertiesAreDefaultValues()
    {
        var data = new SystemStatsData();

        Assert.Null(data.SystemName);
        Assert.Equal(0, data.NumberOfFiles);
        Assert.Equal(0, data.NumberOfImages);
        Assert.Equal(0L, data.TotalDiskSize);
        Assert.True(data.AreFilesAndImagesEqual);
    }

    [Fact]
    public void InitPropertiesCanBeSet()
    {
        var data = new SystemStatsData
        {
            SystemName = "NES",
            NumberOfFiles = 100,
            NumberOfImages = 100,
            TotalDiskSize = 500000000L
        };

        Assert.Equal("NES", data.SystemName);
        Assert.Equal(100, data.NumberOfFiles);
        Assert.Equal(100, data.NumberOfImages);
        Assert.Equal(500000000L, data.TotalDiskSize);
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(10, 5, false)]
    [InlineData(0, 0, true)]
    [InlineData(1, 0, false)]
    [InlineData(0, 1, false)]
    public void AreFilesAndImagesEqualReturnsExpected(int files, int images, bool expected)
    {
        var data = new SystemStatsData
        {
            NumberOfFiles = files,
            NumberOfImages = images
        };

        Assert.Equal(expected, data.AreFilesAndImagesEqual);
    }

    [Fact]
    public void TotalDiskSizeCanBeZero()
    {
        var data = new SystemStatsData { TotalDiskSize = 0L };
        Assert.Equal(0L, data.TotalDiskSize);
    }

    [Fact]
    public void TotalDiskSizeCanBeLargeValue()
    {
        var data = new SystemStatsData { TotalDiskSize = long.MaxValue };
        Assert.Equal(long.MaxValue, data.TotalDiskSize);
    }
}
