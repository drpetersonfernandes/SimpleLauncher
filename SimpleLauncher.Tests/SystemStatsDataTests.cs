using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="SystemStatsData"/> model covering default values, property assignment,
/// and the AreFilesAndImagesEqual computed property.
/// </summary>
public class SystemStatsDataTests
{
    /// <summary>
    /// Verifies that all default property values of a new SystemStatsData are correct.
    /// </summary>
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

    /// <summary>
    /// Verifies that init-only properties can be set during object initialization.
    /// </summary>
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

    /// <summary>
    /// Verifies that AreFilesAndImagesEqual returns the expected result for various file and image counts.
    /// </summary>
    /// <param name="files">The number of files.</param>
    /// <param name="images">The number of images.</param>
    /// <param name="expected">Whether files and images are expected to be equal.</param>
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

    /// <summary>
    /// Verifies that TotalDiskSize can be set to zero.
    /// </summary>
    [Fact]
    public void TotalDiskSizeCanBeZero()
    {
        var data = new SystemStatsData { TotalDiskSize = 0L };
        Assert.Equal(0L, data.TotalDiskSize);
    }

    /// <summary>
    /// Verifies that TotalDiskSize can be set to a very large value.
    /// </summary>
    [Fact]
    public void TotalDiskSizeCanBeLargeValue()
    {
        var data = new SystemStatsData { TotalDiskSize = long.MaxValue };
        Assert.Equal(long.MaxValue, data.TotalDiskSize);
    }
}
