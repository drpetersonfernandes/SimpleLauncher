using SimpleLauncher.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the FormatFileSize utility covering boundary values and unit suffix detection.
/// </summary>
public class FormatFileSizeExtendedTests
{
    /// <summary>
    /// Verifies that FormatToMb returns "0.00 MB" for zero bytes.
    /// </summary>
    [Fact]
    public void FormatToMbZeroBytes()
    {
        var result = FormatFileSize.FormatToMb(0);
        Assert.Equal("0.00 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToMb returns "1.00 MB" for exactly one megabyte.
    /// </summary>
    [Fact]
    public void FormatToMbOneMegabyte()
    {
        var result = FormatFileSize.FormatToMb(1024 * 1024);
        Assert.Equal("1.00 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToMb returns "0.50 MB" for half a megabyte.
    /// </summary>
    [Fact]
    public void FormatToMbHalfMegabyte()
    {
        var result = FormatFileSize.FormatToMb(512 * 1024);
        Assert.Equal("0.50 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToMb returns "1024.00 MB" for exactly one gigabyte.
    /// </summary>
    [Fact]
    public void FormatToMbOneGigabyte()
    {
        var result = FormatFileSize.FormatToMb(1024L * 1024 * 1024);
        Assert.Equal("1024.00 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToMb handles large values (5 GB) correctly.
    /// </summary>
    [Fact]
    public void FormatToMbLargeValue()
    {
        var result = FormatFileSize.FormatToMb(5L * 1024 * 1024 * 1024);
        Assert.Equal("5120.00 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToMb returns "0.00 MB" for a single byte.
    /// </summary>
    [Fact]
    public void FormatToMbOneByte()
    {
        var result = FormatFileSize.FormatToMb(1);
        Assert.Equal("0.00 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable returns "0.00 B" for zero bytes.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableZeroBytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(0);
        Assert.Equal("0.00 B", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable returns bytes for values under 1024.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableBytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(500);
        Assert.Equal("500.00 B", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable handles the kilobyte boundary correctly.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableKilobytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024);
        Assert.Equal("1.00 KB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable handles the megabyte boundary correctly.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableMegabytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024);
        Assert.Equal("1.00 MB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable handles the gigabyte boundary correctly.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableGigabytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 * 1024);
        Assert.Equal("1.00 GB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable handles the terabyte boundary correctly.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableTerabytes()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 * 1024 * 1024);
        Assert.Equal("1.00 TB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable returns "1.00 B" for a single byte.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableOneByte()
    {
        var result = FormatFileSize.FormatToHumanReadable(1);
        Assert.Equal("1.00 B", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable returns fractional KB correctly (1.50 KB).
    /// </summary>
    [Fact]
    public void FormatToHumanReadableFractionalKb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1536);
        Assert.Equal("1.50 KB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable returns fractional MB correctly (1.50 MB).
    /// </summary>
    [Fact]
    public void FormatToHumanReadableFractionalMb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 + 512 * 1024);
        Assert.Equal("1.50 MB", result);
    }

    /// <summary>
    /// Verifies that a value just under 1 KB stays in the byte unit range.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableJustUnder1Kb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1023);
        Assert.Equal("1023.00 B", result);
    }

    /// <summary>
    /// Verifies that a value just under 1 MB uses the KB unit.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableJustUnder1Mb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 - 1);
        Assert.Contains("KB", result);
    }

    /// <summary>
    /// Verifies that a value just under 1 GB uses the MB unit.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableJustUnder1Gb()
    {
        var result = FormatFileSize.FormatToHumanReadable(1024L * 1024 * 1024 - 1);
        Assert.Contains("MB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable includes the KB suffix for kilobyte-range values.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableContainsSuffix()
    {
        var result = FormatFileSize.FormatToHumanReadable(2048);
        Assert.Contains("KB", result);
    }

    /// <summary>
    /// Verifies that FormatToMb includes the MB suffix in the output.
    /// </summary>
    [Fact]
    public void FormatToMbContainsMbSuffix()
    {
        var result = FormatFileSize.FormatToMb(1024 * 1024);
        Assert.Contains("MB", result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable handles very large values (long.MaxValue) using TB units.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableLargeValue()
    {
        var result = FormatFileSize.FormatToHumanReadable(long.MaxValue);
        Assert.Contains("TB", result);
    }
}
