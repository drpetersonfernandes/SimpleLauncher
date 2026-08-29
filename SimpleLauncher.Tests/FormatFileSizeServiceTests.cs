using SimpleLauncher.Core.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for the <see cref="FormatFileSizeService" /> class.
/// </summary>
public class FormatFileSizeServiceTests
{
    private readonly FormatFileSizeService _service = new();

    /// <summary>
    ///     Verifies that FormatToMb returns "0.00 MB" for zero bytes.
    /// </summary>
    [Fact]
    public void FormatToMbZeroBytesReturnsZeroMb()
    {
        var result = _service.FormatToMb(0);
        Assert.Equal("0.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb returns "1.00 MB" for exactly 1 MB.
    /// </summary>
    [Fact]
    public void FormatToMbOneMbReturnsCorrectFormat()
    {
        var result = _service.FormatToMb(1024L * 1024);
        Assert.Equal("1.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb returns "0.50 MB" for half a MB.
    /// </summary>
    [Fact]
    public void FormatToMbHalfMbReturnsCorrectFormat()
    {
        var result = _service.FormatToMb(512L * 1024);
        Assert.Equal("0.50 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb returns "100.00 MB" for 100 MB.
    /// </summary>
    [Fact]
    public void FormatToMbLargeValueReturnsCorrectFormat()
    {
        var result = _service.FormatToMb(1024L * 1024 * 100);
        Assert.Equal("100.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns the correct unit for various byte values.
    /// </summary>
    /// <param name="bytes">The size in bytes to format.</param>
    /// <param name="expected">The expected human-readable representation.</param>
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

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns "0.00 B" for zero bytes.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableZeroBytesReturnsB()
    {
        var result = _service.FormatToHumanReadable(0);
        Assert.Equal("0.00 B", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns "1.00 KB" for 1 KB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableOneKbReturnsCorrectFormat()
    {
        var result = _service.FormatToHumanReadable(1024);
        Assert.Equal("1.00 KB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns "1.00 GB" for 1 GB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableOneGbReturnsCorrectFormat()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024);
        Assert.Equal("1.00 GB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns "1.00 TB" for 1 TB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableOneTbReturnsCorrectFormat()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 * 1024);
        Assert.Equal("1.00 TB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable does not exceed TB unit.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableDoesNotExceedTb()
    {
        var result = _service.FormatToHumanReadable(long.MaxValue);
        Assert.EndsWith("TB", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that FormatToMb uses invariant culture for decimal formatting.
    /// </summary>
    [Fact]
    public void FormatToMbUsesInvariantCulture()
    {
        var result = _service.FormatToMb(1536L * 1024); // 1.5 MB
        Assert.Equal("1.50 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable uses invariant culture for decimal formatting.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableUsesInvariantCulture()
    {
        var result = _service.FormatToHumanReadable(1536); // 1.5 KB
        Assert.Equal("1.50 KB", result);
    }
}