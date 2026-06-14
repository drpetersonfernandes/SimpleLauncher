using SimpleLauncher.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="FormatFileSizeService"/> covering boundary values,
/// fractional sizes, and culture-invariant formatting.
/// </summary>
public class FormatFileSizeServiceExtendedTests
{
    private readonly FormatFileSizeService _service = new();

    [Fact]
    public void FormatToMbNegativeBytesReturnsNegativeMb()
    {
        var result = _service.FormatToMb(-1024L * 1024);
        Assert.Equal("-1.00 MB", result);
    }

    [Fact]
    public void FormatToMbOneByteReturnsNearZero()
    {
        var result = _service.FormatToMb(1);
        Assert.Equal("0.00 MB", result);
    }

    [Fact]
    public void FormatToMbExactlyOneKb()
    {
        var result = _service.FormatToMb(1024);
        Assert.Equal("0.00 MB", result);
    }

    [Fact]
    public void FormatToMbVeryLargeValue()
    {
        var result = _service.FormatToMb(1024L * 1024 * 1024 * 5); // 5 GB
        Assert.Equal("5120.00 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableNegativeBytesReturnsNegativeB()
    {
        var result = _service.FormatToHumanReadable(-1);
        Assert.Equal("-1.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableExactlyOneKbBoundary()
    {
        var result = _service.FormatToHumanReadable(1023);
        Assert.Equal("1023.00 B", result);
    }

    [Fact]
    public void FormatToHumanReadableJustOverOneKb()
    {
        var result = _service.FormatToHumanReadable(1025);
        Assert.Equal("1.00 KB", result);
    }

    [Fact]
    public void FormatToHumanReadableExactlyOneMbBoundary()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 - 1);
        Assert.EndsWith("KB", result);
    }

    [Fact]
    public void FormatToHumanReadableExactlyOneGbBoundary()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 - 1);
        Assert.EndsWith("MB", result);
    }

    [Fact]
    public void FormatToHumanReadableExactlyOneTbBoundary()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 * 1024 - 1);
        Assert.EndsWith("GB", result);
    }

    [Fact]
    public void FormatToHumanReadableFractionalKb()
    {
        var result = _service.FormatToHumanReadable(1536); // 1.5 KB
        Assert.Equal("1.50 KB", result);
    }

    [Fact]
    public void FormatToHumanReadableFractionalMb()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 + 512L * 1024); // 1.5 MB
        Assert.Equal("1.50 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableFractionalGb()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 + 512L * 1024 * 1024); // 1.5 GB
        Assert.Equal("1.50 GB", result);
    }

    [Fact]
    public void FormatToHumanReadableVeryLargeTb()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 * 1024 * 100); // 100 TB
        Assert.Equal("100.00 TB", result);
    }

    [Fact]
    public void FormatToMbDecimalPrecision()
    {
        // 1 byte = ~0.000001 MB, should show 0.00
        var result = _service.FormatToMb(1);
        Assert.Equal("0.00 MB", result);
    }

    [Fact]
    public void FormatToHumanReadableOneByte()
    {
        var result = _service.FormatToHumanReadable(1);
        Assert.Equal("1.00 B", result);
    }

    [Fact]
    public void FormatToMbInvariantCultureDecimalPoint()
    {
        // Ensure decimal point is '.' not ','
        var result = _service.FormatToMb(1536L * 1024);
        Assert.Contains(".", result);
        Assert.DoesNotContain(",", result);
    }

    [Fact]
    public void FormatToHumanReadableInvariantCultureDecimalPoint()
    {
        var result = _service.FormatToHumanReadable(1536);
        Assert.Contains(".", result);
        Assert.DoesNotContain(",", result);
    }
}
