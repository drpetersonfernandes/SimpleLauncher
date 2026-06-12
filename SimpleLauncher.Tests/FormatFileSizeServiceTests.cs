using SimpleLauncher.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="FormatFileSizeService"/> class.
/// </summary>
public class FormatFileSizeServiceTests
{
    private readonly FormatFileSizeService _service = new();

    [Fact]
    public void FormatToMbZeroBytesReturnsZeroMb()
    {
        var result = _service.FormatToMb(0);
        Assert.Equal("0.00 MB", result);
    }

    [Fact]
    public void FormatToMbOneMbReturnsCorrectFormat()
    {
        var result = _service.FormatToMb(1024L * 1024);
        Assert.Equal("1.00 MB", result);
    }

    [Fact]
    public void FormatToMbHalfMbReturnsCorrectFormat()
    {
        var result = _service.FormatToMb(512L * 1024);
        Assert.Equal("0.50 MB", result);
    }

    [Fact]
    public void FormatToMbLargeValueReturnsCorrectFormat()
    {
        var result = _service.FormatToMb(1024L * 1024 * 100);
        Assert.Equal("100.00 MB", result);
    }

    [Theory]
    [InlineData(0, "0.00 B")]
    [InlineData(1, "1.00 B")]
    [InlineData(1023, "1023.00 B")]
    [InlineData(1024, "1.00 KB")]
    [InlineData(1024L * 1024, "1.00 MB")]
    [InlineData(1024L * 1024 * 1024, "1.00 GB")]
    [InlineData(1024L * 1024 * 1024 * 1024, "1.00 TB")]
    public void FormatToHumanReadableReturnsCorrectUnit(long bytes, string expected)
    {
        var result = _service.FormatToHumanReadable(bytes);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatToHumanReadableZeroBytesReturnsB()
    {
        var result = _service.FormatToHumanReadable(0);
        Assert.Equal("0.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableOneKbReturnsCorrectFormat()
    {
        var result = _service.FormatToHumanReadable(1024);
        Assert.Equal("1.00 KB", result);
    }

    [Fact]
    public void FormatToHumanReadableOneGbReturnsCorrectFormat()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024);
        Assert.Equal("1.00 GB", result);
    }

    [Fact]
    public void FormatToHumanReadableOneTbReturnsCorrectFormat()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 * 1024);
        Assert.Equal("1.00 TB", result);
    }

    [Fact]
    public void FormatToHumanReadableDoesNotExceedTb()
    {
        var result = _service.FormatToHumanReadable(long.MaxValue);
        Assert.EndsWith("TB", result);
    }

    [Fact]
    public void FormatToMbUsesInvariantCulture()
    {
        var result = _service.FormatToMb(1536L * 1024); // 1.5 MB
        Assert.Equal("1.50 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableUsesInvariantCulture()
    {
        var result = _service.FormatToHumanReadable(1536); // 1.5 KB
        Assert.Equal("1.50 KB", result);
    }
}
