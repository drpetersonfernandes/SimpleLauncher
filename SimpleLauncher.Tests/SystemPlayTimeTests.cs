using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="SystemPlayTime"/> model for default values and formatted play time display.
/// </summary>
public class SystemPlayTimeTests
{
    /// <summary>
    /// Verifies that a new <see cref="SystemPlayTime"/> has zero play time by default.
    /// </summary>
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var item = new SystemPlayTime { SystemName = "NES" };

        Assert.Equal("NES", item.SystemName);
        Assert.Equal(0, item.PlayTimeSeconds);
    }

    /// <summary>
    /// Verifies that zero seconds formats as 0:00:00.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeZeroSecondsReturnsZeroFormat()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 0 };
        Assert.Equal("0:00:00", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that play time under one hour formats correctly with minutes and seconds.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeUnderOneHour()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 90 }; // 1m 30s
        Assert.Equal("0:01:30", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that exactly 3600 seconds formats as 1:00:00.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeExactlyOneHour()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3600 };
        Assert.Equal("1:00:00", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that play time over one hour formats with hours, minutes, and seconds.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeOverOneHour()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3661 }; // 1h 1m 1s
        Assert.Equal("1:01:01", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that a large play time value formats correctly.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeLargeValue()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 7384 }; // 2h 3m 4s
        Assert.Equal("2:03:04", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that 3599 seconds formats as 0:59:59.
    /// </summary>
    [Fact]
    public void FormattedPlayTime59Minutes59Seconds()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3599 };
        Assert.Equal("0:59:59", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that the <see cref="SystemPlayTime.PlayTimeSeconds"/> property can be updated and the formatted output reflects the change.
    /// </summary>
    [Fact]
    public void PlayTimeSecondsCanBeUpdated()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 100 };
        item.PlayTimeSeconds = 200;

        Assert.Equal(200, item.PlayTimeSeconds);
        Assert.Equal("0:03:20", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that the <see cref="SystemPlayTime.SystemName"/> property can be set during initialization.
    /// </summary>
    [Fact]
    public void SystemNameIsInitOnly()
    {
        var item = new SystemPlayTime { SystemName = "SNES" };
        Assert.Equal("SNES", item.SystemName);
    }

    /// <summary>
    /// Verifies that <see cref="SystemPlayTime.FormattedPlayTime"/> updates dynamically when <see cref="SystemPlayTime.PlayTimeSeconds"/> changes.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeReflectsCurrentPlayTimeSeconds()
    {
        var item = new SystemPlayTime { SystemName = "Genesis", PlayTimeSeconds = 0 };
        Assert.Equal("0:00:00", item.FormattedPlayTime);

        item.PlayTimeSeconds = 120;
        Assert.Equal("0:02:00", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that formatting uses invariant culture regardless of system locale.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeUsesInvariantCulture()
    {
        var item = new SystemPlayTime { SystemName = "PSX", PlayTimeSeconds = 3723 }; // 1h 2m 3s
        Assert.Equal("1:02:03", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that play time exceeding 24 hours formats correctly without wrapping.
    /// </summary>
    [Fact]
    public void FormattedPlayTimeOver24Hours()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 90000 }; // 25h 0m 0s
        Assert.Equal("25:00:00", item.FormattedPlayTime);
    }
}
