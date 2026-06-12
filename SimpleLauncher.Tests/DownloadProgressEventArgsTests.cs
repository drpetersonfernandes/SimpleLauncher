using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="DownloadProgressEventArgs"/> model.
/// </summary>
public class DownloadProgressEventArgsTests
{
    /// <summary>
    /// Verifies that a new DownloadProgressEventArgs has correct default values.
    /// </summary>
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var args = new DownloadProgressEventArgs();

        Assert.Equal(0L, args.BytesReceived);
        Assert.Null(args.TotalBytesToReceive);
        Assert.Equal(0.0, args.ProgressPercentage);
        Assert.Equal("", args.StatusMessage);
    }

    /// <summary>
    /// Verifies that BytesReceived can be set and retrieved.
    /// </summary>
    [Fact]
    public void BytesReceivedCanBeSet()
    {
        var args = new DownloadProgressEventArgs { BytesReceived = 1024 };
        Assert.Equal(1024L, args.BytesReceived);
    }

    /// <summary>
    /// Verifies that TotalBytesToReceive can be set and retrieved.
    /// </summary>
    [Fact]
    public void TotalBytesToReceiveCanBeSet()
    {
        var args = new DownloadProgressEventArgs { TotalBytesToReceive = 2048L };
        Assert.Equal(2048L, args.TotalBytesToReceive);
    }

    /// <summary>
    /// Verifies that TotalBytesToReceive can be set to null.
    /// </summary>
    [Fact]
    public void TotalBytesToReceiveCanBeSetToNull()
    {
        var args = new DownloadProgressEventArgs
        {
            TotalBytesToReceive = 1024L
        };
        args.TotalBytesToReceive = null;
        Assert.Null(args.TotalBytesToReceive);
    }

    /// <summary>
    /// Verifies that ProgressPercentage can be set and retrieved.
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(33.33)]
    public void ProgressPercentageCanBeSet(double percentage)
    {
        var args = new DownloadProgressEventArgs { ProgressPercentage = percentage };
        Assert.Equal(percentage, args.ProgressPercentage);
    }

    /// <summary>
    /// Verifies that StatusMessage can be set and retrieved.
    /// </summary>
    [Fact]
    public void StatusMessageCanBeSet()
    {
        var args = new DownloadProgressEventArgs { StatusMessage = "Downloading..." };
        Assert.Equal("Downloading...", args.StatusMessage);
    }

    /// <summary>
    /// Verifies that StatusMessage defaults to empty string, not null.
    /// </summary>
    [Fact]
    public void StatusMessageDefaultsToEmptyString()
    {
        var args = new DownloadProgressEventArgs();
        Assert.NotNull(args.StatusMessage);
        Assert.Equal("", args.StatusMessage);
    }

    /// <summary>
    /// Verifies that DownloadProgressEventArgs inherits from EventArgs.
    /// </summary>
    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new DownloadProgressEventArgs();
        Assert.IsAssignableFrom<EventArgs>(args);
    }

    /// <summary>
    /// Verifies that all properties can be set together.
    /// </summary>
    [Fact]
    public void AllPropertiesCanBeSetTogether()
    {
        var args = new DownloadProgressEventArgs
        {
            BytesReceived = 500,
            TotalBytesToReceive = 1000,
            ProgressPercentage = 50.0,
            StatusMessage = "Half done"
        };

        Assert.Equal(500L, args.BytesReceived);
        Assert.Equal(1000L, args.TotalBytesToReceive);
        Assert.Equal(50.0, args.ProgressPercentage);
        Assert.Equal("Half done", args.StatusMessage);
    }
}
