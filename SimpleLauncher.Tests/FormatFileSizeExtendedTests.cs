using SimpleLauncher.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

public class FormatFileSizeExtendedTests
{
    [Fact]
    public void FormatToMbZeroBytes()
    {
        var result = FormatFileSize.FormatToMb(0);
        Assert.Equal("0.00 MB", result);
    }

    [Fact]
    public void FormatToMbOneMegabyte()
    {
        var result = FormatFileSize.FormatToMb(1024 * 1024);
        Assert.Equal("1.00 MB", result);
    }

    [Fact]
    public void FormatToMbHalfMegabyte()
    {
        var result = FormatFileSize.FormatToMb(512 * 1024);
        Assert.Equal("0.50 MB", result);
    }

    [Fact]
    public void FormatToMbOneGigabyte()
    {
        var result = FormatFileSize.FormatToMb(1024L * 1024 * 1024);
        Assert.Equal("1024.00 MB", result);
    }

    [Fact]
    public void FormatToMbLargeValue()
    {
        var result = FormatFileSize.FormatToMb(5L * 1024 * 1024 * 1024);
        Assert.Equal("5120.00 MB", result);
    }

    [Fact]
    public void FormatToMbOneByte()
    {
        var result = FormatFileSize.FormatToMb(1);
        Assert.Equal("0.00 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableZeroBytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(0);
        Assert.Equal("0.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableBytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(500);
        Assert.Equal("500.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableKilobytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024);
        Assert.Equal("1024.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableMegabytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024);
        Assert.Equal("1024.00 KB", result);
    }

    [Fact]
    public void FormatToHumanReadableGigabytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 * 1024);
        Assert.Equal("1024.00 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableTerabytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 * 1024 * 1024);
        Assert.Equal("1024.00 GB", result);
    }

    [Fact]
    public void FormatToHumanReadableOneByte()
    {
        var result = FormatFileSize.FormatToHumanReadable(1);
        Assert.Equal("1.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableFractionalKb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1536);
        Assert.Equal("1.50 KB", result);
    }

    [Fact]
    public void FormatToHumanReadableFractionalMb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 + 512 * 1024);
        Assert.Equal("1.50 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableJustUnder1Kb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1023);
        Assert.Equal("1023.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableJustUnder1Mb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 - 1);
        Assert.Contains("KB", result);
    }

    [Fact]
    public void FormatToHumanReadableJustUnder1Gb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 * 1024 - 1);
        Assert.Contains("MB", result);
    }

    [Fact]
    public void FormatToHumanReadableContainsSuffix()
    {
        var result = FormatFileSize.FormatToHumanReadable(2048);
        Assert.Contains("KB", result);
    }

    [Fact]
    public void FormatToMbContainsMbSuffix()
    {
        var result = FormatFileSize.FormatToMb(1024 * 1024);
        Assert.Contains("MB", result);
    }

    [Fact]
    public void FormatToHumanReadableLargeValue()
    {
        var result = FormatFileSize.FormatToHumanReadable(long.MaxValue);
        Assert.Contains("TB", result);
    }
}
