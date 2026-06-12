using SimpleLauncher.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the FormatFileSize utility covering MB formatting and human-readable file size formatting.
/// </summary>
public class FormatFileSizeTests
{
    /// <summary>
    /// Verifies that FormatToMb returns the expected MB string for various byte values.
    /// </summary>
    [Theory]
    [InlineData(0, "0.00 MB")]
    [InlineData(1024 * 1024, "1.00 MB")]
    [InlineData(2 * 1024 * 1024, "2.00 MB")]
    [InlineData(1536 * 1024, "1.50 MB")]
    [InlineData(1048576 * 5, "5.00 MB")]
    public void FormatToMbReturnsExpected(long bytes, string expected)
    {
        var result = FormatFileSize.FormatToMb(bytes);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that FormatToHumanReadable returns the expected string with appropriate unit suffixes.
    /// </summary>
    [Theory]
    [InlineData(0, "0.00 B")]
    [InlineData(512, "512.00 B")]
    [InlineData(1024, "1.00 KB")]
    [InlineData(1025, "1.00 KB")]
    [InlineData(1536, "1.50 KB")]
    [InlineData(1024 * 1024, "1.00 MB")]
    [InlineData(1024 * 1024 + 1, "1.00 MB")]
    [InlineData(1536 * 1024, "1.50 MB")]
    [InlineData(1024L * 1024 * 1024, "1.00 GB")]
    [InlineData(1024L * 1024 * 1024 + 1, "1.00 GB")]
    [InlineData(1024L * 1024 * 1024 * 1024, "1.00 TB")]
    [InlineData(1024L * 1024 * 1024 * 1024 + 1, "1.00 TB")]
    public void FormatToHumanReadableReturnsExpected(long bytes, string expected)
    {
        var result = FormatFileSize.FormatToHumanReadable(bytes);
        Assert.Equal(expected, result);
    }
}
