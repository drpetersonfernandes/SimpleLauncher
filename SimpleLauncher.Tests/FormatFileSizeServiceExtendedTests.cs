using SimpleLauncher.Core.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Extended tests for <see cref="FormatFileSizeService" /> covering boundary values,
///     fractional sizes, and culture-invariant formatting.
/// </summary>
public class FormatFileSizeServiceExtendedTests
{
    private readonly FormatFileSizeService _service = new();

    /// <summary>
    ///     Verifies that FormatToMb returns a negative MB value for negative byte input.
    /// </summary>
    [Fact]
    public void FormatToMbNegativeBytesReturnsNegativeMb()
    {
        var result = _service.FormatToMb(-1024L * 1024);
        Assert.Equal("-1.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb returns near-zero MB for a single byte.
    /// </summary>
    [Fact]
    public void FormatToMbOneByteReturnsNearZero()
    {
        var result = _service.FormatToMb(1);
        Assert.Equal("0.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb returns 0.00 MB for exactly one kilobyte.
    /// </summary>
    [Fact]
    public void FormatToMbExactlyOneKb()
    {
        var result = _service.FormatToMb(1024);
        Assert.Equal("0.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb correctly formats a very large value (5 GB).
    /// </summary>
    [Fact]
    public void FormatToMbVeryLargeValue()
    {
        var result = _service.FormatToMb(1024L * 1024 * 1024 * 5); // 5 GB
        Assert.Equal("5120.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns negative bytes for negative input.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableNegativeBytesReturnsNegativeB()
    {
        var result = _service.FormatToHumanReadable(-1);
        Assert.Equal("-1.00 B", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns bytes for exactly 1023 bytes.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableExactlyOneKbBoundary()
    {
        var result = _service.FormatToHumanReadable(1023);
        Assert.Equal("1023.00 B", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns KB for just over 1 KB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableJustOverOneKb()
    {
        var result = _service.FormatToHumanReadable(1025);
        Assert.Equal("1.00 KB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns KB for just under 1 MB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableExactlyOneMbBoundary()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 - 1);
        Assert.EndsWith("KB", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns MB for just under 1 GB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableExactlyOneGbBoundary()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 - 1);
        Assert.EndsWith("MB", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns GB for just under 1 TB.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableExactlyOneTbBoundary()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 * 1024 - 1);
        Assert.EndsWith("GB", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable correctly handles fractional KB values.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableFractionalKb()
    {
        var result = _service.FormatToHumanReadable(1536); // 1.5 KB
        Assert.Equal("1.50 KB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable correctly handles fractional MB values.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableFractionalMb()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 + 512L * 1024); // 1.5 MB
        Assert.Equal("1.50 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable correctly handles fractional GB values.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableFractionalGb()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 + 512L * 1024 * 1024); // 1.5 GB
        Assert.Equal("1.50 GB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable correctly formats a very large TB value.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableVeryLargeTb()
    {
        var result = _service.FormatToHumanReadable(1024L * 1024 * 1024 * 1024 * 100); // 100 TB
        Assert.Equal("100.00 TB", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb returns 0.00 MB for a single byte (decimal precision test).
    /// </summary>
    [Fact]
    public void FormatToMbDecimalPrecision()
    {
        // 1 byte = ~0.000001 MB, should show 0.00
        var result = _service.FormatToMb(1);
        Assert.Equal("0.00 MB", result);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable returns "1.00 B" for a single byte.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableOneByte()
    {
        var result = _service.FormatToHumanReadable(1);
        Assert.Equal("1.00 B", result);
    }

    /// <summary>
    ///     Verifies that FormatToMb uses invariant culture with a period decimal separator.
    /// </summary>
    [Fact]
    public void FormatToMbInvariantCultureDecimalPoint()
    {
        // Ensure decimal point is '.' not ','
        var result = _service.FormatToMb(1536L * 1024);
        Assert.Contains(".", result, StringComparison.Ordinal);
        Assert.DoesNotContain(",", result, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that FormatToHumanReadable uses invariant culture with a period decimal separator.
    /// </summary>
    [Fact]
    public void FormatToHumanReadableInvariantCultureDecimalPoint()
    {
        var result = _service.FormatToHumanReadable(1536);
        Assert.Contains(".", result, StringComparison.Ordinal);
        Assert.DoesNotContain(",", result, StringComparison.Ordinal);
    }
}